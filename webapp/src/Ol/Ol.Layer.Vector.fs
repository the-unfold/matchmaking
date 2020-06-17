namespace Ol.Layer.Vector

open Fable.Core
open Ol.Layer
open Ol.Layer.BaseVector
open Ol.Source.Vector

type VectorLayer =
    inherit BaseVectorLayer

type VectorLayerOptions =
    abstract source: VectorSource with get, set

type VectorLayerStatic = 
    [<Emit "new $0($1...)">] abstract Create: options: VectorLayerOptions -> Layer