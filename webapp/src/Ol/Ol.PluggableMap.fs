namespace Ol.PluggableMap

open Fable.Core
open Ol.Object
open Ol.Events
open Ol.Coordinate
open Ol.Pixel

type PluggableMap =
    inherit BaseObject

[<StringEnum>]
type PluggableMapEventTypes =
    | Click
    | Dbclick
    | Moveend
    | Movestart
    | Pointerdrag
    | Pointermove
    | Postcompose
    | Postrender
    | Precompose
    | Propertychange
    | Rendercomplete
    | Singleclick



type MapEvent =
    inherit BaseEvent

    abstract map: PluggableMap
    abstract ``type``: PluggableMapEventTypes

type MapBrowserEvent =
    inherit MapEvent

    abstract coordinate: Coordinate
    abstract dragging: bool
    abstract pixel: Pixel

module PluggableMapExtentions =
    let upcastOnArgs<'T when 'T :> BaseEvent> (evtName: PluggableMapEventTypes) (x: PluggableMap) (f: 'T -> unit) : EventsKey =
        x.on(evtName.ToString(), fun evt -> (f (evt :?> 'T)))

    type PluggableMap with 
        member this.onClick = upcastOnArgs<MapBrowserEvent> Click this
        member this.movestart = upcastOnArgs<MapEvent> Movestart this
        member this.moveend = upcastOnArgs<MapEvent> Moveend this