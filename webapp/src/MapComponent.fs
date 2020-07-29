module MapComponent

open Browser.Dom
open Browser.Types
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props

open Ol.Map
open Ol.PluggableMap.PluggableMapExtentions
open OlImport
open Common

type Props = { 
    center: LonLat 
    zoom: float
}

let initMap lonLat zoom =
    // creating a basic map
    let vectorSource = vectorSourceStatic.Create !!{| wrapX = false |}


    let mapOptions = jsOptions<MapOptions>(fun x ->
        x.layers <- [| 
            tileLayerStatic.Create !!{|source = osmStatic.Create ()|} // {source: new OSM()}
            vectorLayerStatic.Create !!{|source = vectorSource|} 
        |]
        x.view <- viewStatic.Create !!{|center = fromLonLat (lonLat.Lon, lonLat.Lat); zoom = zoom|})

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