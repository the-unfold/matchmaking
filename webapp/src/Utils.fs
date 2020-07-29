module Utils

open Browser.Types
open Fable.React

type Deferred<'t> =
    | NotStarted
    | InProgress
    | Resolved of 't

type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't
