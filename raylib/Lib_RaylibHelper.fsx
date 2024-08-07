#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

// Type alias for Raylib
type rl = Raylib

// Better reference equality
let isSame x y = LanguagePrimitives.PhysicalEquality x y

// Annoying CBool in Raylib-cs. Most functions return a CBool. Not a problem in
// C# because of implicit type conversion. But F# has explicit type conversion
let inline toBool (cbool:CBool) : bool =
    CBool.op_Implicit cbool

// Constants: multiply by this constants to transform deg->rad or vice-versa
let deg2rad             = System.MathF.Tau / 360f
let rad2deg             = 360f / System.MathF.Tau
let inline vec2 x y     = Vector2(x,y)
let inline rect x y w h = Rectangle(x,y,w,h)
let rng                 = System.Random ()
let randI min max       = rng.Next(min,max)
let randF min max       = min + (rng.NextSingle() * (max-min))
let rand  min max       = min + (rng.NextDouble() * (max-min))
let randomOf (array:array<'a>) =
    array.[randI 0 array.Length]

module Vector2 =
    // rotation matrix:
    // cos(a)  -sin(a)
    // sin(a)   cos(a)
    /// Rotates a Vector by the origin (0,0). Rotation direction depends on how you
    /// view the world. In a game where +X means right and +Y means down because (0,0)
    /// is the TopLeft corner of the screen. Then rotating by +90° rotates something
    /// anti-clockswise.
    let rotate (rad:float32) (v:Vector2) =
        let x = (v.X *  (cos rad)) + (v.Y * (sin rad))
        let y = (v.X * -(sin rad)) + (v.Y * (cos rad))
        Vector2(x,y)

    let rotateDeg (deg:float32) (v:Vector2) =
        rotate (deg * deg2rad) v

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

type Drageable<'a> =
    | InDrag of 'a * offset:Vector2
    | NoDrag

/// A Helper function to Drag any kind of object around.
/// `current` is the current variable that holds the current state of the drag.
/// `drageables` the objects that should be drageable
/// `toCollision` a function that creates a collision rectangle for a drageable object. This collision rectangle is used for mouse collision
/// `mouse` current state of mouse
/// `doAction` function that is executed when user Drags something. The drageable object is passed and an offset where the user clicked on the collision rect
/// returns the new state of the drageable state.
let processDrag current drageables toCollision mouse : Drageable<'a> =
    match current, mouse.Left with
    | NoDrag,   (Up|Released) -> NoDrag
    | InDrag _, (Up|Released) -> NoDrag
    | NoDrag, (Down|Pressed)  ->
        let mutable selected = NoDrag
        for drageable in drageables do
            let rect = toCollision drageable
            if toBool <| rl.CheckCollisionPointRec(mouse.Position, rect) then
                selected <- InDrag (drageable, (mouse.Position - (vec2 rect.X rect.Y)))
        selected
    | InDrag (drageable,offset), (Down|Pressed) ->
        InDrag (drageable,offset)
