module Api.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Npgsql.FSharp
open Chiron
let (<!>) = Chiron.Operators.(<!>)

type Position = {
    Lat: float
    Lon: float
    } with 

    static member FromJson (_ : Position) = 
            fun (lat, lon) -> { Lon = lon; Lat = lat }
            <!> Json.read "coordinates"


type User = {
    Id: int
    Name: string
    Location: Position
    Radius: float
}

let parsePosition json: Position =
    (Json.parse >> Json.deserialize) json

let connection =
    Sql.host "postgis"
    |> Sql.port 5432
    |> Sql.username "docker"
    |> Sql.password "docker"
    |> Sql.database "gis"

let getUsers() =
    connection
    |> Sql.connectFromConfig
    |> Sql.query "SELECT id, name, ST_AsGeoJson(location) as loc, radius FROM users"
    |> Sql.execute (fun read -> {
        Id = read.int "id"
        Name = read.text "name"
        Location =  "loc" |> read.text |> parsePosition
        Radius = read.double "radius"
    })

let getUsersInArea (lon: float) (lat: float) (rad: float) =
    connection
    |> Sql.connectFromConfig
    |> Sql.query "SELECT name FROM users WHERE ST_Intersects(
        st_buffer(ST_GeographyFromText(@point), @rad), 
        st_buffer(users.location, users.radius))"
    |> Sql.parameters [
        ("point", Sql.string (sprintf "POINT(%f %f)" lon lat))
        ("rad", Sql.double rad) ]
    |> Sql.execute (fun read -> read.string "name")

let handleGetHello =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let response = "Hello!"
            return! Giraffe.ResponseWriters.json response next ctx
        }

let handleGetHelloApi =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let response = "Hello Api!"
            return! Giraffe.ResponseWriters.json response next ctx
        }

let handleGetUsers =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let users = 
                match getUsers() with                 
                | Result.Ok users -> users
                | Result.Error e -> raise e

            return! Giraffe.ResponseWriters.json users next ctx
        }

let handleFindUsers (lon: float, lat: float, rad: float): HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let users = 
                match getUsersInArea lon lat rad with
                | Result.Ok us -> us
                | Result.Error e -> raise e

            return! Giraffe.ResponseWriters.json users next ctx
        }

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> handleGetHello
                route "/api" >=> handleGetHelloApi
                route "/users" >=> handleGetUsers
                routef "/find-users/%f-%f-%f" handleFindUsers
            ]
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
    builder.WithOrigins("")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (context: WebHostBuilderContext) (app : IApplicationBuilder) =
    (match context.HostingEnvironment.IsDevelopment () with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

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