namespace Auth

open System
open System.Security.Claims

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open IdentityServer4
open IdentityServer4.Test
open IdentityServer4.Models
open Giraffe

module TestUsers = 
    let getUsers() =
        seq {
            TestUser(
                SubjectId = "505",
                Username = "test",
                Password = "test",
                Claims =
                    [| Claim(IdentityModel.JwtClaimTypes.Name, "Test User")
                       Claim(IdentityModel.JwtClaimTypes.GivenName, "User")
                       Claim(IdentityModel.JwtClaimTypes.FamilyName, "Test")
                       Claim(IdentityModel.JwtClaimTypes.Email, "user@test.com")
                       Claim(IdentityModel.JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean)
                       Claim(IdentityModel.JwtClaimTypes.WebSite, "https://github.com/") |]
                )
        } |> ResizeArray

module Configuration =
    let getClients(): seq<Client> =
        seq {
            Client(
                ClientId = "client_id",
                ClientSecrets = [| Secret("secret".Sha256()) |],
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = [| "api" |]
            )
            Client(
                ClientId = "webapp",
                ClientSecrets = [| Secret("secret".Sha256()) |],
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = [| "api" |]
            )
        }

    let getIdentityResources(): seq<IdentityResource> =
        seq {
            IdentityResources.OpenId()
            IdentityResources.Profile()
        }

    let getApiScopes(): seq<ApiScope> =
        seq {
            ApiScope("api", [| IdentityModel.JwtClaimTypes.Email |])
        }

module Web = 
    let app : HttpFunc -> HttpContext -> HttpFuncResult =
        choose [
            GET >=>
                choose [
                    route "/" >=> text "idSrv"
                ]
            setStatusCode 404 >=> text "Not Found" ]

    let errorHandler (ex : Exception) (logger : ILogger) =
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message

module Program =
    let configureServices (services: IServiceCollection) =
        services.AddIdentityServer()
            .AddDeveloperSigningCredential()
            .AddInMemoryApiScopes(Configuration.getApiScopes())
            .AddInMemoryClients(Configuration.getClients())
            .AddInMemoryIdentityResources(Configuration.getIdentityResources())
            .AddTestUsers(TestUsers.getUsers())
            |> ignore
        services.AddGiraffe() |> ignore

    let configureApp (context: WebHostBuilderContext) (app: IApplicationBuilder) =
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseIdentityServer() |> ignore
        app.UseGiraffe(Web.app) |> ignore

    [<EntryPoint>]
    let main args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder -> 
                webBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices) |> ignore)
            .Build()
            .Run()
        
        0
