#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib_RaylibHelper.fsx"
open Raylib_cs
open Lib_RaylibHelper
open System.Numerics

let screenWidth, screenHeight = 800, 800

// Definition of a Moveable, also could just be a Rectangle, but then a function
// needs to pass a byref. So instead of this i turn it into its own reference
// type. At least assigning an additional Color makes it somehow more useful.
type MoveableRect = {
    mutable Rect: Rectangle
    Color:        Color
}

type Drageable =
    | InDrag of rect:MoveableRect * offset:Vector2
    | NoDrag

let processMoveables selection moveables mouse =
    match selection, mouse.Left with
        | NoDrag,   (Up|Released) -> NoDrag
        | InDrag _, (Up|Released) -> NoDrag
        | NoDrag, (Down|Pressed) ->
            let mutable selected = NoDrag
            for moveable in moveables do
                let r = moveable.Rect
                if toBool <| rl.CheckCollisionPointRec(mouse.Position, r) then
                    selected <- InDrag (moveable, ((vec2 r.X r.Y) - mouse.Position))
            selected
        | InDrag (m,offset), (Down|Pressed) ->
            let r = m.Rect
            m.Rect <- rect (mouse.Position.X+offset.X) (mouse.Position.Y+offset.Y) r.Width r.Height
            InDrag (m,offset)

let moveables = [
    { Rect = rect 100f 100f 100f 100f; Color = Color.Yellow }
    { Rect = rect 200f 200f 100f 100f; Color = Color.Red    }
    { Rect = rect 300f 300f 100f 100f; Color = Color.Blue   }
]

let mutable selection = NoDrag

rl.InitWindow(screenWidth, screenHeight, "Hello, World!")
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse ()

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    selection <- processMoveables selection moveables mouse

    for mov in moveables do
        rl.DrawRectangleRec(mov.Rect, mov.Color)

    rl.EndDrawing ()
rl.CloseWindow()
