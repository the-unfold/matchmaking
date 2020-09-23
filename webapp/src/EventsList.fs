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
    Events: Deferred<Result<Event list, string>>
}

type Msg =
    | EditTriggered of Event
    | GetEvents of AsyncOperationStatus<Result<Event list, string>>

let getEvents apiToken =
        async {
            let token = sprintf "%O %s" apiToken.TokenType apiToken.AccessToken

            let! response =
                Http.request "/api/events"
                |> Http.method GET
                |> Http.header (Headers.authorization token)
                |> Http.send 

            let events = response |> decodeResponseAuto<EventDto list> |> Result.bind (resultTraverse eventFromDto)

            return events
        }

let getEventsCmd () = 
    let storedToken = 
        match Browser.WebStorage.localStorage.getItem "token" with
        | "" -> Error "token not found"
        | str -> Decode.Auto.fromString<ApiToken> str

    match storedToken with
    | Ok token -> 
        Cmd.OfAsync.either getEvents token (Finished >> GetEvents) (fun exn -> exn.Message |> Error |> Finished |> GetEvents)
    | Error _ -> Cmd.none

let init (user: User): State * Cmd<Msg> =
    // let mockEvent = {
    //     Id = 1
    //     Title = "FP Specialty test event"
    //     ImageUrl = None
    //     Description = "Мы профессиональная команда, нубы идут лесом.Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку. Мы профессиональная команда, нубы идут лесом. Без ссылки на твиттер не рассматриваем, вступление в группу только за взятку."
    //     Location = "https://us02web.zoom.us/j/81359006423" |> Url |> EventUrl
    //     DateTime = System.DateTime.Parse("12/12/2020 10:00")
    //     Tags = [Tag "Haskell"; Tag "Category theory"; Tag "Rock-n-roll"]
    //     Organizers = [user]
    //     Attendees = [user]
    // }
    let state = {
        Events = NotStarted
    }

    

    

    state, Cmd.ofMsg (GetEvents Started)

let update (msg: Msg) (state: State): State * Cmd<Msg> =
    match msg with
    | EditTriggered evt -> state, Cmd.none
    | GetEvents (Finished res) -> {state with Events = Resolved res}, Cmd.none
    | GetEvents Started -> state, getEventsCmd ()



let render (state: State) (dispatch: Msg -> unit) = 
    div [Class "pa-lg page-std"] [
        h1 [] [ str "My Events" ]

        match state.Events with
        | Resolved (Ok events) ->
            div [] (events |> List.map (fun evt -> 
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
        | Resolved (Error err) ->
            div [] [
                span [] [str <| "Couldn't load events: " + err]
            ]
        | InProgress ->
            div [] [
                span [] [str "Loading..."]
            ]
        | NotStarted ->
            div [] []
    ]