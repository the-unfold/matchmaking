module Api.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Microsoft.IdentityModel.Tokens
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Npgsql.FSharp
open Thoth.Json.Net
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open System.Data.Common;
open Npgsql

type User = {
    id: int
    auth_id: int
    name: string
}

type Tag = {
    id: int
    name: string
    slug: string
}

type Event = {
    id: int
    title: string
    description: string
    image_url: string option
    location_geo: string option
    location_url: string option
    date_time: System.DateTime
    tags: Tag list
    organizers: User list
    attendees: User list
}

let connection =
    Sql.host "postgis"
    |> Sql.port 5432
    |> Sql.username "docker"
    |> Sql.password "docker"
    |> Sql.database "gis"

let pgConnectionString = Sql.formatConnectionString connection

let getUsers() = 
    use conn = new NpgsqlConnection(pgConnectionString)
    conn.Open()

    select {
        table "users"
    } |> conn.SelectAsync<User>

type EventsRow = {
    id: int
    title: string
    description: string
    image_url: string option
    location_geo: string option
    location_url: string option
    date_time: System.DateTime
}

type TagsRow = {
    id: int
    name: string
    slug: string
}

type EventTagsRow = {
    event_id: int
    tag_id: int
}

let rowToEvent (r: EventsRow): Event = 
     {
        id = r.id
        title = r.title
        description = r.description
        image_url = r.image_url
        location_geo = r.location_geo
        location_url = r.location_url
        date_time = r.date_time
        tags = []
        organizers = []
        attendees = [] 
    }

let rowToTag (r: TagsRow): Tag =
    {
        id = r.id
        name = r.name
        slug = r.slug
    }

let mkTupleFlip a b = 
    (b, a)

let getEvents () =
    use conn = new NpgsqlConnection(pgConnectionString)
    conn.Open()

    task {
        let! rows = 
            select {
                table "events"
                leftJoin "event_tags" "event_id" "events.id"
                leftJoin "tags" "id" "event_tags.tag_id"
                orderBy "events.id" Asc
            } |> conn.SelectAsync<EventsRow, EventTagsRow, TagsRow>

        let events = 
            rows 
            |> Seq.fold (fun result (e, _, t) -> 
                match result with 
                | [] -> [rowToEvent e]
                | x::xs when x.id = e.id -> {x with tags = rowToTag t :: x.tags} :: xs
                | xs -> rowToEvent e :: xs) []
            |> Seq.toList

        return events
    }

let getUserByAuth authId =
    connection
    |> Sql.connectFromConfig
    |> Sql.query "SELECT id, auth_id, name FROM users WHERE auth_id = @auth_id"
    |> Sql.parameters [
        ("auth_id", Sql.int authId)]
    |> Sql.executeRow (fun read -> {
        id = read.int "id"
        auth_id = read.int "auth_id"
        name = read.text "name"
    })

let getTags query=
    connection
    |> Sql.connectFromConfig
    |> Sql.query "SELECT name FROM tags WHERE name ILIKE @query;"
    |> Sql.parameters [
        ("query", Sql.string (sprintf "%%%s%%" query))]
    |> Sql.execute (fun read -> read.string "name")


let handleGetHello: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let response = "Hello there!"
            return! Giraffe.ResponseWriters.json response next ctx
        }

let handleGetHelloApi: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let response = "Hello Api!"
            return! Giraffe.ResponseWriters.json response next ctx
        }

let mkTuple a b =
    (a, b)

let handleGetUsers: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! users = getUsers();
            let payload = users |> Seq.toList |> mkTuple 2 |> Encode.Auto.toString<User list>

            return! Giraffe.ResponseWriters.text payload next ctx
        }

let handleGetUserByAuth authId: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let user = 
                match getUserByAuth authId with
                | Result.Ok u -> u
                | Result.Error e -> raise e

            return! Giraffe.ResponseWriters.json user next ctx
        }

let handleGetTags query: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let tags =
                match getTags query with
                | Ok tags -> tags
                | Result.Error e -> raise e

            return! Giraffe.ResponseWriters.json tags next ctx
        }

let handleGetEvents: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> 
        task {
            let! events = getEvents()
            
            let payload = Encode.Auto.toString<Event list> (2, events)

            return! Giraffe.ResponseWriters.text payload next ctx
        }

let handleGetSecured: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        text "Secured hello!" next ctx

// ---------------------------------
// Web app
// ---------------------------------

let authorize =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> handleGetHello
                route "/api" >=> handleGetHelloApi
                routef "/tags/%s" handleGetTags
                route "/events" >=> handleGetEvents
                // routef "/find-users/%f-%f-%f" handleFindUsers
            ]
        authorize >=>
            GET >=>
                route "/secured" >=> handleGetSecured
                route "/users" >=> handleGetUsers
                routef "/user/%i" handleGetUserByAuth

        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (context: WebHostBuilderContext) (app : IApplicationBuilder) =
    (match context.HostingEnvironment.IsDevelopment () with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseAuthentication()
        .UseAuthorization()
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(fun options ->
            options.RequireHttpsMetadata <- false // need to figure out the correct way to set up ssl to remove this
            options.Authority <- "http://auth:5000" // and use https here
            options.TokenValidationParameters <- TokenValidationParameters(
                ValidateAudience = false
            )    
        ) |> ignore

    services.AddAuthorization(fun options -> 
        options.AddPolicy("api", fun policy -> 
            policy.RequireAuthenticatedUser () |> ignore
            policy.RequireClaim("scope", "api") |> ignore
        )
    ) |> ignore

    services.AddCors()
        .AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Information)
           .AddConsole()
           .AddDebug() |> ignore



[<EntryPoint>]
let main _ =
    Dapper.FSharp.OptionTypes.register();

    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging) |> ignore)
        .Build()
        .Run()
    0