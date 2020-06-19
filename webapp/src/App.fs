module App

open Fable.Core
open Fable.Core.JsInterop
open Browser.Dom
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React
open Fetch

open Ol.Coordinate
open Ol.Geom
open Ol.Source.Vector
open Ol.Source.OSM
open Ol.Layer.Tile
open Ol.Layer.Vector
open Ol.Interaction.Draw
open Ol.Feature
open Ol.View
open Ol.Map
open Ol.PluggableMap
open Ol.PluggableMap.PluggableMapExtentions

importAll "ol/ol.css"

[<ImportDefault("ol/Map")>]
let mapStatic: MapStatic = jsNative

[<ImportDefault("ol/View")>]
let viewStatic: ViewStatic = jsNative

[<ImportDefault("ol/layer/Tile")>]
let tileLayerStatic: TileLayerStatic = jsNative

[<ImportDefault("ol/layer/Vector")>]
let vectorLayerStatic: VectorLayerStatic = jsNative

[<ImportDefault("ol/source/OSM")>]
let osmStatic: OSMStatic = jsNative

[<ImportDefault("ol/source/Vector")>]
let vectorSourceStatic: VectorSourceStatic = jsNative

[<ImportDefault("ol/interaction/Draw")>]
let drawStatic: DrawStatic = jsNative

[<ImportDefault("ol/Feature")>]
let featureStatic: FeatureStatic = jsNative

[<ImportDefault("ol/geom/Point")>]
let pointStatic: PointStatic = jsNative

[<ImportDefault("ol/geom/Circle")>]
let circleStatic: CircleStatic = jsNative

[<Import("fromLonLat", "ol/proj")>]
/// Transforms a coordinate from longitude/latitude to a different projection.
let fromLonLat: float * float -> Coordinate = jsNative

[<Import("toLonLat", "ol/proj")>]
/// Transforms a coordinate to longitude/latitude.
let toLonLat: Coordinate -> float * float = jsNative

let vectorSource = vectorSourceStatic.Create !!{| wrapX = false |}

let mapOptions = jsOptions<MapOptions>(fun x -> 
    x.target <- "map"
    x.layers <- [| 
        tileLayerStatic.Create !!{|source = osmStatic.Create ()|}
        vectorLayerStatic.Create !!{|source = vectorSource|} 
    |]
    
    x.view <- viewStatic.Create !!{|center = fromLonLat (82.921733, 55.029910); zoom = 16.0|})

let theMap = mapStatic.Create mapOptions
// let draw = drawStatic.Create !!{|source = vectorSource; ``type``= Point|}

[<Measure>] type km
[<Measure>] type deg

type Position = {
    Lon: float<deg>
    Lat: float<deg>
}

type Radius = float<km>

let lonLatToPosition (x: float * float): Position =
    { Lon = fst x * 1.0<deg>; Lat = snd x * 1.0<deg> }

let positionToLonLat (p: Position): float * float =
    (float p.Lon, float p.Lat)

let addPoint (vs: VectorSource) (p: Position) =
    let point =
        p |> positionToLonLat 
        |> fromLonLat 
        |> pointStatic.Create 
        
        
    point |> featureStatic.Create |> vs.addFeature 
    point

let movePoint (p: Point) (pos: Position) =
    pos |> positionToLonLat |> fromLonLat |> p.setCoordinates

let addCircle (vs: VectorSource) (rp: Radius * Point) =
    let coords = (snd rp).getCoordinates ()
    let radius = (rp |> fst |> float) * 1000.0

    let circle = circleStatic.Create (coords, radius)
    let feature = featureStatic.Create circle
    vs.addFeature feature
    circle


type Model = {
    position: Position option
    radius: Radius option
    point: Point option
    circle: Circle option
    text: string
}

type Msg =
    | SetPositionMsg of Position
    | SetRadiusMsg of Radius
    | PointAdded of Point
    | CircleAdded of Circle
    | PointMoved
    | TestResponse of string

let init () = ({
        position = None 
        radius = None 
        point = None 
        circle = None 
        text = ""
    }, Cmd.none)

let update (msg: Msg) (model: Model) =
    match msg with
    | SetPositionMsg p -> 
        { model with position = Some p }, 
        match model.point with
        | Some x -> Cmd.OfFunc.perform (fun (pt, pos) -> movePoint pt pos) (x, p) (fun _ -> PointMoved)
        | None -> Cmd.OfFunc.perform (vectorSource |> addPoint) p PointAdded
    | SetRadiusMsg r -> 
        { model with radius = Some r }, 
        Cmd.none
    | PointAdded p -> 
        { model with point = Some p }, 
        Cmd.OfFunc.perform (addCircle vectorSource) (0.5<km>, p) CircleAdded
    | CircleAdded c -> 
        { model with circle = Some c }, 
        Cmd.none
    | PointMoved -> model, Cmd.none
    | TestResponse txt -> { model with text = txt }, Cmd.none

let withFallbackMessage<'T> (fallbackMsg: string) (f: 'T -> string) (x: 'T option): string =
    match x with
    | Some t -> f t
    | None -> fallbackMsg

let formatPosition (x: Position) =
    sprintf "Lon: %f, Lat: %f" x.Lon x.Lat

let formatRadius (x: Radius) =
    sprintf "Radius: %.1fkm" x

let view model dispatch =
    div [] [
        div [] [
            span [] [ 
                model.position |> 
                withFallbackMessage "No location" formatPosition |> 
                str 
            ]
        ]
        div [] [
            label [] [ model.radius |> withFallbackMessage "" formatRadius |> str ]
            br []
            input [ 
                Type "range"; 
                Min 0.1; 
                Max 100; 
                Step 0.1;
                Class "slider-default";
                Style [Width "400px"]
                Value 0.5
                OnChange (fun x -> x.Value |> float |> fun x -> x * 1.0<km> |> SetRadiusMsg |> dispatch) 
            ]
        ]
        div [] [
            button [ 
                OnClick (fun _ -> 
                    let a = (fetch "https://localhost:5001/" [])
                    a |> Promise.bind (fun x -> x.text()) |> Promise.map (TestResponse >> dispatch) |> ignore
                )
            ] [ 
                str "Test Request" 
            ]
        ]
        div [] [
            label [] [ str model.text ]
        ]
    ]



let mapSub (initial: Model) =
    let sub dispatch =
        theMap.onClick (fun evt -> 
            evt.coordinate |> 
            toLonLat |> 
            lonLatToPosition |> 
            SetPositionMsg |> 
            dispatch
        ) |> ignore
    
    Cmd.ofSub sub


Program.mkProgram init update view
|> Program.withReactBatched  "app-root"
|> Program.withConsoleTrace
|> Program.withSubscription mapSub
|> Program.run