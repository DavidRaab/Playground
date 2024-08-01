#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
open Raylib_cs
open System.Numerics

Raylib.InitWindow(800, 800, "Render Texture")

let rt  = Raylib.LoadRenderTexture(800, 800)
// Size should be 800,800, but Height must be flipped
let src = Rectangle(0f, 0f, float32 rt.Texture.Width, float32 -rt.Texture.Height)
let dst = Rectangle(0f, 0f, 800f,  800f)

Raylib.SetTargetFPS(60)
while not <| CBool.op_Implicit (Raylib.WindowShouldClose()) do
    // Now all drawing operations draws to an Texture on the GPU
    Raylib.BeginTextureMode(rt)
    if CBool.op_Implicit <| Raylib.IsMouseButtonDown(MouseButton.Left) then
        Raylib.DrawCircleV(Raylib.GetMousePosition(), 2f, Color.RayWhite)
    Raylib.EndTextureMode()

    // we draw just the texture
    Raylib.BeginDrawing ()
    Raylib.ClearBackground(Color.Black)
    Raylib.DrawTexturePro(rt.Texture, src, dst, Vector2.Zero, 0f, Color.White)
    Raylib.EndDrawing ()

Raylib.CloseWindow()
