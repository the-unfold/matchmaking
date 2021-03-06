[<RequireQualifiedAccess>]
module Navbar 

open Elmish
open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Thoth.Json
open Utils
open Common

type State = {
    User: User        
}

type Msg = 
    | EventsNavTriggered

let init (user: User) =
    { User = user }, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | _ -> state, Cmd.none


let render (state: State) (dispatch: Msg -> unit) =
    div [Class "row content-sb align-center navbar"] [
        div [] [
            span [Class "nav-item"; OnClick (fun _ -> dispatch EventsNavTriggered)] [ str "Events" ]
        ]
        div [] [
            FunctionComponent.Of UserComponent.view {User = state.User}
        ]
    ]
