namespace WebApp

open Elmish
open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Thoth.Json
open Utils

[<RequireQualifiedAccess>]
module Login = 
    type TokenType =
        TokenType of string

    type TokenScope =
        TokenScope of string

    type TokenResponse = {
        AccessToken: string
        ExpiresIn: int
        TokenType: string
        Scope: string
    }

    type UserClaims = {
        Sub: string
        Name: string
        Email: string
    }

    type State = {
        Login: string
        Password: string
        LoginAttempt: Deferred<Result<TokenResponse, string>>
        UserInfo: Deferred<Result<UserClaims, string>>
    }

    type Msg = 
        // | LoginTriggered
        | LoginTextChanged of string
        | PasswordTextChanged of string
        | Login of AsyncOperationStatus<Result<TokenResponse, string>>
        | GetUser of AsyncOperationStatus<Result<UserClaims, string>>
    


    let init () =
        { Login = ""; Password = ""; LoginAttempt = NotStarted; UserInfo = NotStarted }, Cmd.none

    let logIn (login, password) =
        let data = 
            {| username = login
               password = password
               grant_type = "password"
               client_id = "webapp"
               client_secret = "secret"
               scope = "api openid profile email" |}
            |> Qs.queryString.stringify

        async {
            let! response =
                Http.request "/auth/connect/token" 
                |> Http.method POST
                |> Http.header (Headers.contentType "application/x-www-form-urlencoded")
                |> Http.content (BodyContent.Text data)
                |> Http.send

            console.log response

            return response
        }

    let loginSuccess response =
        match response.statusCode with
        | 200 -> Login (Finished (Decode.Auto.fromString<TokenResponse> (response.responseText, caseStrategy=SnakeCase)))
        | _ -> Login (Finished (Error response.responseText))

    let loginCommand (login, password) : Cmd<_> =
        Cmd.OfAsync.either logIn (login, password) loginSuccess raise

    let getUserInfo (accessToken: TokenResponse) =
        async {
            let token = sprintf "%O %s" accessToken.TokenType accessToken.AccessToken
            let! response =
                Http.request "/auth/connect/userinfo"
                |> Http.method GET
                |> Http.header (Headers.authorization token)
                |> Http.send
            
            console.log response
            
            return response
        }

    let getUserInfoSuccess response =
        match response.statusCode with
        | 200 -> GetUser (Finished (Decode.Auto.fromString<UserClaims> (response.responseText, caseStrategy=SnakeCase)))
        | _ -> GetUser (Finished (Error response.responseText))

    let getUserInfoCommand (token): Cmd<_> =
        Cmd.OfAsync.either getUserInfo token getUserInfoSuccess raise


    let update (msg: Msg) (state: State) =
        match msg with
        // | LoginTriggered -> state, loginCommand (state.Login, state.Password)
        | LoginTextChanged login -> { state with Login = login }, Cmd.none
        | PasswordTextChanged password -> { state with Password = password }, Cmd.none
        | Login Started -> state, loginCommand (state.Login, state.Password)
        | Login (Finished (Ok token)) -> 
            { state with LoginAttempt = Resolved (Ok token); UserInfo = InProgress }, 
            getUserInfoCommand token
        | Login (Finished (Error e)) -> { state with LoginAttempt = Resolved (Error e) }, Cmd.none
        | GetUser Started -> state, Cmd.none
        | GetUser (Finished result) -> { state with UserInfo = Resolved result}, Cmd.none

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
            button [ OnClick (fun _ -> dispatch (Login Started))] [ str "Login" ]
        ]