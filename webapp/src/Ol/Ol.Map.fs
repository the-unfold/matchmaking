namespace Ol.Map

open Fable.Core
open Ol.PluggableMap
open Ol.View
open Ol.BaseLayer
open Ol.Interaction

type Map =
    inherit PluggableMap

    abstract addInteraction: interaction: Interaction -> unit

type MapOptions = 
    abstract target: string with get, set
    abstract layers: BaseLayer array with get, set
    abstract view: View with get, set
    abstract interactions: Interaction array with get, set

type MapStatic =
    /// <param name="options">Map options.</param>
    [<Emit "new $0($1...)">] abstract Create: options: MapOptions -> Map