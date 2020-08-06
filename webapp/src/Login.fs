[<RequireQualifiedAccess>]
module Login

open Browser.Dom
open Fable
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Elmish

open Utils
open OidcClient

[<Import("UserManager", "oidc-client")>]
let userManagerStatic: UserManagerStatic = jsNative

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

let oidcMetadata = jsOptions<OidcMetadata>(fun x ->
    x.issuer <- "/auth/"
    x.authorization_endpoint <- "/auth/connect/authorize"
    x.token_endpoint <- "/auth/connect/token"
    x.userinfo_endpoint <- "/auth/connect/userinfo"
    x.end_session_endpoint <- "/auth/"
    x.check_session_iframe <- "/auth/connect/checksession"
    x.revocation_endpoint <- "/auth/connect/revocation"
    x.scopes_supported <- [|"openid"; "profile"; "api"; "offline_access"|]
    x.claims_supported <- [|"sub" ; "name" ; "family_name"; "given_name"; "middle_name"; "nickname"; "preferred_username"; "profile"; "picture"; "website"; "gender"; "birthdate"; "zoneinfo"; "locale"; "updated_at"|]
    x.grant_types_supported <- [|"authorization_code"; "client_credentials"; "refresh_token"; "implicit"|]
    x.response_types_supported <- [|"code"; "token"; "id_token"; "id_token token"; "code id_token"; "code token"; "code id_token token"|]
    x.jwks_uri <- "/auth/.well-known/openid-configuration/jwks"
)


let userManagerSettings = jsOptions<UserManagerSettings>(fun x -> 
    x.authority <- "/auth/"
    x.client_id <- "webapp"
    x.redirect_uri <- "/api/"
    x.post_logout_redirect_url <- "/"
    x.response_type <- "id_token token"
    x.scope <- "openid profile api"
    x.filterProtocolClaims <- true
    x.loadUserInfo <- true
    x.metadata <- oidcMetadata
)
let userManager = userManagerStatic.Create userManagerSettings

let loginCommand (login, password) =
    Cmd.OfFunc.attempt 
        (fun a -> 
            console.log a
            userManager.getUser().``then`` (fun a -> console.log a) |> ignore
            // userManager.signinRedirect() |> ignore
            userManager.signinPopup() |> ignore
        ) 
        (login, password) 
        (fun _ -> invalidOp "login failed")

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