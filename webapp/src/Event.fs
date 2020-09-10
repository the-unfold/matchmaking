[<RequireQualifiedAccess>]
module Event

open Elmish
open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Thoth.Json
open Utils
open Common


type Tag = Tag of string
type EventLocation = GeographicLocation of LonLat | EventUrl of string
type Person = {
    Name: string
}

type State = {
    IsOnline: bool
    Title: string
    Description: string
    Tags: Tag list   
    Location: EventLocation 
    Attendees: Person list
}

type Msg = 
    | IsOnlineChanged of bool
    | TitleChanged of string
    | DescriptionChanged of string
    | TagAdded of Tag
    | TagRemoved of Tag
    | LocationChanged of EventLocation
    | AttendeeAdded of Person
    | AttendeeRemoved of Person


let init (user: User): State * Cmd<Msg> =
    let state = {
        IsOnline = true
        Title = ""
        Description = ""
        Tags = []
        Location = EventUrl ""
        Attendees = []
    }

    state, Cmd.none

let a = (<>) 1

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | IsOnlineChanged b -> { state with IsOnline = b }, Cmd.none
    | TitleChanged txt -> { state with Title = txt }, Cmd.none
    | DescriptionChanged txt -> { state with Description = txt }, Cmd.none
    | TagAdded tag -> { state with Tags = tag::state.Tags }, Cmd.none
    | TagRemoved tag -> { state with Tags = List.filter ((<>) tag) state.Tags}, Cmd.none
    | LocationChanged loc -> { state with Location = loc }, Cmd.none
    | AttendeeAdded p -> { state with Attendees = p::state.Attendees }, Cmd.none
    | AttendeeRemoved person -> { state with Attendees = List.filter ((<>) person) state.Attendees }, Cmd.none


let render (state: State) (dispatch: Msg -> unit) = 
    div [] [
        h2 [] [ str "Manage Event" ]

        div [Class "form-item"] [
            label [] [ str "Event Link" ]
            input []
        ]

        div [Class "form-item"] [
            label [] [ str "Event Time" ]
            input []
        ]

        div [Class "form-item"] [
            label [] [ str "Event Title" ]
            input []
        ]

        div [Class "form-item"] [
            label [] [ str "Event Description" ]
            textarea [] []
        ]

        div [Class "form-item"] [
            label [] [ str "Search Tags" ]
            input []
        ]

        div [Class "tag-list"] [
            span [Class "tag-default"] [ str "Haskell" ]
            span [Class "tag-default"] [ str "F#" ]
            span [Class "tag-default"] [ str "Scala" ]
            span [Class "tag-default"] [ str "Elm" ]
        ]
    ]