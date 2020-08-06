namespace Auth.Models

open System.ComponentModel.DataAnnotations

type LoginModel()=
    [<Required>]
    member this.Login = "admin"
    [<Required>]
    member this.Password = "password"
    [<Required>]
    member this.ReturnUrl = "return url"