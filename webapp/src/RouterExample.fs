[<RequireQualifiedAccess>]
module RouterExample

open Elmish
open Elmish.Navigation
open Fable.React
open Browser.Dom
open Elmish.React
open Elmish.Navigation

let (</>) = UrlParser.op_LessDivideGreater

type Route =
    // | Login
    // | DiscoverEvents
    | MyEvents
    | Event of int
// | ManageEvent of int
// | Community of int
// | ManageCommunity of int

// parser
let route: UrlParser.Parser<Route -> Route, Route> =
    UrlParser.oneOf [ UrlParser.map MyEvents (UrlParser.s "my-events")
                      UrlParser.map Event (UrlParser.s "event" </> UrlParser.i32) ]

type State = { route: Route }

type Msg = FuckEd of int

let update msg state = state, Cmd.none

let urlUpdate (result: Option<Route>) (model: State): State * Cmd<Msg> =
    match result with
    | Some MyEvents -> { model with route = result.Value }, Cmd.none // Issue some search Cmd instead

    | Some (Event id) -> { model with route = result.Value }, Cmd.none

    | None -> (model, Navigation.modifyUrl "#")


let init result =
    let (model, cmd) =
        urlUpdate result { route = Route.MyEvents }

    model, cmd

let showRoute route =
    match route with
    | MyEvents -> "MyEvents"
    | Event eventId -> sprintf "Event %i" eventId

let render (state: State) (dispatch: Msg -> unit) =

    div [] [
        str "RouterExample render: "
        str <| showRoute state.route
    ]

// Program.mkProgram init update render
// |> Program.toNavigable (UrlParser.parseHash route) urlUpdate
// |> Program.withReactBatched "app-root"
// |> Program.withErrorHandler (fun (str, ex) ->
//     console.error str
//     console.error ex)
// |> Program.withConsoleTrace
// // |> Program.withSubscription mapSub
// |> Program.run
