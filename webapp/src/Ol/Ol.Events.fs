namespace Ol.Events

open Fable.Core
open Ol

type BaseEvent =
    abstract ``type``: string
    abstract target: obj
    abstract preventDefault: unit -> unit
    abstract stopPropagation: unit -> unit

type ListenerFunction =
    BaseEvent -> unit

type Target =
    inherit Disposable

type EventsKey =
    abstract ``type``: string
    abstract listener: ListenerFunction
    abstract target: obj



