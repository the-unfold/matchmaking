[<RequireQualifiedAccess>]
module Login

open Elmish
open Fable.Core.JS
open Fable.Core
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Thoth.Json
open Utils
open Common
open Utils.AsyncResult
// let Async = Microsoft.FSharp.Control.Async



type State = {
    Login: string
    Password: string
    UserInfo: Deferred<Result<User, string>>
}

type Msg = 
    // | LoginTriggered
    | LoginTextChanged of string
    | PasswordTextChanged of string
    | Login of AsyncOperationStatus<Result<ApiToken, string>>
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

        let token = decodeResponseAuto<ApiToken> tokenResponse

        return token
    }
    
let getUser (accessToken: ApiToken) =
    asyncResult {
        let token = sprintf "%O %s" accessToken.TokenType accessToken.AccessToken

        let authUserResponse =
            Http.request "/auth/connect/userinfo"
            |> Http.method GET
            |> Http.header (Headers.authorization token)
            |> Http.send 

        let! authUser = async.Bind (authUserResponse, decodeResponseAuto<AuthUser> >> async.Return)

        let userResponse =
            Http.request (sprintf "/api/user/%s" authUser.Sub)
            |> Http.method GET
            |> Http.header (Headers.authorization token)
            |> Http.send

        let! user = async.Bind (userResponse, decodeResponseAuto<User> >> async.Return)

        return user
    }

let loginCommand (login, password) : Cmd<_> =
    Cmd.OfAsync.either logIn (login, password) (Finished >> Login) raise

let getUserCommand (token: ApiToken) : Cmd<_> =
    Cmd.OfAsync.either getUser token (Finished >> GetUser) raise

// let getUserInfo (accessToken: TokenResponse) =
//     async {
//         let token = sprintf "%O %s" accessToken.TokenType accessToken.AccessToken

//         let! userResponse =
//             Http.request "/auth/connect/userinfo"
//             |> Http.method GET
//             |> Http.header (Headers.authorization token)
//             |> Http.send 
        
//         console.log userResponse
        
//         let authUser = decodeResponse<AuthUser> userResponse

//         // let user = Result.bind (fun au -> )

//         return user
//     }


// let loginAngGetUser (login, password) =
   
//     asyncResult {
//         let! res = logIn (login, password)
        
//         let! ui = getUserInfo res

//         return ui
//     }

// let getUserInfoCommand (token: TokenResponse): Cmd<Msg> =
//     Cmd.OfAsync.either getUserInfo token (Finished >> GetUser) raise

let saveTokenToLocalStorageCommand (token: ApiToken): Cmd<Msg> =
    let tokenJson = Encode.Auto.toString (0, token)
    Browser.WebStorage.localStorage.setItem ("token", tokenJson)
    Cmd.none

let init (): State * Cmd<Msg> =
    let state = { Login = ""; Password = ""; UserInfo = NotStarted }
    let storedToken = 
        match Browser.WebStorage.localStorage.getItem "token" with
        | "" -> Error "token not found"
        | str -> Decode.Auto.fromString<ApiToken> str
    
    match storedToken with
    | Error _ -> state, Cmd.none
    | Ok token -> state, getUserCommand token


let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    // | LoginTriggered -> state, loginCommand (state.Login, state.Password)
    | LoginTextChanged login -> { state with Login = login }, Cmd.none
    | PasswordTextChanged password -> { state with Password = password }, Cmd.none
    | Login Started -> state, loginCommand (state.Login, state.Password)
    | Login (Finished (Ok token)) -> 
        { state with UserInfo = InProgress }, 
        Cmd.batch [getUserCommand token; saveTokenToLocalStorageCommand token]
    | Login (Finished (Error e)) -> state, Cmd.none
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