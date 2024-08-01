#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

let color r g b a =
    let mutable c = Color()
    c.R <- r
    c.G <- g
    c.B <- b
    c.A <- a
    c

let mutable current = Vector2(400f, 400f)
let mutable target  = Vector2(400f, 400f)

Raylib.InitWindow(800, 800, "Render Texture")
Raylib.SetTargetFPS(60)
while not <| CBool.op_Implicit (Raylib.WindowShouldClose()) do
    let dt = Raylib.GetFrameTime()
    target  <- Raylib.GetMousePosition()
    current <- Vector2.Lerp(current, target, dt)

    Raylib.BeginDrawing ()
    Raylib.ClearBackground(Color.Black)

    // Draw circle that follows mouse cursor
    Raylib.DrawCircleV(current, 30f, Color.DarkBlue)
    Raylib.DrawCircleV(current,  2f, Color.DarkBrown)

    // Draw mouse cursor and line
    Raylib.DrawCircleV(target, 5f, Color.Yellow)
    Raylib.DrawLineV(current, target, Color.RayWhite)
    Raylib.EndDrawing ()

Raylib.CloseWindow()
