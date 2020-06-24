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
// open FSharp.Data.Sql

// [<Literal>]
// let connectionString = @"Host=192.168.99.100;Database=prototype_postgis;Username=docker;Password=docker"

// type sql = SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL, connectionString>

type User = {
    Id: int
    Name: string
}

let connection =
    Sql.host "192.168.99.100"
    |> Sql.port 5433
    |> Sql.username "docker"
    |> Sql.password "docker"
    |> Sql.database "prototype_postgis"

let getUsers() =
    connection
    |> Sql.connectFromConfig
    |> Sql.query "SELECT id, name FROM users"
    |> Sql.execute (fun read -> {
        Id = read.int "id"
        Name = read.text "name"
    })

let handleGetHello =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let response = "Hello!"
            return! json response next ctx
        }

let handleGetUsers =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let users = 
                match getUsers() with 
                | Ok users -> users
                | Error e -> invalidOp "failed to get users"

            return! json users next ctx
        }

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> handleGetHello
                route "/users" >=> handleGetUsers
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
    builder.WithOrigins("http://localhost:8080")
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
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
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
    // let contentRoot = Directory.GetCurrentDirectory()
    // let webRoot     = Path.Combine(contentRoot, "WebRoot")
    // WebHostBuilder()
    //     .UseKestrel()
    //     .UseContentRoot(contentRoot)
    //     .UseIISIntegration()
    //     .UseWebRoot(webRoot)
    //     .Configure(Action<IApplicationBuilder> configureApp)
    //     .ConfigureServices(configureServices)
    //     .ConfigureLogging(configureLogging)
    //     .Build()
    //     .Run()
    // 0