namespace Auth.Models

open System.ComponentModel.DataAnnotations

type LoginModel()=
    let mutable login = ""
    let mutable password = ""
    let mutable returnUrl = ""

    [<Required>]
    member this.Login with get() = login and set(value) = login <- value
    [<Required>]
    member this.Password with get() = password and set(value) = password <- value
    [<Required>]
    member this.ReturnUrl with get() = returnUrl and set(value) = returnUrl <- value