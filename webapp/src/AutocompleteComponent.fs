[<RequireQualifiedAccess>]
module AutocompleteComponent

open Fable.Core.JS
open Fable.React
open Fable.React.Props
open Utils

type Props = {
    Items: string list   
    OnItemSelected: string -> unit
}

type State = {
    InputValue: string
    DisplayedItems: string list
}

let private filterItems (query: string) (items: string list) =
    match query.ToLower() with
    | "" -> [] 
    | q -> items |> List.filter (fun x -> x.ToLower().Contains(q))

let view (props: Props) =
    let state = Hooks.useState({InputValue = ""; DisplayedItems = []})

    let inputValueChanged x state =
       { state with 
            InputValue = x
            DisplayedItems = filterItems x props.Items } 

    // let inputFocused state =
    //     { state with IsActive = true }
    
    // let inputUnfocused state =
    //     { state with IsActive = false }

    let panelDisplayValue state =
        match state.DisplayedItems with
        | _::_ -> DisplayOptions.Initial
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
            div [
                Class "mm-autocomplete-panel"
                Style [Display (panelDisplayValue state.current)]] 
                (state.current.DisplayedItems |> List.map (fun x -> 
                    span [Class "mm-autocomplete-item"; 
                          OnClick (const' x >> itemClicked >> state.update) ] [str x]))
        ]
        
    ]