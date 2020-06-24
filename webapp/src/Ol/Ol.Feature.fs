namespace Ol.Feature

open Fable.Core
open Ol.Object
open Ol.Geom

type FeatureId = S of string | I of int

type FeatureGeometry =
    | Point of Point
    | Circle of Circle
    | LineString of LineString
    | GeometryCollection of GeometryCollection
//   = SimpleGeometry of SimpleGeometry
//   | GeometryCollection of GeometryCollection



type Feature =
    inherit BaseObject

    abstract getGeometry: unit -> Geometry
    abstract getId: unit -> FeatureId
    
type FeatureStatic =
    [<Emit "new $0($1...)">] abstract Create: Geometry -> Feature

module FeatureExtensions =
    type Feature with
        member this.getGeometryT (): FeatureGeometry =
            let geom = this.getGeometry () :?> SimpleGeometry
            let t = geom.getType()
            match t with
            | GeometryType.Point -> Point (geom :?> Point)
            | GeometryType.Circle -> Circle (geom :?> Circle)
            | GeometryType.LineString -> LineString (geom :?> LineString)
            | GeometryType.GeometryCollection -> GeometryCollection (geom :?> GeometryCollection)
            | _ -> invalidOp "not implemented FeatureExtensions"
             