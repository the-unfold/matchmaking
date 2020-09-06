module Utils

type Deferred<'t> =
    | NotStarted
    | InProgress
    | Resolved of 't

type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't

module AsyncResult =
    let map f ar = async {
        let! r = ar
        return Result.map f r
    }

    let bind ar f = async {
        let! r = ar
        match r with
        | Ok v -> return! f v
        | Error e -> return e |> Error
    }

    let lift v = async {
        return v |> Result.Ok 
    }

    type AsyncResultBuilder() =
        member __.Return(v) = lift v
        member __.ReturnFrom(v) = v
        member __.Bind(ar, f) = bind ar f

[<AutoOpen>]
module AsyncResultExpressionBuilder =
    let asyncResult = new AsyncResult.AsyncResultBuilder()