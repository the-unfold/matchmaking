[<RequireQualifiedAccess>]
module UserMap

open Browser.Dom
open Browser.Types
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fable.SimpleHttp
open Elmish
open Elmish.React

open Ol.Coordinate
open Ol.Map
open Ol.PluggableMap
open Ol.PluggableMap.PluggableMapExtentions

open OlImport
open Common
open Utils
open MapComponent

type State = {
    Area: Area option
}

type Msg = 
    | AreaSet of Area

let init () =
    { Area = None }, Cmd.none
   
let update (msg: Msg) (state: State) =
    match msg with 
    | AreaSet _ -> state, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    // div [] [
        ofType<MapComponent,_,_> { center = {Lon = 82.921733; Lat = 55.029910}; zoom = 14.0 } []
    // ]   
// type State = {
//     area: Area option
//     areaFeatures: AreaFeatures option
//     users: User list
//     foundUsers: string list
// }

// type Msg =
//     | SetArea of Area // Пользователь установил точку с default радиусом
//     | SetAreaSuccess of AreaFeatures // Точка успешно установилась в OpenLayers
//     | SetRadius of Radius // Пользователь изменил радиус
//     | SetRadiusSuccess of Radius // Радиус успешно изменен в OpenLayers
//     | GetUsers // Приложение захотело получить users от сервера
//     | GetUsersSuccess of Result<User list, string> // Пришёл ответ с users
//     | FindUsers of Area // Пользователь нажал кнопку Get All Users
//     | FindUsersSuccess of Result<string list, string> // Пришёл ответ с all users
    



// let init () = ({
//         area = None
//         areaFeatures = None
//         users = []
//         foundUsers = []
//     }, Cmd.OfFunc.either initMap () raise)


// let update (msg: Msg) (state: State) =
//     match msg with
//     | SetArea a -> 
//         { state with area = Some a }, 
//         Cmd.batch [
//             Cmd.OfFunc.either (removeAreaFeatures vectorSource) state.areaFeatures (fun _ -> Done) raise
//             Cmd.OfFunc.either (setAreaFeatures vectorSource) a SetAreaSuccess raise
//         ]
        
//     | SetAreaSuccess af -> 
//         { state with areaFeatures = Some af },
//         Cmd.none
//     | SetRadius r -> 
//         match state.area with
//         | Some a -> 
//             { model with area = Some { Center = a.Center; Radius = r } },
//             Cmd.OfFunc.either (fun (af, rad) -> changeAreaRadius af rad) (model.areaFeatures.Value, r) SetRadiusSuccess raise
//         | None -> 
//             model, Cmd.none
//     | SetRadiusSuccess r -> 
//         match state.area with 
//         | Some a -> (model, Cmd.ofMsg (FindUsers a))
//         | None -> model, Cmd.none
//     | GetUsers -> 
//         state, Cmd.OfPromise.either getUsersSub () GetUsersSuccess raise
//     | GetUsersSuccess users ->
//         match users with 
//         | Result.Ok us -> 
//             { model with users = us },
//             Cmd.OfFunc.either (List.map (addUserFeatures vectorSource)) us (fun _-> Done) raise
//         | Result.Error e -> invalidOp e
//     | FindUsers area ->
//         state, Cmd.OfPromise.either findUsersSub area FindUsersSuccess raise
//     | FindUsersSuccess users ->
//         match users with 
//         | Result.Ok us -> { state with foundUsers = us }, Cmd.none
//         | Result.Error e -> invalidOp e

// let render (state: State) (dispatch: Msg -> unit) =
// 	div [Id "map"] []


