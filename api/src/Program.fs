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
// open Chiron
// let (<!>) = Chiron.Operators.(<!>)

// type Position = {
//     Lat: float
//     Lon: float
//     } with 

//     static member FromJson (_ : Position) = 
//             fun (lat, lon) -> { Lon = lon; Lat = lat }
//             <!> Json.read "coordinates"


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

// let parsePosition json: Position =
//     (Json.parse >> Json.deserialize) json

let connection =
    Sql.host "postgis"
    |> Sql.port 5432
    |> Sql.username "docker"
    |> Sql.password "docker"
    |> Sql.database "gis"

let getUsers() =
    connection
    |> Sql.connectFromConfig
    |> Sql.query "SELECT id, auth_id, name FROM users"
    |> Sql.execute (fun read -> {
        id = read.int "id"
        auth_id = read.int "auth_id"
        name = read.text "name"
    })

type GetEventsRow = {
    id: int
    title: string
    description: string
    image_url: string option
    location_geo: string option
    location_url: string option
    date_time: System.DateTime
    tag_id: int
    tag_name: string
    tag_slug: string
}

let eventFromRow r =
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

let tagFromRow r =
    {
        id = r.tag_id
        name = r.tag_name
        slug = r.tag_slug
    }

let getEvents () =
    connection
    |> Sql.connectFromConfig
    |> Sql.query 
        @"SELECT e.id, e.title, e.description, e.image_url, e.location_geo, e.location_url, e.date_time, t.id as tag_id, t.name as tag_name, t.slug as tag_slug
         FROM events as e
         JOIN event_tags ON e.id = event_tags.event_id
         JOIN tags as t ON t.id = event_tags.tag_id "
    |> Sql.execute (fun read -> {
        id = read.int "id"
        title = read.string "title"
        description = read.string "description"
        image_url = read.stringOrNone "image_url"
        location_geo = read.stringOrNone "location_geo"
        location_url = read.stringOrNone "location_url"
        date_time = read.dateTime "date_time"
        tag_id = read.int "tag_id"
        tag_name = read.string "tag_name"
        tag_slug = read.string "tag_slug"
    })
    |> Result.map (List.fold (fun result r -> 
        match result with
        | [] -> [eventFromRow r]
        | e::xs when e.id = r.id -> 
            {e with tags = tagFromRow r :: e.tags} :: xs
        | e::xs -> 
            eventFromRow r :: xs
        ) [])

    
    // |> Sql.query "SELECT id, name, ST_AsGeoJson(location) as loc, radius FROM users"
    // |> Sql.execute (fun read -> {
    //     Id = read.int "id"
    //     Name = read.text "name"
    //     Location =  "loc" |> read.text |> parsePosition
    //     Radius = read.double "radius"
    // })

// let getUsersInArea (lon: float) (lat: float) (rad: float) =
//     connection
//     |> Sql.connectFromConfig
//     |> Sql.query "SELECT name FROM users WHERE ST_Intersects(
//         st_buffer(ST_GeographyFromText(@point), @rad), 
//         st_buffer(users.location, users.radius))"
//     |> Sql.parameters [
//         ("point", Sql.string (sprintf "POINT(%f %f)" lon lat))
//         ("rad", Sql.double rad) ]
//     |> Sql.execute (fun read -> read.string "name")

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

let handleGetUsers: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let users = 
                match getUsers() with                 
                | Result.Ok users -> users
                | Result.Error e -> raise e

            return! Giraffe.ResponseWriters.json users next ctx
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
// let handleFindUsers (lon: float, lat: float, rad: float): HttpHandler =
//     fun (next: HttpFunc) (ctx: HttpContext) ->
//         task {
//             let users = 
//                 match getUsersInArea lon lat rad with
//                 | Result.Ok us -> us
//                 | Result.Error e -> raise e

//             return! Giraffe.ResponseWriters.json users next ctx
//         }

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
            let events =
                match getEvents () with
                | Ok events -> events
                | Result.Error e -> raise e
            
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
                // routef "/find-users/%f-%f-%f" handleFindUsers
            ]
        authorize >=>
            GET >=>
                route "/secured" >=> handleGetSecured
                route "/users" >=> handleGetUsers
                route "/events" >=> handleGetEvents
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
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging) |> ignore)
        .Build()
        .Run()
    0