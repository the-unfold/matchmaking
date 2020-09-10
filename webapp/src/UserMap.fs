
[<RequireQualifiedAccess>]
module UserMap 

open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React

open Common
open Utils

open MapComponent

type State = {
    Areas: Area list
    User: User
    Navbar: Navbar.State
    Count: int
}

type Msg = 
    // | NavbarMsg of Navbar.Msg
    | MapMsg of MapComponentMsg
    | Increment

let init (user: User) =
    let navbarState, navbarCmd = Navbar.init user
    { Areas = []; User = user; Navbar = navbarState; Count = 0 }, Cmd.none
   
let update (msg: Msg) (state: State) =
    match msg with 
    // | NavbarMsg _ -> state, Cmd.none
    | MapMsg (Clicked x) -> {state with Areas = {Center = x; Radius = 2.2<km>}::state.Areas}, Cmd.none
    | Increment -> {state with Count = state.Count + 1}, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    // let m = state.Areas |> List.map (fun a -> ofFloat a.Center.Lon) |> List.toSeq
    div [] [
        // Navbar.render state.Navbar (NavbarMsg >> dispatch)
        FunctionComponent.Of(mapComponentFn) 
            { center = {Lon = 82.921733; Lat = 55.029910}; 
              zoom = 14.0; 
              areas = state.Areas; 
              dispatch = (MapMsg >> dispatch) }
        
        // ol [] [
        //     state.Areas |> List.map (fun (area) -> 
        //         li [] [sprintf "%f; %f (%f)" area.Center.Lon area.Center.Lat area.Radius |> str]
        //     ) |> ofList 
        // ]

        div [] [
            ofInt state.Count
            button [OnClick (fun _ -> dispatch Increment)] [str "Increment"]
        ]
    ]   