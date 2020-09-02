namespace Auth

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Identity
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.DependencyInjection
open IdentityServer4.Models
open Auth.Data

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
            ApiResource("api", "Main backend API")
        ]
    

    let getIdentityResources () =
        [
            IdentityResources.OpenId () :> IdentityResource
            IdentityResources.Profile () :> IdentityResource

            IdentityResources.Email () :> IdentityResource
        ]

    let getApiScopes () =
        [
            ApiScope("api")
        ]

module Program =

    let configureServices (services: IServiceCollection): unit =
        
        services.AddDbContext<AuthDbContext>(
            fun config -> 
            config.UseInMemoryDatabase("MEMORY") |> ignore
        ).AddIdentity<IdentityUser, IdentityRole>(
            fun config ->
            config.Password.RequireDigit <- false
            config.Password.RequireLowercase <- false
            config.Password.RequireNonAlphanumeric <- false
            config.Password.RequireUppercase <- false
            config.Password.RequiredLength <- 4
        ).AddEntityFrameworkStores<AuthDbContext>() |> ignore

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
        let host = 
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(
                fun webBuilder -> 
                webBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices) |> ignore
            ).Build()
        
        using (host.Services.CreateScope()) (
            fun scope -> 
            DatabaseInitializer.Init scope.ServiceProvider
        )

        host.Run()
        0
