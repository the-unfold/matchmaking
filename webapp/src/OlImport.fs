module OlImport

open Fable.Core
open Fable.Core.JsInterop

open Ol.Observable
open Ol.Events
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
open Ol.Feature.FeatureExtensions


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

[<Import("unByKey", "ol/Observable")>]
let unByKey: key: EventsKey -> unit = jsNative