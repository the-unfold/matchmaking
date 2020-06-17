namespace Ol.Source.OSM

open Fable.Core
open Ol.Source.XYZ

type OSM =
    inherit XYZ

type OSMStatic =
    [<Emit "new $0()">] abstract Create: unit -> OSM