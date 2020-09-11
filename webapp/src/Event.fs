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
    ValidatedTitle: Result<string, string>
    Description: string
    ValidatedDescription: Result<string, string>
    Tags: Tag list   
    Location: EventLocation 
    ValidatedLocation: Result<EventLocation, string>
    Attendees: Person list
    IsFormDirty: bool
}

type Msg = 
    | IsOnlineChanged of bool
    | TitleChanged of string
    | TitleValidated of Result<string, string>
    | DescriptionChanged of string
    | DescriptionValidated of Result<string, string>
    | TagAdded of Tag
    | TagRemoved of Tag
    | LocationChanged of EventLocation
    | LocationValidated of Result<EventLocation, string>
    | AttendeeAdded of Person
    | AttendeeRemoved of Person
    | SaveTriggered

let validateTitle = (Validate.required "Title required")
let validateDescription = (Validate.required "Description required")
let validateLocation x = 
    match x with 
    | GeographicLocation lonLat -> Ok x
    | EventUrl x -> 
        x |> Validate.required "Event URL required" 
          |> Result.bind (Validate.url "Invalid url") 
          |> Result.map EventUrl



let init (user: User): State * Cmd<Msg> =
    let state = {
        IsOnline = true
        Title = ""
        ValidatedTitle = Ok ""
        Description = ""
        ValidatedDescription = Ok ""
        Tags = []
        Location = EventUrl ""
        ValidatedLocation = Ok (EventUrl "")
        Attendees = []
        IsFormDirty = false
    }

    { state with
        ValidatedTitle = validateTitle state.Title
        ValidatedDescription = validateDescription state.Description 
        ValidatedLocation = validateLocation state.Location }, Cmd.none


let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | IsOnlineChanged b -> { state with IsOnline = b }, Cmd.none
    | TitleChanged txt -> 
        { state with Title = txt },
        txt |> validateTitle |> TitleValidated |> Cmd.ofMsg
    | TitleValidated res -> { state with ValidatedTitle = res }, Cmd.none
    | DescriptionChanged txt -> 
        { state with Description = txt }, 
        txt |> validateDescription |> DescriptionValidated |> Cmd.ofMsg 
    | DescriptionValidated res -> { state with ValidatedDescription = res }, Cmd.none
    | TagAdded tag -> { state with Tags = tag::state.Tags }, Cmd.none
    | TagRemoved tag -> { state with Tags = List.filter ((<>) tag) state.Tags}, Cmd.none
    | LocationChanged loc -> 
        { state with Location = loc },
        loc |> validateLocation |> LocationValidated |> Cmd.ofMsg 
    | LocationValidated res -> { state with ValidatedLocation = res }, Cmd.none
    | AttendeeAdded p -> { state with Attendees = p::state.Attendees }, Cmd.none
    | AttendeeRemoved person -> { state with Attendees = List.filter ((<>) person) state.Attendees }, Cmd.none
    | SaveTriggered -> { state with IsFormDirty = true }, Cmd.none


let render (state: State) (dispatch: Msg -> unit) = 
    div [] [
        h2 [] [ str "Manage Event" ]
        div [Class "row"] [
            div [Class "column flex-1 chld-mb-md"] [

                div [Class "column"] [
                    label [Class "label-default"] [ str "Event Link" ]
                    input [Class "input-default"; OnChange (fun e -> e.Value |> EventUrl |> LocationChanged |> dispatch)]
                    match state.ValidatedLocation, state.IsFormDirty with
                    | Error err, true -> span [Class "validation-message"] [str err]
                    | _,_ -> span [Style [Display DisplayOptions.None]] []
                ]

                div [Class "column"] [
                    label [Class "label-default"] [ str "Event Time" ]
                    input [Class "input-default"]
                ]

                div [Class "column"] [
                    label [Class "label-default"] [ str "Event Title" ]
                    input [Class "input-default"; OnChange (fun e -> e.Value |> TitleChanged |> dispatch)]
                    match state.ValidatedTitle, state.IsFormDirty with
                    | Error err, true -> span [Class "validation-message"] [ str err ]
                    | _, _ -> span [Style [Display DisplayOptions.None]] []
                ]

                div [Class "column"] [
                    label [Class "label-default"] [ str "Event Description" ]
                    textarea [Class "input-default"; OnChange (fun e -> e.Value |> DescriptionChanged |> dispatch)] []
                    match state.ValidatedDescription, state.IsFormDirty with
                    | Error err, true -> span [Class "validation-message"] [str err]
                    | _,_ -> span [Style [Display DisplayOptions.None]] []
                ]

                div [Class "column"] [
                    label [Class "label-default"] [ str "Search Tags" ]
                    input [Class "input-default"]
                ]

                div [Class "chld-mr-sm"] [
                    span [Class "tag"] [ str "Haskell" ]
                    span [Class "tag"] [ str "F#" ]
                    span [Class "tag"] [ str "Scala" ]
                    span [Class "tag"] [ str "Elm" ]
                ]
            ]

            div [Class "column flex-1"] [
                h4 [] [ str "Attendees" ]
            ]
        ]
        button [Class "btn-default"; OnClick (fun _ -> dispatch SaveTriggered)] [str "Save"]
    ]