[<RequireQualifiedAccess>]
module AutocompleteComponent

open Fable.Core.JS
open Elmish
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Thoth.Json
open Utils

// type private Msg =
//     | ValueChanged of string
//     | GetItems of AsyncOperationStatus<Result<string list, string>>

type Items =
    | ItemsList of string list
    | ItemsUrl of string

type Props = {
    Items: Items
    OnItemSelected: string -> unit
}

type State = {
    InputValue: string
    DisplayedItems: string list
    DisplayedItemsDeferred: Deferred<Result<string list, string>>
}

// type State = {
//     InputValue: string
//     DisplayedItems: Deferred<Result<string list, string>>
// }

// let private getTagsCmd query =
//     let url = "/api/tags"
//     async {
//         let! response =
//             Http.request url
//             |> Http.method GET
//             |> Http.send

//         let items = decodeResponse<string list> response
//         return items
//     }

// let private update (msg: Msg) (state: State): State * Cmd<Msg> =
//     match msg with
//     | ValueChanged str -> 
//         { state with InputValue = str }, 
//         Cmd.ofMsg (GetItems Started)
//     | GetItems Started -> 
//         { state with DisplayedItems = InProgress }, 
//         Cmd.OfAsync.either getTagsCmd (state.InputValue) (Finished >> GetItems) raise
//     | GetItems (Finished res) -> { state with DisplayedItems = Resolved res }, Cmd.none

let private filterItems (query: string) (items: string list) =
    match query.ToLower() with
    | "" -> [] 
    | q -> items |> List.filter (fun x -> x.ToLower().Contains(q))


let view (props: Props) =
    let state = Hooks.useState({
        InputValue = "";
        DisplayedItems = []
        DisplayedItemsDeferred = NotStarted
    })
    
    // let url = "/api/tags/"
    
    let getTags url query = 
        async {
            let! response = 
                Http.request (sprintf "%s/%s" url query)
                |> Http.method GET
                |> Http.send
            
            console.log response

            let items = decodeResponse<string list> response

            return items
        }

    let tagsReceived tags st =
        { st with DisplayedItemsDeferred = Resolved tags }

    let inputValueChanged x st =
        match x, props.Items with
        | "", _ -> {st with InputValue = x; DisplayedItemsDeferred = NotStarted}
        | str, ItemsList tags -> 
            let filtered = filterItems str tags
            { st with InputValue = str; DisplayedItemsDeferred = Resolved <| Ok filtered }
        | str, ItemsUrl url -> 
            async {
                let! tags = getTags url str
                
                tags |> tagsReceived |> state.update
                return ()
            } |> Async.Start

            { st with InputValue = str; DisplayedItemsDeferred = InProgress } 

    let panelDisplayValue state =
        match state.DisplayedItemsDeferred with
        | Resolved (Ok (_::_)) -> DisplayOptions.Initial
        | _ -> DisplayOptions.None

    let itemClicked x state =
        do props.OnItemSelected x
        inputValueChanged "" state

    div [Class "mm-autocomplete"] [
        input [
            Class "input-std"
            Value state.current.InputValue
            OnChange (fun e -> e.Value |> inputValueChanged |> state.update)]
        div [Class "mm-autocomplete-panel-origin"] [
            match state.current.DisplayedItemsDeferred with
            | NotStarted -> 
                div [Class "mm-autocomplete-panel"; Style [Display DisplayOptions.None]] []
            | InProgress -> 
                div [Class "mm-autocomplete-panel"] [
                    span [Class "mm-autocomplete-item"] [str "loading..."]
                ]
            | Resolved (Error e) -> 
                div [Class "mm-autocomplete-panel"; Style [Display DisplayOptions.None]] []
                span [Class "validation-message"] [str e]
            | Resolved (Ok tags) ->
                div [Class "mm-autocomplete-panel"] (tags |> List.map (fun x -> 
                    span [Class "mm-autocomplete-item"; 
                          OnClick (const' x >> itemClicked >> state.update) ] [str x]))
            
        ]
        
    ]