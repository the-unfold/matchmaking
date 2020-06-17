namespace Ol.View

open Fable.Core
open Ol.Object
open Ol.Coordinate

type View =
    inherit BaseObject

    abstract getCenter: unit -> Coordinate
    abstract setCenter: center: Coordinate -> unit
    abstract getZoom: unit -> float
    abstract setZoom: zoom: float -> unit

type ViewOptions = 
    abstract center: float * float with get, set
    abstract zoom: float with get, set

type ViewStatic =
    [<Emit "new $0($1...)">] abstract Create: options: ViewOptions -> View