namespace Ol.Geom

open Fable.Core
open Ol.Object
open Ol.Coordinate

[<StringEnum>]
type GeometryType = 
    | [<CompiledName("Point")>] Point 
    | [<CompiledName("LineString")>] LineString 
    | [<CompiledName("LinearRing")>] LinearRing
    | [<CompiledName("Polygon")>] Polygon 
    | [<CompiledName("MultiLineString")>] MultiLineString 
    | [<CompiledName("MultiPolygon")>] MultiPolygon 
    | [<CompiledName("GeometryCollection")>] GeometryCollection 
    | [<CompiledName("Circle")>] Circle

type Geometry =
    inherit BaseObject

    abstract getClosestPoint: point: Coordinate -> Coordinate

type SimpleGeometry =
    inherit Geometry

    abstract getType: unit -> GeometryType
    abstract getCoordinates: unit -> Coordinate
    abstract setCoordinates: Coordinate -> unit

type Point =
    inherit SimpleGeometry

    abstract getCoordinates: unit -> Coordinate
    abstract setCoordinates: Coordinate -> unit
    
type Circle =
    inherit SimpleGeometry

    abstract getCenter: unit -> Coordinate
    abstract getRadius: unit -> float

    abstract setCenter: Coordinate -> unit
    abstract setRadius: float -> unit


type PointStatic =
    [<Emit "new $0($1...)">] abstract Create: Coordinate -> Point

type CircleStatic =
    [<Emit "new $0($1...)">] abstract Create: Coordinate * float -> Circle