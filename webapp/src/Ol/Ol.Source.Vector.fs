namespace Ol.Source.Vector

open Fable.Core
open Ol.Source

type VectorSource =
    inherit Source

type VectorSourceOptions =
    abstract wrapX: bool with get, set

type VectorSourceStatic =
    [<Emit "new $0($1...)">] abstract Create: VectorSourceOptions -> VectorSource