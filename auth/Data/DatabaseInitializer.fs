namespace Auth.Data

module DatabaseInitializer =

    open Microsoft.AspNetCore.Identity
    open Microsoft.Extensions.DependencyInjection
    open System
    open System.Security.Claims


    let Init: IServiceProvider -> unit = 
        fun sp -> 

        let userManager = sp.GetService<UserManager<IdentityUser>>()

        let user = 
            IdentityUser (
                UserName = "user"
            )
       
        let result = userManager.CreateAsync(user, "pass").GetAwaiter().GetResult()

        if result.Succeeded 
        then 
            Console.WriteLine "Test user added successfuly"
            userManager.AddClaimAsync(user, Claim(ClaimTypes.Role, "Administrator")).GetAwaiter().GetResult() |> ignore
        else Console.WriteLine "Failed to add user!"