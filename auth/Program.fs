namespace Auth

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Mvc
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
                ClientId = "webapp",
                // ClientSecrets = [| Secret("client_secret_app".Sha256()) |],
                RedirectUris = [|"/api/"|],
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = [| "openid"; "profile"; "api" |],
                AllowAccessTokensViaBrowser = true
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
        // services.AddMvc() |> ignore
        // services.AddControllersWithViews() |> ignore
        services.AddControllersWithViews().AddRazorRuntimeCompilation() |> ignore
        services.AddRazorPages() |> ignore
        ()

    let configureApp (context: WebHostBuilderContext) (app: IApplicationBuilder): unit =
        app.UseRouting() |> ignore
        app.UseIdentityServer() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseEndpoints(fun endpoints -> 
            endpoints.MapDefaultControllerRoute() |> ignore
            endpoints.MapRazorPages() |> ignore
        ) |> ignore
        // app.UseMvc() |> ignore
        // app.UseMvcWithDefaultRoute() |> ignore
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
