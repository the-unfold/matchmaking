module Utils

open Fable.SimpleHttp
open Thoth.Json
open System.Text.RegularExpressions

type Deferred<'t> =
    | NotStarted
    | InProgress
    | Resolved of 't

type AsyncOperationStatus<'t> =
    | Started
    | Finished of 't

let const' a _ = a

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(m.Value) else None

let inline decodeResponseAuto<'T> response =
     match response.statusCode, response.responseText with
        | 200, t -> Decode.Auto.fromString<'T> (t, caseStrategy=SnakeCase)
        | _, t -> Error t

let resultTraverse (f: 'a -> Result<'b, 'e>) (list: 'a list): Result<'b list, 'e> =
    let folder head tail = 
        Result.bind (fun h -> 
            Result.bind (fun t -> Ok (h::t)) tail
        ) (f head)

    List.foldBack folder list (Ok [])

[<RequireQualifiedAccess>]
module Validate =
    let required errMsg x =
        match x with
        | "" -> Error errMsg
        | _ -> Ok x

    let url errMsg x =
        match x with
        | Regex @"^https?://[^\s/$.?#].[^\s]*$" url -> Ok url
        | _ -> Error errMsg

    let dateTime errMsg x =
        match System.DateTime.TryParse x with
        | true, r -> Ok r
        | _ -> Error errMsg


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

[<RequireQualifiedAccess>]
module Image = 
    open Fable.Core.JsInterop
    let inline load (relativePath: string) : string = importDefault relativePath


[<RequireQualifiedAccess>]
module Stylesheet =
    open Fable.Core.JsInterop
    let inline apply (relativePath: string) : unit = importSideEffects relativePath
