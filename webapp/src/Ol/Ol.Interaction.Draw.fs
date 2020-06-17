namespace Ol.Interaction.Draw

open Fable.Core
open Ol.Events
open Ol.Interaction.Pointer
open Ol.Source.Vector
open Ol.Geom
open Ol.Events
open Ol.Feature

type DrawEvent =
    inherit BaseEvent

    abstract feature: Feature

type DrawListenerFunction =
    DrawEvent -> unit

type Draw =
    inherit PointerInteraction

    abstract on: string * listener: DrawListenerFunction -> EventsKey

type DrawOptions =
    abstract source: VectorSource with get, set
    abstract ``type``: GeometryType with get, set

type DrawStatic =
    [<Emit "new $0($1...)">] abstract Create: DrawOptions -> Draw


type Events =
    | Drawabort
    | Drawend
    | Drawstart