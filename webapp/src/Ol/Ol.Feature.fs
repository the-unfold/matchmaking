namespace Ol.Feature

open Ol.Object
open Ol.Geom

type Feature =
    inherit BaseObject

    abstract getGeometry: unit -> Geometry