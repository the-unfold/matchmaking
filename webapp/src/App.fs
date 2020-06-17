module App

open Fable.Core
open Fable.Core.JsInterop
open Browser.Dom
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React

open Ol.Geom
open Ol.Source.Vector
open Ol.Source.OSM
open Ol.Layer.Tile
open Ol.Layer.Vector
open Ol.Interaction.Draw
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

[<Import("fromLonLat", "ol/proj")>]
let fromLonLat: float * float -> float * float = jsNative

[<Import("toLonLat", "ol/proj")>]
let toLonLat: float * float -> float * float = jsNative

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

type Model = {
    position: Position option
    radius: Radius option
}

type Msg =
    | SetPositionMsg of Position
    | SetRadiusMsg of Radius

let init (): Model = { position = None; radius = None }

let update (msg: Msg) (model: Model) =
    match msg with
    | SetPositionMsg p -> { model with position = Some p }
    | SetRadiusMsg r -> { model with radius = Some r }

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
                Max 1000; 
                Step 0.1;
                Class "slider-default";
                Style [Width "400px"]
                OnChange (fun x -> x.Value |> float |> fun x -> x * 1.0<km> |> SetRadiusMsg |> dispatch) 
            ]
        ]
    ]

let lonLatToPosition (x: float * float): Position =
    { Lon = fst x * 1.0<deg>; Lat = snd x * 1.0<deg> }

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


Program.mkSimple init update view
|> Program.withReactBatched  "app-root"
|> Program.withConsoleTrace
|> Program.withSubscription mapSub
|> Program.run