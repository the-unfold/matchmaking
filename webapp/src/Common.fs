module Common

open Thoth.Json

// type declarations
[<Measure>] type km
[<Measure>] type deg

/// Широта и долгота. Общего назначения
type LonLat = {
    Lon: float
    Lat: float
}

let lonLatFromTuple t =
    {Lon = fst t; Lat = snd t}

let lonLatToTuple x =
    (x.Lon, x.Lat)
//  with
//     static member Decoder: Decoder<Position> =
//         Decode.map2 
//             (fun lat lon -> {Lat = lat * 1.0<deg>; Lon = lon * 1.0<deg>})
//             (Decode.field "lat" Decode.float)
//             (Decode.field "lon" Decode.float)

type Radius = float<km>

/// Сейчас используется для того, чтобы обозначить территорию,
/// на которой пользователь хочет собирать добычу и владеть самками
/// (на которой определено пожелание пользователя...)
type Area = {
    Center: LonLat
    Radius: Radius
}

type TokenType =
    TokenType of string

type TokenScope =
    TokenScope of string

type ApiToken = {
    AccessToken: string
    ExpiresIn: int
    TokenType: string
    Scope: string
}

type AuthUser = {
    Sub: string
    Name: string
    Email: string
}

type User = {
    Id: int
    AuthId: int
    Name: string
    ImageUrl: string option
}

type Tag = Tag of string
type Url = Url of string
type EventLocation = GeographicLocation of LonLat | EventUrl of Url

type Event = {
    Id: int
    Title: string
    Description: string
    Location: EventLocation
    DateTime: System.DateTime
    ImageUrl: string option
    Tags: Tag list
    Organizers: User list
    Attendees: User list 
}

type TagDto = {
    Id: int
    Name: string
    Slug: string
}

type EventDto = {
    Id: int
    Title: string
    Description: string
    LocationGeo: string option
    LocationUrl: string option
    DateTime: System.DateTime
    ImageUrl: string option
    Tags: TagDto list
    Organizers: User list
    Attendees: User list
}

let eventFromDto dto =
    let location =
        match dto.LocationGeo, dto.LocationUrl with
        | None, Some url -> url |> Url |> EventUrl |> Ok
        | Some lonLat, None -> Error "Geographic location not implemented"
        | _ -> Error "Missing event location"

    Result.map 
        (fun loc -> {
            Id = dto.Id
            Title = dto.Title
            Description = dto.Description
            Location = loc
            DateTime = dto.DateTime
            ImageUrl = dto.ImageUrl
            Tags = List.map (fun t -> Tag t.Name) dto.Tags
            Organizers = dto.Organizers
            Attendees = dto.Attendees
        })
        location
        
    