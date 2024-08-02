#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

// This programs allows drawing lines that then can be splited with
// the Koch Fractal up to a certain limit. The order in which lines are
// drawn is important in which direction the line is split up.
//
// The line is split up to the left. So when you draw a line from left
// to right. think of it as looking from the starting point where you
// started drawing to the end point. Then the left side gets extended.

module Line =
    // Helper function
    let private vec2 x y = Vector2(x,y)

    // Line.T
    type T = Line of start:Vector2 * stop:Vector2
    let create start stop = Line (start,stop)

    let start (Line (start,_)) = start
    let stop  (Line (_,stop))  = stop

    let normalize vec = Vector2.Normalize(vec)
    let length line   = Vector2.Distance(start line, stop line)

    /// returns the midpoint of a line
    let midpoint (Line (start,stop)) =
        (vec2 ((start.X + stop.X) / 2f) ((start.Y + stop.Y) / 2f))

    /// the center point is the new tip. it is calculated from the mid point and
    /// goes orthogonal up from the line
    let centerPoint line =
        let d = (stop line) - (start line)
        (midpoint line) + (normalize (vec2 d.Y -d.X)) * ((length line) / 3f)

    /// returns the left and right point where a line has to be splited
    let lrPoint line =
        let dir = (normalize ((stop line) - (start line)))
        let l   = dir * ((length line) / 3f)
        let r   = dir * ((length line) / 3f * 2f)
        (start line) + l,(start line) + r

    /// turns a single line into 4 new lines
    let splitLine input =
        let s      = start input
        let e      = stop input
        let center = centerPoint input
        let l,r    = lrPoint input
        [(create s l); (create l center); (create center r); (create r e)]

// Helper functions to create vector2 and a line
let vec2 x y = Vector2(x,y)
let line     = Line.create

// Annoying CBool in Raylib-cs. Most functions return a CBool. Not a problem in
// C# because of implicit type conversion. But F# has explicit type conversion
let inline toBool (cbool:CBool) : bool =
    CBool.op_Implicit cbool

// Example using F# DU to get Mouse Button State
type MouseState =
    | Up
    | Pressed
    | Down
    | Released

// Get State of a Mouse Button
let buttonState button =
    if   Raylib.IsMouseButtonPressed  button |> toBool then Pressed
    elif Raylib.IsMouseButtonDown     button |> toBool then Down
    elif Raylib.IsMouseButtonReleased button |> toBool then Released
    else Up

// Generetas a Koch Fractal Snowflake
Raylib.InitWindow(800, 800, "Snowflake")
Raylib.SetTargetFPS(60)

// Lines to Draw
// Note: For Beginners. This creates an immutable List. List stays immutable.
//       adding `mutable` only makes the variable itself mutable. So we can
//       swap out one immutable list with another immutable list.
let mutable lines = []

// Again a DU. Basically works like a State Machine.
type MouseSelection =
    | NotStarted
    | Start  of Vector2
    | Drag   of Vector2 * Vector2
    | Finish of Vector2 * Vector2
let mutable selection = NotStarted

// Game Loop
while not <| CBool.op_Implicit (Raylib.WindowShouldClose()) do
    let mousePos = Raylib.GetMousePosition()

    // Partial Application of MouseButton
    let left = buttonState MouseButton.Left
    selection <-
        match selection, left with
        | NotStarted,   Pressed  -> Start mousePos
        | NotStarted,   Down     -> Start mousePos
        | NotStarted,   Released -> NotStarted
        | NotStarted,   Up       -> NotStarted
        | Start _   ,   Pressed  -> Start mousePos
        | Start s,      Down     -> Drag (s,mousePos)
        | Start s,      Released -> Finish (s,mousePos)
        | Start s,      Up       -> Finish (s,mousePos)
        | Drag  (_,_),  Pressed  -> Start mousePos
        | Drag  (s,_),  Down     -> Drag (s,mousePos)
        | Drag  (s,_),  Released -> Finish (s,mousePos)
        | Drag  (s,_),  Up       -> Finish (s,mousePos)
        | Finish (_,_), Pressed  -> Start mousePos
        | Finish (_,_), Down     -> Start mousePos
        | Finish (_,_), Released -> NotStarted
        | Finish (_,_), Up       -> NotStarted

    Raylib.BeginDrawing()
    Raylib.ClearBackground(Color.Black)

    match selection with
    | NotStarted  -> ()
    | Start _     -> ()
    | Drag (s,e)  -> Raylib.DrawLineEx(s, e, 1f, Color.RayWhite)
    | Finish(s,e) -> lines <- (line s e) :: lines

    // Draw "split" button
    let rect = Rectangle(250f, 10f, 100f, 30f)
    Raylib.DrawRectangleRec(rect, Color.Gray)
    Raylib.DrawRectangleLinesEx(rect, 1f, Color.White)
    Raylib.DrawText("Split", 250+27, 13, 24, Color.Black)

    if left = Pressed && (Raylib.CheckCollisionPointRec(mousePos, rect) |> toBool) then
        lines <- lines |> List.collect (fun line ->
            // only split lines into more segments if line is longer than 10px
            if Line.length line > 10f then Line.splitLine line else [line]
        )

    // Draw "clear" button
    let rect = Rectangle(400f, 10f, 100f, 30f)
    Raylib.DrawRectangleRec(rect, Color.Gray)
    Raylib.DrawRectangleLinesEx(rect, 1f, Color.White)
    Raylib.DrawText("Clear", 400+22, 13, 24, Color.Black)

    if left = Pressed && (Raylib.CheckCollisionPointRec(mousePos, rect) |> toBool) then
        lines <- []

    // Draws the lines
    for line in lines do
        Raylib.DrawLineEx((Line.start line), (Line.stop line), 1f, Color.Blue)

    Raylib.EndDrawing()

Raylib.CloseWindow ()

