namespace WebApp

module App =

    open Fable.Core
    open Fable.Core.JsInterop
    open Browser.Dom
    open Fable.React
    open Fable.React.Props
    open Elmish
    open Elmish.React
    open Fetch
    open Thoth.Json 

    open Utils
    open Common

    // importing OpenLayers js library using F# type mappings
    importAll "ol/ol.css"

    [<RequireQualifiedAccess>]
    type Page =
        | Login of Login.State
        | Map of UserMap.State
        | Event of Event.State

    type State = {
        CurrentPage: Page
        Navbar: Navbar.State option
        User: User option
    }

    type Msg =
        | LoginMsg of Login.Msg
        | UserMapMsg of UserMap.Msg
        | EventMsg of Event.Msg
        | NavbarMsg of Navbar.Msg

    let init (): State * Cmd<Msg> = 
        let loginState, loginCmd = Login.init ()

        let initialState = {
            CurrentPage = Page.Login loginState 
            Navbar = None
            User = None
        }

        let initialCmd = Cmd.batch [
            Cmd.map LoginMsg loginCmd
        ]

        initialState, initialCmd

    let findUsersSub area =
        (fetch (sprintf "/api/find-users/%f-%f-%f" area.Center.Lat area.Center.Lon (area.Radius * 1000.0)) [])
        |> Promise.bind (fun r -> r.text())
        |> Promise.map (fun txt -> Decode.fromString (Decode.list Decode.string) txt)



    let update (msg: Msg) (state: State): State * Cmd<Msg> =
        match msg, state.CurrentPage with
        | LoginMsg loginMsg, Page.Login loginState -> 
            match loginMsg with
            | Login.GetUser (Finished (Ok user)) -> 
                let userMapState, userMapCmd = UserMap.init user
                let navbarState, navbarCmd = Navbar.init user
                { state with 
                    User = Some user
                    CurrentPage = Page.Map userMapState
                    Navbar = Some navbarState }, 
                Cmd.batch [
                    Cmd.map UserMapMsg userMapCmd; 
                    Cmd.map NavbarMsg navbarCmd]
            | loginMsg -> 
                let loginState, loginCmd = Login.update loginMsg loginState
                { state with CurrentPage = Page.Login loginState}, Cmd.map LoginMsg loginCmd

        | UserMapMsg userMapMsg, Page.Map userMapState -> 
            let userMapState, userMapCmd = UserMap.update userMapMsg userMapState 
            { state with CurrentPage = Page.Map userMapState}, Cmd.map UserMapMsg userMapCmd

        | EventMsg eventMsg, Page.Event eventState ->
            let nextEventState, eventCmd = Event.update eventMsg eventState
            { state with CurrentPage = Page.Event nextEventState }, Cmd.map EventMsg eventCmd

        | NavbarMsg navbarMsg, _ -> 
            match navbarMsg, state.User with
            | Navbar.EventsNavTriggered, Some user -> 
                let eventState, eventCmd = Event.init user
                { state with CurrentPage = Page.Event eventState }, Cmd.map EventMsg eventCmd
            | _,_ -> state, Cmd.none

        | _,_ -> state, Cmd.none

    let showPosition _ (x: Area) =
        sprintf "Lon: %f, Lat: %f" x.Center.Lon x.Center.Lat

    let showRadius _ (x: Area) =
        sprintf "Radius: %.1fkm" x.Radius




    let render (state: State) (dispatch: Msg -> unit) =
        div [] [
            match state.Navbar with
            | Some navbarState -> 
                Navbar.render navbarState (NavbarMsg >> dispatch)
            | None -> 
                div [] []
            match state.CurrentPage with
            | Page.Login loginState-> 
                Login.render loginState (LoginMsg >> dispatch)
            | Page.Map userMapState -> 
                UserMap.render userMapState (UserMapMsg >> dispatch)
            | Page.Event eventState ->
                Event.render eventState (EventMsg >> dispatch)
        ]

    Program.mkProgram init update render
    |> Program.withReactBatched "app-root"
    |> Program.withErrorHandler (fun (str, ex) -> 
            console.error str
            console.error ex
        )
    |> Program.withConsoleTrace
    // |> Program.withSubscription mapSub
    |> Program.run