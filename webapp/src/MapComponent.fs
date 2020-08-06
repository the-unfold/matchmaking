module MapComponent

open Browser.Dom
open Browser.Types
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props

open Ol.Map
open Ol.PluggableMap.PluggableMapExtentions
open Ol.Layer.Tile
open Ol.Layer.Vector
open Ol.View
open Ol.Source.Vector
open OlImport
open Common

type Props = { 
    center: LonLat 
    zoom: float
}

let initMap lonLat zoom =
    // creating a basic map
    let vectorSourceOptions = jsOptions<VectorSourceOptions>(fun x -> x.wrapX <- false)
    let vectorSource = vectorSourceStatic.Create vectorSourceOptions
    let tileLayerOptions = jsOptions<TileLayerOptions>(fun x -> x.source <- osmStatic.Create ()) // {source: new OSM()}
    let vectorLayerOptions = jsOptions<VectorLayerOptions>(fun x -> x.source <- vectorSource)
    let viewOptions = jsOptions<ViewOptions>(fun x -> 
        x.center <- (lonLat.Lon, lonLat.Lat)
        x.zoom <- zoom
    )
    let mapOptions = jsOptions<MapOptions>(fun x ->
        x.layers <- [| 
            tileLayerStatic.Create tileLayerOptions 
            vectorLayerStatic.Create vectorLayerOptions 
        |]
        x.view <- viewStatic.Create viewOptions)

    let theMap = mapStatic.Create mapOptions
    // let draw = drawStatic.Create !!{|source = vectorSource; ``type``= Point|}

    theMap

let olMap = initMap {Lon = 82.921733; Lat = 55.029910} 14.0
// olMap.setTarget(document.getElementById("map"))

type MapComponent(props) =
    inherit Component<Props, obj>(props)
    do base.setInitState(props)

    let olMap = initMap props.center props.zoom

    let mutable element: Element = unbox null
    let clickEventsKey = olMap.onClick (fun e -> console.log e)

    override this.render() =
        div [Class "map"; Ref (fun e -> element <- e)] []

    override this.componentDidMount() =
        olMap.setTarget element

    override this.componentWillUnmount() =
        unByKey clickEventsKey
        console.log("component will unmount")