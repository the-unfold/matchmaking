namespace WebApp

open Elmish
open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp

[<RequireQualifiedAccess>]
module Login = 
    type State = 
        { Login: string
          Password: string }

    type Msg = 
        | LoginTriggered
        | LoginTextChanged of string
        | PasswordTextChanged of string

    let init () =
        { Login = ""; Password = "" }, Cmd.none

    let logIn (login, password) =
        let data = 
            {| username = login
               password = password
               grant_type = "password"
               client_id = "webapp"
               client_secret = "secret"
               scope = "api" |}
            |> Qs.queryString.stringify

        async {
            let! response =
                Http.request "http://localhost/auth/connect/token" 
                |> Http.method POST
                |> Http.header (Headers.contentType "application/x-www-form-urlencoded")
                |> Http.content (BodyContent.Text data)
                |> Http.send

            let result = response.statusCode, response.responseText
            console.log result

            return result
        }

    let loginCommand (login, password) : Cmd<_> =
        Cmd.OfAsync.attempt 
            logIn (login, password) 
            raise

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