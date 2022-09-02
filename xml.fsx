#!/usr/bin/env -S dotnet fsi

open System.Xml.Linq

[<StructuredFormatDisplay("Rect({x},{y},{width},{height})")>]
type Rect(x:int,y:int,width:int,height:int) =
    member this.x      = x
    member this.y      = y
    member this.width  = width
    member this.height = height


type Texture = {
    Name: string
    Rect: Rect
}

module XElement =
    let attr name (xelement:XElement) =
        xelement.Attribute(XName.op_Implicit name).Value

    let attrInt name x =
        int (attr name x)

    let attrs (x:XElement) =
        x.Attributes ()
        |> Seq.map (fun attr -> attr.Name.LocalName, attr.Value)
        |> dict

    let attrsIntMany names (x:XElement) = [
        for name in names do
            yield attrInt name x
    ]

let getTextures (xml:XDocument) = [
    for tex in xml.Descendants "SubTexture" do
        let a = XElement.attrsIntMany ["x";"y";"width";"height"] tex
        {
            Name = XElement.attr "name" tex
            Rect = Rect(a.[0], a.[1], a.[2], a.[3])
        }
]

let xml      = XDocument.Load("example.xml")
let textures = getTextures xml
printfn "%A" textures
