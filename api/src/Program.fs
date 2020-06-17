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

let handleGetHello =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let response = "Hello!"
            return! json response next ctx
        }

// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> handleGetHello
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