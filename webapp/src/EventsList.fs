[<RequireQualifiedAccess>]
module EventsList

open Elmish
open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Thoth.Json
open Utils
open Common

type State = {
    Events: Event list
}

type Msg =
    | EditTriggered of Event

let init (user: User): State * Cmd<Msg> =
    let mockEvent = {
        Title = "FP Specialty test event"
        Description = "Мы профессиональная команда, нубы идут лесом.Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку."
        Location = "https://us02web.zoom.us/j/81359006423" |> Url |> EventUrl
        DateTime = System.DateTime.Parse("12/12/2020 10:00")
        Tags = [Tag "Haskell"; Tag "Category theory"; Tag "Rock-n-roll"]
        Organizers = [user]
        Attendees = [user]
    }
    let state = {
        Events = [mockEvent]
    }

    state, Cmd.none

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | EditTriggered evt -> state, Cmd.none



let render (state: State) (dispatch: Msg -> unit) = 
    div [Class "pa-lg page-std"] [
        h1 [] [ str "My Events" ]

        div [] (state.Events |> List.map (fun evt -> 
            div [Class "column event-info"] [
                h2 [] [ str evt.Title ]
                p [] [ str evt.Description ]
                div [Class "row content-sb"] [
                    match evt.Location with 
                    | EventUrl (Url x) -> a [] [ str x ]
                    | GeographicLocation x -> span [] [ str "address not implemented" ]

                    span [] [ str <| sprintf "Attendees: %i" (List.length evt.Attendees)]
                    button [Class "btn-std-lg"; OnClick (fun _ -> evt |> EditTriggered |> dispatch)] [str "Manage"]
                ]
                div [Class "row wrap align-center chld-mr-sm chld-mb-sm"] (evt.Tags |> List.map (fun (Tag tag) -> 
                    span [Class "tag"] [str tag]))

            ]
        ))
    ]