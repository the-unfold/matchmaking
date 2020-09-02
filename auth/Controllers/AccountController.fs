namespace Auth.Controllers

open Auth.Models
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Identity
open System
open System.Threading

type AccountController(signInManager: SignInManager<IdentityUser>, userManager: UserManager<IdentityUser>) =
    inherit Controller()

    member this._signInManager = signInManager
    member this._userManager = userManager

    [<HttpGet>]
    member this.Login(returnUrl: string) =
        async {
            let model = LoginModel(ReturnUrl = returnUrl)
            return this.View(model)
        } |> Async.StartAsTask

    [<HttpPost>]
    member this.Login(model: LoginModel, cancellationToken: CancellationToken) =
        Console.WriteLine(sprintf "Login: %s, Password: %s" model.Login model.Password)
        async {
            if (not this.ModelState.IsValid)
            then 
                return this.View(model) :> ActionResult
            else 
                
                let! user = this._userManager.FindByNameAsync(model.Login) |> Async.AwaitTask

                Console.WriteLine (user.ToString())

                let! result = this._signInManager.PasswordSignInAsync(user, model.Password, false, false)  |> Async.AwaitTask

                if result.Succeeded 
                then 
                    return this.Redirect(model.ReturnUrl) :> ActionResult
                else 
                    return this.View(model) :> ActionResult

        } |> Async.StartAsTask