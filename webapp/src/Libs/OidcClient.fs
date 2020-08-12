module OidcClient

open Fable.Core
open Fable.Core.JS


type Profile =
    abstract iss: string
    abstract sub: string
    abstract aud: string
    abstract exp: string
    abstract iat: string

type User =
    abstract id_token: string
    abstract access_token: string
    abstract token_type: string
    abstract scope: string
    abstract profile: Profile
    abstract expires_at: float
    abstract state: obj

type OidcMetadata =
    abstract issuer: string with get, set
    abstract authorization_endpoint: string with get, set
    abstract token_endpoint: string with get, set
    abstract token_endpoint_auth_methods_supported: string list with get, set
    abstract token_endpoint_auth_alg_values_supported: string list with get, set
    abstract userinfo_endpoint: string with get, set
    abstract check_session_iframe: string with get, set
    abstract end_session_endpoint: string with get, set
    abstract jwks_uri: string with get, set
    abstract registration_endpoint: string with get, set
    abstract scopes_supported: string[] with get, set
    abstract response_types_supported: string[] with get, set
    abstract acr_values_supported: string[] with get, set
    abstract subject_types_supported: string[] with get, set
    abstract userinfo_signing_alg_values_supported: string[] with get, set
    abstract userinfo_encryption_alg_values_supported: string[] with get, set
    abstract userinfo_encryption_enc_values_supported: string[] with get, set
    abstract id_token_signing_alg_values_supported: string[] with get, set
    abstract id_token_encryption_alg_values_supported: string[] with get, set
    abstract id_token_encryption_enc_values_supported: string[] with get, set
    abstract request_object_signing_alg_values_supported: string[] with get, set
    abstract display_values_supported: string[] with get, set
    abstract claim_types_supported: string[] with get, set
    abstract claims_supported: string[] with get, set
    abstract claims_parameter_supported: bool with get, set
    abstract service_documentation: string with get, set
    abstract ui_locales_supported: string[] with get, set

    abstract revocation_endpoint: string with get, set
    abstract introspection_endpoint: string with get, set
    abstract frontchannel_logout_supported: bool with get, set
    abstract frontchannel_logout_session_supported:  bool with get, set
    abstract backchannel_logout_supported: bool with get, set
    abstract backchannel_logout_session_supported: bool with get, set
    abstract grant_types_supported: string[] with get, set
    abstract response_modes_supported: string[] with get, set
    abstract code_challenge_methods_supported: string[] with get, set


type OidcClientSettings =
    abstract metadata: OidcMetadata with get, set
    abstract authority: string with get, set
    abstract client_id: string with get, set
    abstract client_secret: string with get, set
    abstract redirect_uri: string with get, set
    abstract post_logout_redirect_url: string with get, set
    abstract response_type: string with get, set
    abstract scope: string with get, set
    abstract filterProtocolClaims: bool with get, set
    abstract loadUserInfo: bool with get, set

type UserManagerSettings =
    inherit OidcClientSettings

type OidcClient =
    abstract settings: OidcClientSettings

type UserManager =
    inherit OidcClient

    abstract settings: UserManagerSettings

    abstract getUser: unit -> Promise<User>

    abstract signinRedirect: unit -> Promise<obj>
    abstract signinRedirectCallback: unit -> Promise<User>

    abstract signinPopup: unit -> Promise<User>
    abstract signinPopupCallback: unit -> Promise<User>

type UserManagerStatic =
    [<Emit "new $0($1...)">] abstract Create: options: UserManagerSettings -> UserManager