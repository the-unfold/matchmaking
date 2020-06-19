module App

open Fable.Core
open Fable.Core.JsInterop
open Browser.Dom
open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React
open Fetch

open Ol.Coordinate
open Ol.Geom
open Ol.Source.Vector
open Ol.Source.OSM
open Ol.Layer.Tile
open Ol.Layer.Vector
open Ol.Interaction.Draw
open Ol.Feature
open Ol.View
open Ol.Map
open Ol.PluggableMap
open Ol.PluggableMap.PluggableMapExtentions

importAll "ol/ol.css"

[<ImportDefault("ol/Map")>]
let mapStatic: MapStatic = jsNative

[<ImportDefault("ol/View")>]
let viewStatic: ViewStatic = jsNative

[<ImportDefault("ol/layer/Tile")>]
let tileLayerStatic: TileLayerStatic = jsNative

[<ImportDefault("ol/layer/Vector")>]
let vectorLayerStatic: VectorLayerStatic = jsNative

[<ImportDefault("ol/source/OSM")>]
let osmStatic: OSMStatic = jsNative

[<ImportDefault("ol/source/Vector")>]
let vectorSourceStatic: VectorSourceStatic = jsNative

[<ImportDefault("ol/interaction/Draw")>]
let drawStatic: DrawStatic = jsNative

[<ImportDefault("ol/Feature")>]
let featureStatic: FeatureStatic = jsNative

[<ImportDefault("ol/geom/Point")>]
let pointStatic: PointStatic = jsNative

[<ImportDefault("ol/geom/Circle")>]
let circleStatic: CircleStatic = jsNative

[<Import("fromLonLat", "ol/proj")>]
/// Transforms a coordinate from longitude/latitude to a different projection.
let fromLonLat: float * float -> Coordinate = jsNative

[<Import("toLonLat", "ol/proj")>]
/// Transforms a coordinate to longitude/latitude.
let toLonLat: Coordinate -> float * float = jsNative

let vectorSource = vectorSourceStatic.Create !!{| wrapX = false |}

let mapOptions = jsOptions<MapOptions>(fun x -> 
    x.target <- "map"
    x.layers <- [| 
        tileLayerStatic.Create !!{|source = osmStatic.Create ()|}
        vectorLayerStatic.Create !!{|source = vectorSource|} 
    |]
    
    x.view <- viewStatic.Create !!{|center = fromLonLat (82.921733, 55.029910); zoom = 16.0|})

let theMap = mapStatic.Create mapOptions
// let draw = drawStatic.Create !!{|source = vectorSource; ``type``= Point|}

[<Measure>] type km
[<Measure>] type deg

type Position = {
    Lon: float<deg>
    Lat: float<deg>
}

type Radius = float<km>

type Area = {
    Center: Position
    Radius: Radius
}

type AreaFeatures = {
    CenterFeature: Feature
    RadiusFeature: Feature
}

let lonLatToPosition (x: float * float): Position =
    { Lon = fst x * 1.0<deg>; Lat = snd x * 1.0<deg> }

let positionToLonLat (p: Position): float * float =
    (float p.Lon, float p.Lat)

let addPoint (vs: VectorSource) (p: Position) =
    let coords = 
        p |> positionToLonLat 
        |> fromLonLat 
    let point = pointStatic.Create coords
        
    let feature = featureStatic.Create point

    vs.addFeature feature
    point

let movePoint (p: Point) (pos: Position) =
    let coords = pos |> positionToLonLat |> fromLonLat
    p.setCoordinates coords
    coords

let addCircle (vs: VectorSource) (rp: Radius * Point) =
    let coords = (snd rp).getCoordinates ()
    let radius = (rp |> fst |> float) * 1000.0

    let circle = circleStatic.Create (coords, radius)
    let feature = featureStatic.Create circle
    console.log feature
    vs.addFeature feature
    circle

let setAreaFeatures (vs: VectorSource) (area: Area) =
    let coords = area.Center |> positionToLonLat |> fromLonLat
    let radius = (area.Radius |> float) * 1000.0

    let point = pointStatic.Create coords
    let circle = circleStatic.Create (coords, radius)

    let areaFeatures = {
        CenterFeature = featureStatic.Create point
        RadiusFeature = featureStatic.Create circle
    }

    vs.addFeature areaFeatures.CenterFeature
    vs.addFeature areaFeatures.RadiusFeature

    areaFeatures

let removeAreaFeatures (vs: VectorSource) (af: AreaFeatures option) =
    match af with
    | Some x -> 
        vs.removeFeature x.CenterFeature
        vs.removeFeature x.RadiusFeature
    | None -> ()

let changeAreaRadius (af: AreaFeatures) (r: Radius) =
    let circle = af.RadiusFeature.getGeometry() :?> Circle
    let rad = (r |> float) * 1000.0
    circle.setRadius rad
    r

type Model = {
    area: Area option
    areaFeatures: AreaFeatures option
    text: string
}

type Msg =
    | SetArea of Area
    | SetAreaSuccess of AreaFeatures
    | SetRadius of Radius
    | SetRadiusSuccess of Radius
    | Done
    | TestResponse of string

let init () = ({
        area = None
        areaFeatures = None
        text = ""
    }, Cmd.none)

let update (msg: Msg) (model: Model) =
    match msg with
    | SetArea a -> 
        { model with area = Some a }, 
        Cmd.batch [
            Cmd.OfFunc.perform (removeAreaFeatures vectorSource) model.areaFeatures (fun _ -> Done)
            Cmd.OfFunc.perform (setAreaFeatures vectorSource) a SetAreaSuccess
        ]
        
    | SetAreaSuccess af -> 
        { model with areaFeatures = Some af },
        Cmd.none
    | SetRadius r -> 
        match model.area with
        | Some a -> 
            { model with area = Some { Center = a.Center; Radius = r } },
            Cmd.OfFunc.perform (fun (af, rad) -> changeAreaRadius af rad) (model.areaFeatures.Value, r) SetRadiusSuccess
        | None -> model, Cmd.none
    | SetRadiusSuccess r -> model, Cmd.none
    | TestResponse txt -> { model with text = txt }, Cmd.none
    | Done -> model, Cmd.none

let withFallbackMessage<'T> (fallbackMsg: string) (f: 'T -> string) (x: 'T option): string =
    match x with
    | Some t -> f t
    | None -> fallbackMsg

let formatPosition (x: Area) =
    sprintf "Lon: %f, Lat: %f" x.Center.Lon x.Center.Lat

let formatRadius (x: Area) =
    sprintf "Radius: %.1fkm" x.Radius

let view model dispatch =
    div [] [
        div [] [
            span [] [ 
                model.area |> 
                withFallbackMessage "No location" formatPosition |> 
                str 
            ]
        ]
        div [] [
            label [] [ model.area |> withFallbackMessage "" formatRadius |> str ]
            br []
            input [ 
                Type "range"; 
                Min 0.1; 
                Max 100; 
                Step 0.1;
                Class "slider-default";
                Style [Width "400px"]
                OnChange (fun x -> x.Value |> float |> fun x -> x * 1.0<km> |> SetRadius |> dispatch) 
            ]
        ]
        div [] [
            button [ 
                OnClick (fun _ -> 
                    let a = (fetch "https://localhost:5001/" [])
                    a |> Promise.bind (fun x -> x.text()) |> Promise.map (TestResponse >> dispatch) |> ignore
                )
            ] [ 
                str "Test Request" 
            ]
        ]
        div [] [
            label [] [ str model.text ]
        ]
    ]

let mapClickToDefaultArea (evt: MapBrowserEvent) =
    {
        Center = evt.coordinate |> toLonLat |> lonLatToPosition
        Radius = 0.5<km>
    }

let mapSub (initial: Model) =
    let sub dispatch =
        theMap.onClick (mapClickToDefaultArea >> SetArea >> dispatch) |> ignore
    
    Cmd.ofSub sub


Program.mkProgram init update view
|> Program.withReactBatched  "app-root"
|> Program.withConsoleTrace
|> Program.withSubscription mapSub
|> Program.run