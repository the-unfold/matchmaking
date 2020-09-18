[<RequireQualifiedAccess>]
module Login

open Elmish
open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Thoth.Json
open Utils
open Common
open Utils.AsyncResult

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

type State = {
    Login: string
    Password: string
    LoginAttempt: Deferred<Result<TokenResponse, string>>
    UserInfo: Deferred<Result<User, string>>
}

type Msg = 
    // | LoginTriggered
    | LoginTextChanged of string
    | PasswordTextChanged of string
    | Login of AsyncOperationStatus<Result<TokenResponse, string>>
    | GetUser of AsyncOperationStatus<Result<User, string>>


let a = TokenType ""

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
        let! tokenResponse =
            Http.request "/auth/connect/token" 
            |> Http.method POST
            |> Http.header (Headers.contentType "application/x-www-form-urlencoded")
            |> Http.content (BodyContent.Text data)
            |> Http.send

        console.log tokenResponse

        let token = decodeResponse<TokenResponse> tokenResponse

        return token
    }
    



let loginCommand (login, password) : Cmd<_> =
    Cmd.OfAsync.either logIn (login, password) (Finished >> Login) raise

let getUserInfo (accessToken: TokenResponse) =
    async {
        let token = sprintf "%O %s" accessToken.TokenType accessToken.AccessToken

        let! userResponse =
            Http.request "/auth/connect/userinfo"
            |> Http.method GET
            |> Http.header (Headers.authorization token)
            |> Http.send 
        
        console.log userResponse
        
        let user = decodeResponse<User> userResponse

        return user
    }


// let loginAngGetUser (login, password) =
   
//     asyncResult {
//         let! res = logIn (login, password)
        
//         let! ui = getUserInfo res

//         return ui
//     }

let getUserInfoCommand (token: TokenResponse): Cmd<Msg> =
    Cmd.OfAsync.either getUserInfo token (Finished >> GetUser) raise

let saveTokenToLocalStorageCommand (token: TokenResponse): Cmd<Msg> =
    let tokenJson = Encode.Auto.toString (0, token)
    Browser.WebStorage.localStorage.setItem ("token", tokenJson)
    Cmd.none

let init (): State * Cmd<Msg> =
    let state = { Login = ""; Password = ""; LoginAttempt = NotStarted; UserInfo = NotStarted }
    let storedToken = 
        match Browser.WebStorage.localStorage.getItem "token" with
        | "" -> Error "token not found"
        | str -> Decode.Auto.fromString<TokenResponse> str
    
    match storedToken with
    | Error _ -> state, Cmd.none
    | Ok token -> state, getUserInfoCommand token


let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    // | LoginTriggered -> state, loginCommand (state.Login, state.Password)
    | LoginTextChanged login -> { state with Login = login }, Cmd.none
    | PasswordTextChanged password -> { state with Password = password }, Cmd.none
    | Login Started -> state, loginCommand (state.Login, state.Password)
    | Login (Finished (Ok token)) -> 
        { state with LoginAttempt = Resolved (Ok token); UserInfo = InProgress }, 
        Cmd.batch [getUserInfoCommand token; saveTokenToLocalStorageCommand token]
    | Login (Finished (Error e)) -> { state with LoginAttempt = Resolved (Error e) }, Cmd.none
    | GetUser Started -> state, Cmd.none
    | GetUser (Finished result) -> { state with UserInfo = Resolved result}, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    div [] [
        div [Class "row mb-md"] [
            div [Class "column mr-md"] [
                label [ Class "label-std mb-md" ] [ str "Login" ]
                input [ Class "input-std"; OnChange (fun e -> e.Value |> LoginTextChanged |> dispatch) ]
            ]
            div [Class "column mr-md"] [
                label [ Class "label-std mb-md" ] [ str "Password" ]
                input [ 
                    Class "input-std"
                    Type "password" 
                    OnChange (fun e -> e.Value |> PasswordTextChanged |> dispatch)
                ]
            ]
        ]
        div [] [
            button [ Class "btn-std"; OnClick (fun _ -> dispatch (Login Started))] [ str "Login" ]
        ]
    ]