[<RequireQualifiedAccess>]
module Login

open Browser.Dom
open Fable.Core
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Elmish

open Utils

type State = {
    Login: string
    Password: string
}

type Msg = 
    | LoginTriggered
    | LoginTextChanged of string
    | PasswordTextChanged of string

let init () =
    { Login = ""; Password = "" }, Cmd.none

let loginCommand (login, password) =
    Cmd.OfFunc.attempt (fun a -> console.log a) (login, password) (fun _ -> invalidOp "login failed")

let update (msg: Msg) (state: State) =
    match msg with
    | LoginTriggered -> state, loginCommand (state.Login, state.Password)
    | LoginTextChanged login -> { state with Login = login }, Cmd.none
    | PasswordTextChanged password -> { state with Password = password }, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    div [] [
        div [] [
            label [] [ str "Login" ]
            input [ OnChange (fun e -> e.Value |> LoginTextChanged |> dispatch) ]
        ]
        div [] [
            label [] [ str "Password" ]
            input [ 
                Type "password" 
                OnChange (fun e -> e.Value |> PasswordTextChanged |> dispatch)
            ]
        ]
        button [ OnClick (fun _ -> dispatch LoginTriggered)] [ str "Login" ]
    ]