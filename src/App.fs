module App

open Browser.Dom
open Fable.Core
open Fable.React
open Fable.React.Helpers

[<Import("Map", from="ol")>]
type Map()=
    class
    end

[<Import("View", from="ol")>]
type View()=
    class
    end

[<Import("TileLayer", from="ol/layer/Tile")>]
type TileLayer()=
    class
    end

[<Import("OSM", from="ol/source/OSM")>]
type OSM()=
    class
    end

let initMap() = Map()

let init()=
    let element = str "root"
    ReactDom.render (element, document.getElementById "root")
    
init()