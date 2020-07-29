namespace Auth

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open IdentityServer4.Models

module Configuration =
    let getClients () =
        [
            Client(
                ClientId = "client_id",
                ClientSecrets = [| Secret("client_secret".Sha256()) |],
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = [| "api" |]
            )
            Client(
                ClientId = "client_id_app",
                ClientSecrets = [| Secret("client_secret_app".Sha256()) |],
                AllowedGrantTypes = GrantTypes.Code,
                AllowedScopes = [| "api" |]
            )
        ]

    let getApiResources () =
        [
            ApiResource("api", "Main bakcend API")
        ]
    

    let getIdentityResources () =
        [
            IdentityResources.OpenId () :> IdentityResource
            IdentityResources.Profile () :> IdentityResource
        ]

    let getApiScopes () =
        [
            ApiScope("api")
        ]

module Program =

    let configureServices (services: IServiceCollection): unit =
        services.AddIdentityServer()
            .AddInMemoryApiScopes(Configuration.getApiScopes())
            .AddInMemoryApiResources(Configuration.getApiResources())
            .AddInMemoryClients(Configuration.getClients())
            .AddInMemoryIdentityResources(Configuration.getIdentityResources())
            .AddDeveloperSigningCredential()
            |> ignore


        ()

    let configureApp (context: WebHostBuilderContext) (app: IApplicationBuilder): unit =
        app.UseIdentityServer() |> ignore
        ()

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
