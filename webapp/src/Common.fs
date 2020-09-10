module Common

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

type User = {
    Sub: string
    Name: string
    Email: string
    Image: string option
}

type Tag = Tag of string
type Url = Url of string
type EventLocation = GeographicLocation of LonLat | EventUrl of Url

type Event = {
    Title: string
    Description: string
    Location: EventLocation
    DateTime: System.DateTime
    Tags: Tag list
    Organizers: User list
    Attendees: User list
}