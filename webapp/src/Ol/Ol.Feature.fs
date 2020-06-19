namespace Ol.Feature

open Fable.Core
open Ol.Object
open Ol.Geom

type FeatureId = S of string | I of int

type Feature =
    inherit BaseObject

    abstract getGeometry: unit -> Geometry
    abstract getId: unit -> FeatureId


type FeatureStatic =
    [<Emit "new $0($1...)">] abstract Create: Geometry -> Feature