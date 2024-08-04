#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib_RaylibHelper.fsx"
open Raylib_cs
open Lib_RaylibHelper
open System.Numerics

let screenWidth, screenHeight = 800, 800

type MoveableRect = {
    mutable Rect: Rectangle
    Color:        Color
}

type Drageable =
    | InDrag of rect:MoveableRect * offset:Vector2
    | NoDrag

let moveables = [
    { Rect = rect 100f 100f 100f 100f; Color = Color.Yellow }
    { Rect = rect 200f 200f 100f 100f; Color = Color.Red    }
    { Rect = rect 300f 300f 100f 100f; Color = Color.Blue   }
]

let processMoveables mouse selection moveables =
    match selection, mouse.Left with
        | NoDrag, Down
        | NoDrag, Pressed ->
            let rec loop moveables =
                match moveables with
                | []               -> NoDrag
                | moveable :: rest ->
                    let rect = moveable.Rect
                    if toBool <| rl.CheckCollisionPointRec(mouse.Position, rect)
                    then InDrag (moveable, ((vec2 rect.X rect.Y) - mouse.Position))
                    else loop rest
            loop moveables
        | NoDrag, Up
        | NoDrag, Released -> NoDrag
        | InDrag (m,offset), Pressed
        | InDrag (m,offset), Down    ->
            let r = m.Rect
            m.Rect <- rect (mouse.Position.X+offset.X) (mouse.Position.Y+offset.Y) r.Width r.Height
            InDrag (m,offset)
        | InDrag _, Up
        | InDrag _, Released -> NoDrag


let mutable selection = NoDrag

rl.InitWindow(screenWidth, screenHeight, "Hello, World!")
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse ()

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    selection <- processMoveables mouse selection moveables

    for mov in moveables do
        rl.DrawRectangleRec(mov.Rect, mov.Color)

    rl.EndDrawing ()
rl.CloseWindow()
