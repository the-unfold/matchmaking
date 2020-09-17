[<RequireQualifiedAccess>]
module MapComponent

open Browser.Dom
open Browser.Types
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props

open Ol.Geom
open Ol.Map
open Ol.PluggableMap.PluggableMapExtentions
open Ol.Layer.Tile
open Ol.Layer.Vector
open Ol.View
open Ol.Source.Vector
open OlImport
open Common

type Msg =
    | Clicked of LonLat

type Props = { 
    center: LonLat 
    zoom: float
    dispatch: Msg -> unit
    areas: Area list
}


let private initMap lonLat zoom =
    // creating a basic map
    let vectorSourceOptions = jsOptions<VectorSourceOptions>(fun x -> x.wrapX <- false)
    let vectorSource = vectorSourceStatic.Create vectorSourceOptions
    let tileLayerOptions = jsOptions<TileLayerOptions>(fun x -> x.source <- osmStatic.Create ()) // {source: new OSM()}
    let vectorLayerOptions = jsOptions<VectorLayerOptions>(fun x -> x.source <- vectorSource)
    let viewOptions = jsOptions<ViewOptions>(fun x -> 
        x.center <- (fromLonLat << lonLatToTuple) lonLat
        x.zoom <- zoom
    )

    let tileLayer = tileLayerStatic.Create tileLayerOptions 
    let vectorLayer = vectorLayerStatic.Create vectorLayerOptions 

    let mapOptions = jsOptions<MapOptions>(fun x ->
        x.layers <- [| tileLayer; vectorLayer |]
        x.view <- viewStatic.Create viewOptions)

    let theMap = mapStatic.Create mapOptions
    // let draw = drawStatic.Create !!{|source = vectorSource; ``type``= Point|}

    theMap, vectorSource

// let mapComponentMemoize (a: MapComponentProps) (b: MapComponentProps) =
//     console.log (a, b)
//     abs (a.center.Lon - b.center.Lon) > 0.00000001 ||
//     abs (a.center.Lat - b.center.Lat) > 0.00000001 ||
//     abs (a.zoom - b.zoom) > 0.0001

let view (props: Props) = 
    let mapRef = Hooks.useRef (initMap props.center props.zoom)
    let mapElementRef = Hooks.useRef None
    Hooks.useEffect((fun () -> 
        let (map,_) = mapRef.current
        match mapElementRef.current with
        | Some e -> map.setTarget e
        | None -> raise (System.Exception "Unable to initialize the map. Missing dom element")
    ), [||])
    Hooks.useEffectDisposable((fun () -> 
        let (map,_) = mapRef.current
        let sub = map.onClick (fun e -> 
            e.coordinate |> toLonLat |> lonLatFromTuple |> Clicked |> props.dispatch)
        { new System.IDisposable with 
            member __.Dispose() = unByKey sub }
    ), [||])
    Hooks.useEffect((fun () -> 
        let (map, vectorSource) = mapRef.current
        let areaCoordinate area =
            area.Center |> lonLatToTuple |> fromLonLat
        let areaToPointFeature = areaCoordinate >> pointStatic.Create >> featureStatic.Create
        let areaToCircleFeature area =
            circleStatic.Create (areaCoordinate area, (float area.Radius) * 1000.0) |> featureStatic.Create

        vectorSource.clear true
        let pointFeatures = props.areas |> List.map areaToPointFeature |> List.toArray
        let circleFeatures = props.areas |> List.map areaToCircleFeature |> List.toArray
        
        vectorSource.addFeatures pointFeatures
        vectorSource.addFeatures circleFeatures
    ), [|props.areas|] )

    div [Class "map"; RefHook mapElementRef] []
