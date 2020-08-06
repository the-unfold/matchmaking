namespace Auth.Controllers

open Microsoft.AspNetCore.Mvc

type LoginController() =
    inherit Controller()

    member this.Index () =
        this.View()