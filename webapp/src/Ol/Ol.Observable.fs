namespace Ol.Observable

open Ol.Events

type Observable =
    inherit Target

    abstract on: string * listener: ListenerFunction -> EventsKey