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

/// Lerps a value between start and stop. Expects a normalized value between 0 and 1.
/// when normalized value is 0 it returns start, when it turns 1 it returns stop.
let inline lerp start stop normalized =
    (start * (LanguagePrimitives.GenericOne - normalized)) + (stop * normalized)

let wrap (start:float32) (stop:float32) (value:float32) : float32 =
    let diff     = stop - start
    let quotient = floor ((value - start) / diff)
    value - (diff * quotient)

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

let inline color r g b a =
    let mutable c = Color()
    c.R <- byte r
    c.G <- byte g
    c.B <- byte b
    c.A <- byte a
    c

let lerpColor (start:Color) (stop:Color) (n:float32) =
    color
        (lerp (float32 start.R) (float32 stop.R) n)
        (lerp (float32 start.G) (float32 stop.G) n)
        (lerp (float32 start.B) (float32 stop.B) n)
        (lerp (float32 start.A) (float32 stop.A) n)

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
    | NoDrag
    | StartDrag of 'a * offset:Vector2
    | InDrag    of 'a * offset:Vector2
    | EndDrag   of 'a * offset:Vector2

type CollisionType =
    | Rect   of Rectangle
    | Circle of position:Vector2 * radius:float32

/// A Helper function to Drag any kind of object around.
/// `current` is the current variable that holds the current state of the drag.
/// `drageables` the objects that should be drageable
/// `toCollision` a function that creates a collision rectangle for a drageable object. This collision rectangle is used for mouse collision
/// `mouse` current state of mouse
/// `doAction` function that is executed when user Drags something. The drageable object is passed and an offset where the user clicked on the collision rect
/// returns the new state of the drageable state.
let processDrag current drageables toCollision mouse : Drageable<'a> =
    let checkCollision () =
        let mutable selected = NoDrag
        for drageable in drageables do
            match toCollision drageable with
            | Rect rect ->
                if toBool <| rl.CheckCollisionPointRec(mouse.Position, rect) then
                    selected <- StartDrag (drageable, (mouse.Position - (vec2 rect.X rect.Y)))
            | Circle (pos,radius) ->
                if toBool <| rl.CheckCollisionPointCircle(mouse.Position, pos, radius) then
                    selected <- StartDrag (drageable, (mouse.Position - (vec2 pos.X pos.Y)))
        selected

    // Some transistions seems odd as the should not happen. For example
    // StartDrag, Up should never happened. When the frame before the Mouse
    // was in a pressed State, then in the next frame there must be a Released
    // State. Logic tells us that his must happen. But computers and technology
    // fails. Sometimes it could be that some state is not correctly updated.
    // maybe mouse was unplugged? Battery run out so no state at all happend?
    // Maybe the user switched Application with Alt-Tab and Released the mouse
    // button there? Maybe the game itself interruped the player with a window
    // and goes into a freezing state without proberly checking state? Who knows
    // I have seen enough application/mouses where mouse drag/events wasn't
    // handled correctly, something is in drag state and a game/app didn't get
    // it correctly. So all States and State Transistions even if they seems
    // to be dump must be handled correctly.
    match current, mouse.Left with
    | NoDrag, Up                        -> NoDrag
    | NoDrag, Released                  -> NoDrag
    // I could handle both states differently and it would make a difference.
    // In this setup you can hold the mouse button down, and with the mouse
    // button down you can go over something drageable and it "correctly"
    // picks up the drag. But maybe this is wrong behaviour? Who knows.
    | NoDrag, (Down|Pressed)            -> checkCollision ()
    | StartDrag (drag,offset), Up       -> EndDrag (drag,offset)
    | StartDrag (drag,offset), Released -> EndDrag (drag,offset)
    // We go into EndDrag not to StartDrag or recheck collision because the
    // current State has something draged. So if for whatever reason a
    // Pressed was issues, something wrong must be happened. So we first must
    // issue an EndDrag until a new StartDrag can be processed. This gives
    // the program the chance to correctly end a StartDrag state.
    | StartDrag (drag,offset), Pressed  -> EndDrag (drag,offset)
    | StartDrag (drag,offset), Down     -> InDrag  (drag,offset)
    | InDrag (drag,offset), Up          -> EndDrag (drag,offset)
    // Again, some bad stuff happened
    | InDrag (drag,offset), Pressed     -> EndDrag (drag,offset)
    | InDrag (drag,offset), Down        -> InDrag (drag,offset)
    | InDrag (drag,offset), Released    -> EndDrag (drag,offset)
    | EndDrag _, Up                     -> NoDrag
    | EndDrag _, Down                   -> checkCollision ()
    | EndDrag _, Pressed                -> checkCollision ()
    | EndDrag _, Released               -> NoDrag
