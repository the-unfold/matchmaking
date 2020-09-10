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

type DateTime = System.DateTime


type Person = {
    Name: string
}

type State = {
    User: User
    IsOnline: bool
    Title: string
    ValidatedTitle: Result<string, string>
    Description: string
    ValidatedDescription: Result<string, string>
    DateTime: string
    ValidatedDateTime: Result<DateTime, string>
    Tags: Tag list   
    Location: EventLocation 
    ValidatedLocation: Result<EventLocation, string>
    Attendees: User list
    Organizers: User list
    IsFormDirty: bool
}

type Msg = 
    | IsOnlineChanged of bool
    | TitleChanged of string
    | TitleValidated of Result<string, string>
    | DescriptionChanged of string
    | DescriptionValidated of Result<string, string>
    | DateTimeChanged of string
    | DateTimeValidated of Result<DateTime, string>
    | TagAdded of Tag
    | TagRemoved of Tag
    | LocationChanged of EventLocation
    | LocationValidated of Result<EventLocation, string>
    | AttendeeAdded of User
    | AttendeeRemoved of User
    | SaveTriggered
    | BackTriggered

let validateTitle = (Validate.required "Title required")
let validateDescription = (Validate.required "Description required")
let validateLocation x = 
    match x with 
    | GeographicLocation lonLat -> Ok x
    | EventUrl (Url x) -> 
        x |> Validate.required "Event URL required" 
          |> Result.bind (Validate.url "Invalid url") 
          |> Result.map (Url >> EventUrl)
let validateDateTime = 
    (Validate.required "Date and time required") >> 
    (Result.bind (Validate.dateTime "Invalid Date/Time format"))

let init (user: User) (event: Event option): State * Cmd<Msg> =

    let isOnline (e: Event) = 
        match e.Location with 
        | GeographicLocation _ -> false 
        | EventUrl _ -> true

    let state = 
        {
            User = user
            IsOnline = event |> Option.map isOnline |> Option.defaultValue true
            Title = event |> Option.map (fun e -> e.Title) |> Option.defaultValue ""
            ValidatedTitle = Ok ""
            Description = event |> Option.map (fun e -> e.Description) |> Option.defaultValue ""
            ValidatedDescription = Ok ""
            DateTime = event |> Option.map (fun e -> e.DateTime.ToString("yyyy-MM-ddTHH:mm")) |> Option.defaultValue ""
            ValidatedDateTime = Ok DateTime.Now
            Tags = event |> Option.map (fun e -> e.Tags) |> Option.defaultValue []
            Location = event |> Option.map (fun e -> e.Location) |> Option.defaultValue (EventUrl << Url <| "")
            ValidatedLocation = Ok << EventUrl <| Url ""
            Attendees = event |> Option.map (fun e -> e.Attendees) |> Option.defaultValue []
            Organizers = event |> Option.map (fun e -> e.Organizers) |> Option.defaultValue []
            IsFormDirty = false
        }

    { state with
        ValidatedTitle = validateTitle state.Title
        ValidatedDescription = validateDescription state.Description 
        ValidatedLocation = validateLocation state.Location 
        ValidatedDateTime = validateDateTime state.DateTime }, Cmd.none


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
    | DateTimeChanged x -> 
        { state with DateTime = x }, 
        x |> validateDateTime |> DateTimeValidated |> Cmd.ofMsg
    | DateTimeValidated dt -> { state with ValidatedDateTime = dt }, Cmd.none
    | TagAdded tag -> 
        match List.tryFind ((=)tag) state.Tags with
        | Some _ -> state, Cmd.none
        | None -> { state with Tags = tag::state.Tags }, Cmd.none
    | TagRemoved tag -> { state with Tags = List.filter ((<>) tag) state.Tags}, Cmd.none
    | LocationChanged loc -> 
        { state with Location = loc },
        loc |> validateLocation |> LocationValidated |> Cmd.ofMsg 
    | LocationValidated res -> { state with ValidatedLocation = res }, Cmd.none
    | AttendeeAdded p -> { state with Attendees = p::state.Attendees }, Cmd.none
    | AttendeeRemoved usr -> { state with Attendees = List.filter ((<>) usr) state.Attendees }, Cmd.none
    | SaveTriggered -> { state with IsFormDirty = true }, Cmd.none
    | BackTriggered -> state, Cmd.none

let tempTags = ["F#"; "Haskell"; "OhMyGod"; "Scala"; "Elm"; "Abracadabra"]

let render (state: State) (dispatch: Msg -> unit) = 
    div [Class "pa-lg page-std"] [
        img [Class "event-image"; Src (Image.load "./assets/event-image-placeholder.svg")]
        h1 [] [ sprintf "Manage Event: %s" state.Title |> str ]
        div [Class "row"] [
            div [Class "column flex-1 chld-mb-md mr-lg"] [
                div [Class "column"] [
                    label [Class "label-std"] [ str "Event Title" ]
                    input [
                        Class "input-std"
                        Value state.Title
                        OnChange (fun e -> e.Value |> TitleChanged |> dispatch)]
                    match state.ValidatedTitle, state.IsFormDirty with
                    | Error err, true -> span [Class "validation-message"] [ str err ]
                    | _, _ -> span [Style [Display DisplayOptions.None]] []
                ]

                div [Class "column"] [
                    label [Class "label-std"] [ str "Event Link" ]
                    input [
                        Class "input-std"
                        Value (match state.Location with EventUrl(Url url) -> url | GeographicLocation _ -> "")
                        OnChange (fun e -> e.Value |> Url |> EventUrl |> LocationChanged |> dispatch)]
                    match state.ValidatedLocation, state.IsFormDirty with
                    | Error err, true -> span [Class "validation-message"] [str err]
                    | _,_ -> span [Style [Display DisplayOptions.None]] []
                ]

                div [Class "column"] [
                    label [Class "label-std"] [ str "Event Date/Time" ]
                    input [
                        Class "input-std"
                        Type "datetime-local"
                        Value state.DateTime
                        OnChange (fun e -> e.Value |> DateTimeChanged |> dispatch)]
                    match state.ValidatedDateTime, state.IsFormDirty with
                    | Error err, true -> span [Class "validation-message"] [str err]
                    | _ -> span [Style [Display DisplayOptions.None]] []
                ]

                div [Class "column"] [
                    label [Class "label-std"] [ str "Event Description" ]
                    textarea [
                        Class "text-area-std"
                        Value state.Description
                        OnChange (fun e -> e.Value |> DescriptionChanged |> dispatch)] []
                    match state.ValidatedDescription, state.IsFormDirty with
                    | Error err, true -> span [Class "validation-message"] [str err]
                    | _,_ -> span [Style [Display DisplayOptions.None]] []
                ]

                div [Class "column"] [
                    label [Class "label-std"] [ str "Search Tags" ]
                    FunctionComponent.Of(AutocompleteComponent.view) {
                        Items = tempTags
                        OnItemSelected = Tag >> TagAdded >> dispatch }
                ]

                div [Class "slot-std row wrap align-center chld-mr-sm chld-mb-sm"] 
                    (state.Tags |> List.map (fun (Tag t) -> 
                        span [Class "tag"; OnClick (fun _ -> t |> Tag |> TagRemoved |> dispatch )] [ str t ]))
                    
                

            ]

            div [Class "column flex-1 ml-xl pl-xl"] [
                h2 [] [ str "Organizers" ]
                FunctionComponent.Of(UserComponent.view) { User = state.User }
                h2 [] [ str "Attendees" ]
                FunctionComponent.Of(UserComponent.view) { User = state.User }
            ]
        ]

        div [Class "row chld-mr-lg"] [
            button [Class "btn-std-lg"; OnClick (fun _ -> dispatch SaveTriggered)] [str "Save Event"]
            button [Class "btn-alt-lg"; OnClick (fun _ -> dispatch BackTriggered)] [str "Don't save"]
        ]
    ]