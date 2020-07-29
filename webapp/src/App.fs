module App


open Fable.Core
open Fable.Core.JsInterop
open Browser.Dom
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React
open Fetch
open Thoth.Json 

open Common

// importing OpenLayers js library using F# type mappings
importAll "ol/ol.css"

// // Feature - это понятие из контекста Open Layers
// type AreaFeatures = {
//     CenterFeature: Feature
//     RadiusFeature: Feature
// }

type User = {
    Id: int
    Name: string
    Location: LonLat
    Radius: float
} 
// with 
//     static member Decoder: Decoder<User> =
//         Decode.map4 
//             (fun i n l r -> {Id = i; Name = n; Location = l; Radius = r})
//             (Decode.field "id" Decode.int)
//             (Decode.field "name" Decode.string)
//             (Decode.field "location" Position.Decoder)
//             (Decode.field "radius" Decode.float)


// helper functions

/// Converts (longitude, latitude) into Position type
// let lonLatToPosition (x: float * float): Position =
//     { Lon = fst x * 1.0<deg>; Lat = snd x * 1.0<deg> }

// /// Converts Position into (longitude, latitude)
// let positionToLonLat (p: Position): float * float =
//     (float p.Lon, float p.Lat)

/// Decode User from json
// let parseUsers json =
    // (Decode.fromString (Decode.list User.Decoder)) json 

/// Creates an OpenLayers Point map feature and adds it into a VectorSource
// let addPoint (vs: VectorSource) (p: Position) =
//     let coords = 
//         p |> positionToLonLat 
//         |> fromLonLat 
//     let point = pointStatic.Create coords
        
//     let feature = featureStatic.Create point

//     vs.addFeature feature
//     point

// /// Changes OpenLayers Point coordinates
// let movePoint (p: Point) (pos: Position) =
//     let coords = pos |> positionToLonLat |> fromLonLat
//     p.setCoordinates coords
//     coords

// /// Creates an OpenLayers Circle map feature and adds it into a VectorSource
// let addCircle (vs: VectorSource) (rp: Radius * Point) =
//     let coords = (snd rp).getCoordinates ()
//     let radius = (rp |> fst |> float) * 1000.0

//     let circle = circleStatic.Create (coords, radius)
//     let feature = featureStatic.Create circle
//     console.log feature
//     vs.addFeature feature
//     circle

// /// Creates OpenLayers map features (Point and Circle) from a given Area and adds them into a VectorSource
// let setAreaFeatures (vs: VectorSource) (area: Area) =
//     let coords = area.Center |> positionToLonLat |> fromLonLat
//     let radius = (area.Radius |> float) * 1000.0

//     let point = pointStatic.Create coords
//     let circle = circleStatic.Create (coords, radius)

//     let areaFeatures = {
//         CenterFeature = featureStatic.Create point
//         RadiusFeature = featureStatic.Create circle
//     }

//     vs.addFeature areaFeatures.CenterFeature
//     vs.addFeature areaFeatures.RadiusFeature

//     areaFeatures

// /// Removes given OpenLayers features from a VectorSource
// let removeAreaFeatures (vs: VectorSource) (af: AreaFeatures option) =
//     match af with
//     | Some x -> 
//         vs.removeFeature x.CenterFeature
//         vs.removeFeature x.RadiusFeature
//     | None -> ()

// /// Changes the radius of a given OpenLayers Circle feature
// let changeAreaRadius (af: AreaFeatures) (r: Radius) =
//     let geomT = af.RadiusFeature.getGeometryT()
//     match geomT with 
//     | Circle c -> 
//         let rad = (r |> float) * 1000.0
//         c.setRadius rad
//         r
//     | _ -> invalidOp "changeAreaRadius: invalid geometry type - expected Circle"

// /// Creates OpenLayers Point and Circle map features for a given User and adds them to a VectorSource
// let addUserFeatures (vs: VectorSource) (u: User) =
//     let coords = u.Location |> positionToLonLat |> fromLonLat
//     let radius = u.Radius

//     let point = pointStatic.Create coords
//     let circle = circleStatic.Create (coords, radius)

//     point |> featureStatic.Create |> vs.addFeature
//     circle |> featureStatic.Create |> vs.addFeature

[<RequireQualifiedAccess>]
type Page =
    | Login
    | Map

type State = {
    CurrentPage: Page
    Login: Login.State
    UserMap: UserMap.State
    area: Area option
    // areaFeatures: AreaFeatures option
    text: string
    users: User list
    foundUsers: string list
}

type Msg =
    | LoginMsg of Login.Msg
    | UserMapMsg of UserMap.Msg
    // | SetArea of Area // Пользователь установил точку с default радиусом
    // | SetAreaSuccess of AreaFeatures // Точка успешно установилась в OpenLayers
    // | SetRadius of Radius // Пользователь изменил радиус
    // | SetRadiusSuccess of Radius // Радиус успешно изменен в OpenLayers
    // | GetUsers // Приложение захотело получить users от сервера
    // | GetUsersSuccess of Result<User list, string> // Пришёл ответ с users
    // | FindUsers of Area // Пользователь нажал кнопку Get All Users
    // | FindUsersSuccess of Result<string list, string> // Пришёл ответ с all users
    | Done // does nothing
    | GetTestMessage // for testing api
    | TestMessage of string // for testing

let init () = 
    let loginState, loginCmd = Login.init ()
    let userMapState, userMapCmd = UserMap.init ()
    
    let initialState = {
        CurrentPage = Page.Login
        Login = loginState 
        UserMap = userMapState
        area = None
        // areaFeatures = None
        text = ""
        users = []
        foundUsers = []
    }

    let initialCmd = Cmd.batch [
        loginCmd
        userMapCmd
    ]

    initialState, initialCmd

// api fetch functions

let getText () =
    (fetch "/api/" [])
    |> Promise.bind (fun r -> r.text())

// let getUsersSub () =
//     (fetch "/api/users" []) 
//     |> Promise.bind (fun r -> r.text())
//     |> Promise.map (parseUsers)

let findUsersSub area =
    (fetch (sprintf "/api/find-users/%f-%f-%f" area.Center.Lat area.Center.Lon (area.Radius * 1000.0)) [])
    |> Promise.bind (fun r -> r.text())
    |> Promise.map (fun txt -> Decode.fromString (Decode.list Decode.string) txt)



let update (msg: Msg) (state: State) =
    match msg with
    | LoginMsg msg -> 
        let loginState, loginCmd = Login.update msg state.Login
        { state with Login = loginState}, loginCmd
    | UserMapMsg msg -> 
        let userMapState, userMapCmd = UserMap.update msg state.UserMap
        { state with UserMap = userMapState}, userMapCmd
    // | SetArea a -> 
    //     { model with area = Some a }, 
    //     Cmd.batch [
    //         Cmd.OfFunc.either (removeAreaFeatures vectorSource) model.areaFeatures (fun _ -> Done) raise
    //         Cmd.OfFunc.either (setAreaFeatures vectorSource) a SetAreaSuccess raise
    //     ]
        
    // | SetAreaSuccess af -> 
    //     { model with areaFeatures = Some af },
    //     Cmd.none
    // | SetRadius r -> 
    //     match model.area with
    //     | Some a -> 
    //         { model with area = Some { Center = a.Center; Radius = r } },
    //         Cmd.OfFunc.either (fun (af, rad) -> changeAreaRadius af rad) (model.areaFeatures.Value, r) SetRadiusSuccess raise
    //     | None -> 
    //         model, Cmd.none
    // | SetRadiusSuccess r -> 
    //     match model.area with 
    //     | Some a -> (model, Cmd.ofMsg (FindUsers a))
    //     | None -> model, Cmd.none
    // | GetUsers -> 
    //     model, Cmd.OfPromise.either getUsersSub () GetUsersSuccess raise
    // | GetUsersSuccess users ->
    //     match users with 
    //     | Result.Ok us -> 
    //         { model with users = us },
    //         // Cmd.OfFunc.either (List.map (addUserFeatures vectorSource)) us (fun _-> Done) raise
    //         Cmd.none
    //     | Result.Error e -> invalidOp e
    // | FindUsers area ->
    //     model, Cmd.OfPromise.either findUsersSub area FindUsersSuccess raise
    // | FindUsersSuccess users ->
    //     match users with 
    //     | Result.Ok us -> { model with foundUsers = us }, Cmd.none
    //     | Result.Error e -> invalidOp e
    | GetTestMessage -> 
        state, Cmd.OfPromise.either getText () TestMessage raise
    | TestMessage msg -> 
        { state with text = msg }, Cmd.none
    | Done -> state, Cmd.none


let showPosition _ (x: Area) =
    sprintf "Lon: %f, Lat: %f" x.Center.Lon x.Center.Lat

let showRadius _ (x: Area) =
    sprintf "Radius: %.1fkm" x.Radius




let render (state: State) (dispatch: Msg -> unit) =
    div [] [
        match state.CurrentPage with
        | Page.Login -> 
            Login.render state.Login (LoginMsg >> dispatch)
        | Page.Map -> 
            UserMap.render state.UserMap (UserMapMsg >> dispatch)
    ]

let view model dispatch =
    div [] [
        // ofType<UserMap.Map,_,_> { center = {Lon = 82.921733; Lat = 55.029910}; zoom = 14.0 } []
        div [] [
            span [] [ 
                model.area 
                |> Option.fold showPosition "No location"
                |> str 
            ]
        ]
        div [] [
            label [] [ model.area |> Option.fold showRadius "" |> str ]
            br []
            input [ 
                Type "range"; 
                Min 0.1; 
                Max 5; 
                Step 0.1;
                Class "slider-default";
                Style [Width "400px"]
                // OnChange (fun x -> x.Value |> float |> fun x -> x * 1.0<km> |> SetRadius |> dispatch) 
            ]
        ]
        div [] [
            label [] [ if model.foundUsers.Length > 0 then str "Users nearby:" else str "You're alone..."]
        ]
        div [] [
            ol [] [
                for u in model.foundUsers -> li [] [ str u ]
            ]
        ]
        div [] [
            button [ 
                // OnClick (fun _ -> dispatch GetUsers)
            ] [ 
                str "Show All Users" 
            ]
        ]
        div [] [
            button [
                OnClick (fun _ -> dispatch GetTestMessage)
            ] [
                str "Text"
            ]
        ]
        div [] [
            label [] [ str model.text ]
        ]
    ]

// let mapClickToDefaultArea (evt: MapBrowserEvent) =
//     {
//         Center = evt.coordinate |> toLonLat |> lonLatToPosition
//         Radius = 0.5<km>
//     }

// let mapSub (initial: Model) =
//     let sub dispatch =
//         theMap.onClick (mapClickToDefaultArea >> SetArea >> dispatch) |> ignore
//     Cmd.ofSub sub


Program.mkProgram init update render
|> Program.withReactBatched "app-root"
|> Program.withErrorHandler (fun (str, ex) -> 
        console.error str
        console.error ex
    )
|> Program.withConsoleTrace
// |> Program.withSubscription mapSub
|> Program.run