#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib_RaylibHelper.fsx"
#load "Lib_SpatialTree.fsx"
open Raylib_cs
open System.Numerics
open Lib_RaylibHelper
open Lib_SpatialTree

type Point = {
    mutable Pos: Vector2
    Radius: float32
}

let screenWidth, screenHeight = 1200, 800
// let tree   = STree.create 32
let points = ResizeArray<_>([
    for i=1 to 10 do
        { Pos = vec2 (randf -600f 600f) (randf -400f 400f); Radius = 10f }
])

// Allows dragging points
let mutable drag = NoDrag

// Camera with 0,0 in screen center
let mutable camera = Camera2D()
camera.Offset   <- vec2 (float32 screenWidth / 2f) (float32 screenHeight / 2f)
camera.Target   <- vec2 0f 0f
camera.Rotation <- 0f
camera.Zoom     <- 1f

rl.InitWindow(screenWidth, screenHeight, "Hello, World!")
rl.SetMouseCursor(MouseCursor.Crosshair)
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse (Some camera)
    // Process Drageable
    drag <- processDrag drag points (fun p -> Circle (p.Pos,p.Radius)) mouse
    match drag with
    | StartDrag (point,off)
    | InDrag    (point,off) -> point.Pos <- worldPosition mouse
    | _ -> ()

    rl.BeginDrawing()

    // Draw World
    rl.BeginMode2D(camera)
    rl.ClearBackground(Color.Black)
    for point in points do
        rl.DrawCircleV(point.Pos, point.Radius, Color.Red)
    onHover drag (fun point -> rl.DrawCircleLinesV(point.Pos, point.Radius, Color.RayWhite))
    rl.EndMode2D()

    // Draw UI
    rl.DrawFPS(0,0)
    rl.DrawLine(0,400, 1200,400, Color.Red)
    rl.DrawLine(600,0, 600,800,  Color.Green)
    onHover drag (fun point -> rl.DrawText(sprintf "Point {%f,%f}" point.Pos.X point.Pos.Y, 10, 770, 24, Color.Yellow))

    rl.EndDrawing()

rl.CloseWindow()
