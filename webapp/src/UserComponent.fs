[<RequireQualifiedAccess>]
module UserComponent

open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Common
open Utils

type Props = {
    User: User
}

let view (props: Props) =
    let userImage = 
        match props.User.Image with
        | Some img -> img
        | None -> Image.load "./assets/user-image-placeholder.jpg"

    div [Class "row align-center"] [
        img [Class "user-image mr-lg"; Src userImage]
        span [Class "text-std"] [str props.User.Name]
    ]