namespace Ol.Layer.Tile

open Fable.Core
open Ol.Layer
open Ol.Layer.BaseTile
open Ol.Source.OSM

type TileLayer =
    inherit BaseTileLayer

type TileLayerOptions = 
    abstract source: OSM with get, set

type TileLayerStatic =
    [<Emit "new $0($1...)">] abstract Create: options: TileLayerOptions -> Layer