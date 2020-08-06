namespace Auth.Controllers

open Auth.Models
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Identity
open System

type AccountController(signInManager: SignInManager<IdentityUser>, userManager: UserManager<IdentityUser>) =
    inherit Controller()

    member this._signInManager = signInManager
    member this._userManager = userManager

    [<HttpGet>]
    member this.Login(returnUrl: string) =
        async {
            let model = LoginModel()
            return this.View(model)
        } |> Async.StartAsTask

    [<HttpPost>]
    member this.Login(model: LoginModel) =
        Console.WriteLine(sprintf "Login: %s, Password: %s, ReturnUrl: %s" model.Login model.Password model.ReturnUrl)
        this.View()
    