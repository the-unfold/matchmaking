namespace Ol.Source.Vector

open Fable.Core
open Ol.Source
open Ol.Feature

type VectorSource =
    inherit Source

    abstract addFeature: Feature -> unit
    abstract addFeatures: Feature list -> unit

    abstract removeFeature: Feature -> unit

type VectorSourceOptions =
    abstract wrapX: bool with get, set

type VectorSourceStatic =
    [<Emit "new $0($1...)">] abstract Create: VectorSourceOptions -> VectorSource