module App

open Fable.Core
open Fable.Core.JsInterop
open Browser.Dom

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


// draw.on ("drawend", (fun (evt: DrawEvent) -> 
//     console.log (evt.feature)
//     console.log (evt.feature.getGeometry())
//     console.log ((evt.feature.getGeometry() :?> SimpleGeometry).getCoordinates())
// )) |> ignore
// theMap.on("click", (fun evt -> console.log (evt :?> MapBrowserEvent).coordinate)) |> ignore
// pluggableMapOnClick theMap (fun evt -> console.log evt.coordinate) |> ignore
theMap.onClick (fun evt -> console.log evt.coordinate) |> ignore
theMap.movestart (fun evt -> console.log evt.``type``) |> ignore
theMap.moveend (fun evt -> console.log evt.``type``) |> ignore
