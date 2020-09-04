module Utils

type Deferred<'t> =
    | NotStarted
    | InProgress
    | Resolved of 't

type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't
