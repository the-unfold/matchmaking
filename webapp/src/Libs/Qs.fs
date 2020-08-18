// ts2fable 0.7.1
module rec Qs
open System
open Fable.Core
open Fable.Core.JS

type Array<'T> = System.Collections.Generic.IList<'T>
type RegExp = System.Text.RegularExpressions.Regex

let [<Import("*","qs")>] queryString: QueryString.IExports = jsNative

module QueryString =

    type [<AllowNullLiteral>] IExports =
        abstract stringify: obj: obj * ?options: IStringifyOptions -> string

    type [<AllowNullLiteral>] defaultEncoder =
        [<Emit "$0($1...)">] abstract Invoke: str: obj option * ?defaultEncoder: obj * ?charset: string -> string

    type [<AllowNullLiteral>] defaultDecoder =
        [<Emit "$0($1...)">] abstract Invoke: str: string * ?decoder: obj * ?charset: string -> string

    type [<AllowNullLiteral>] IStringifyOptions =
        abstract delimiter: string option with get, set
        abstract strictNullHandling: bool option with get, set
        abstract skipNulls: bool option with get, set
        abstract encode: bool option with get, set
        abstract encoder: (obj option -> defaultEncoder -> string -> IStringifyOptionsEncoder -> string) option with get, set
        abstract filter: U2<Array<U2<string, float>>, (string -> obj option -> obj option)> option with get, set
        abstract arrayFormat: IStringifyOptionsArrayFormat option with get, set
        abstract indices: bool option with get, set
        abstract sort: (obj option -> obj option -> float) option with get, set
        abstract serializeDate: (DateTime -> string) option with get, set
        abstract format: IStringifyOptionsFormat option with get, set
        abstract encodeValuesOnly: bool option with get, set
        abstract addQueryPrefix: bool option with get, set
        abstract allowDots: bool option with get, set
        abstract charset: IStringifyOptionsCharset option with get, set
        abstract charsetSentinel: bool option with get, set

    type [<AllowNullLiteral>] IParseOptions =
        abstract comma: bool option with get, set
        abstract delimiter: U2<string, RegExp> option with get, set
        abstract depth: float option with get, set
        abstract decoder: (string -> defaultDecoder -> string -> IStringifyOptionsEncoder -> obj option) option with get, set
        abstract arrayLimit: float option with get, set
        abstract parseArrays: bool option with get, set
        abstract allowDots: bool option with get, set
        abstract plainObjects: bool option with get, set
        abstract allowPrototypes: bool option with get, set
        abstract parameterLimit: float option with get, set
        abstract strictNullHandling: bool option with get, set
        abstract ignoreQueryPrefix: bool option with get, set
        abstract charset: IStringifyOptionsCharset option with get, set
        abstract charsetSentinel: bool option with get, set
        abstract interpretNumericEntities: bool option with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] IStringifyOptionsEncoder =
        | Key
        | Value

    type [<StringEnum>] [<RequireQualifiedAccess>] IStringifyOptionsArrayFormat =
        | Indices
        | Brackets
        | Repeat
        | Comma

    type [<StringEnum>] [<RequireQualifiedAccess>] IStringifyOptionsFormat =
        | [<CompiledName "RFC1738">] RFC1738
        | [<CompiledName "RFC3986">] RFC3986

    type [<StringEnum>] [<RequireQualifiedAccess>] IStringifyOptionsCharset =
        | [<CompiledName "utf-8">] Utf8
        | [<CompiledName "iso-8859-1">] Iso88591
