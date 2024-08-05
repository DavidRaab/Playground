#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

// Type alias for Raylib
type rl = Raylib

// Annoying CBool in Raylib-cs. Most functions return a CBool. Not a problem in
// C# because of implicit type conversion. But F# has explicit type conversion
let inline toBool (cbool:CBool) : bool =
    CBool.op_Implicit cbool

let inline vec2 x y     = Vector2(x,y)
let inline rect x y w h = Rectangle(x,y,w,h)
let rng                 = System.Random ()
let nextI min max       = rng.Next(min,max)
let nextF min max       = min + (rng.NextSingle() * (max-min))

let color r g b a =
    let mutable c = Color()
    c.R <- r
    c.G <- g
    c.B <- b
    c.A <- a
    c

// A nicer way to get Mouse State
type MouseButtonState =
    | Up
    | Pressed
    | Down
    | Released

type MouseState = {
    Position: Vector2
    Wheel:    float32
    Left:     MouseButtonState
    Middle:   MouseButtonState
    Right:    MouseButtonState
}

// Get State of a Mouse Button
let getMouseButtonState button =
    if   Raylib.IsMouseButtonPressed  button |> toBool then Pressed
    elif Raylib.IsMouseButtonReleased button |> toBool then Released
    elif Raylib.IsMouseButtonDown     button |> toBool then Down
    else Up

let getMouse () = {
    Position = Raylib.GetMousePosition ()
    Wheel    = Raylib.GetMouseWheelMove ()
    Left     = getMouseButtonState MouseButton.Left
    Middle   = getMouseButtonState MouseButton.Middle
    Right    = getMouseButtonState MouseButton.Right
}

let guiButton (rect:Rectangle) (text:string) : bool =
    let mouse = getMouse()

    let isHover = Raylib.CheckCollisionPointRec(mouse.Position, rect) |> toBool
    if isHover
    then Raylib.DrawRectangleRec(rect, Color.LightGray)
    else Raylib.DrawRectangleRec(rect, Color.Gray)

    let fontSize = 24
    let tw       = float32 <| Raylib.MeasureText(text, fontSize)
    let yText    = rect.Y + (rect.Height / 2f - (float32 fontSize / 2f))
    let xText    = rect.X + (rect.Width - tw) / 2f
    Raylib.DrawText(text, int xText, int yText, fontSize, Color.Black)
    Raylib.DrawRectangleLinesEx(rect, 2f, Color.White)

    if mouse.Left = Pressed then isHover else false