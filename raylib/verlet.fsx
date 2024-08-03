#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib_RaylibHelper.fsx"
open Raylib_cs
open Lib_RaylibHelper
open System.Numerics

// Some Resources to Watch:
// https://www.youtube.com/watch?v=-GWTDhOQU6M  --  Verlet Integration
// https://www.youtube.com/watch?v=lS_qeBy3aQI  --  Writing a Physics Engine from Scratch

// Some constants / Game state
let screenWidth, screenHeight    = 1200, 800
let defaultSpeed                 = 100f
let circleAmount                 = 1000
let gravityAmount                = 20f
let circleMinSize, circleMaxSize = 5f, 10f
let mutable showVelocity         = false

// Helper functions
let isSame x y    = LanguagePrimitives.PhysicalEquality x y
let rng           = System.Random ()
let nextI min max = rng.Next(min,max)
let nextF min max = min + (rng.NextSingle() * (max-min))

// Data-structures
type Circle = {
    mutable Position: Vector2
    mutable Velocity: Vector2
    Acceleration: Vector2
    Radius:       float32
    Color:        Color
}

module Circle =
    let randomCircle (speed:float32) = {
        Position     = vec2 (nextF 0f (float32 screenWidth)) (nextF 0f (float32 screenHeight))
        Velocity     = (vec2 (nextF -1f 1f) (nextF -1f 1f)) * speed
        Acceleration = (vec2 0f 9.81f) * gravityAmount
        Radius       = nextF circleMinSize circleMaxSize
        Color        =
            match nextI 0 5 with
            | 0 -> Color.DarkBlue
            | 1 -> Color.Orange
            | 2 -> Color.Purple
            | 3 -> Color.SkyBlue
            | 4 -> Color.DarkGreen
    }

    let update circle (dt:float32) =
        circle.Position <- circle.Position + (circle.Velocity * dt)
        circle.Velocity <- circle.Velocity + (circle.Acceleration * dt)

    let draw circle =
        Raylib.DrawCircle (int circle.Position.X, int circle.Position.Y, circle.Radius, circle.Color)
        if showVelocity then
            Raylib.DrawLine (
                int circle.Position.X, int circle.Position.Y,
                int (circle.Position.X + circle.Velocity.X),
                int (circle.Position.Y + circle.Velocity.Y),
                Color.RayWhite
            )

    let resolveCollision circle circles =
        for other in circles do
            if isSame circle other then
                ()
            else
                let toOther        = other.Position - circle.Position
                let distance       = toOther.Length ()
                let neededDistance = circle.Radius + other.Radius
                if distance >= neededDistance then
                    ()
                else
                    let toOther     = Vector2.Normalize(toOther)
                    let overlap     = (neededDistance - distance)
                    let halfOverlap = (toOther * overlap) / 2f
                    circle.Position <- circle.Position - halfOverlap
                    circle.Velocity <- (-toOther) * (circle.Velocity.Length())
                    other.Position  <- other.Position  + halfOverlap
                    other.Velocity  <- toOther * (other.Velocity.Length())

    let resolveScreenBoundaryCollision circle =
        let w = float32 screenWidth
        let h = float32 screenHeight
        let pos = circle.Position
        // Collision with Bottom Axis
        if pos.Y > (h - circle.Radius) then
            circle.Position <- vec2 pos.X (h - circle.Radius)
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 0f -1f)
        // Collision with left Axis
        if pos.X < circle.Radius then
            circle.Position <- vec2 circle.Radius pos.Y
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 1f 0f)
        // Collision with Right Axis
        if pos.X > (w - circle.Radius) then
            circle.Position <- vec2 (w - circle.Radius) pos.Y
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 -1f 0f)
        // Collision with Up Axis
        if pos.Y < circle.Radius then
            circle.Position <- vec2 pos.X circle.Radius
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 0f 1f)

// Circles to draw
let mutable circles =
    List.init circleAmount (fun i -> Circle.randomCircle defaultSpeed)

// Game Loop
Raylib.InitWindow(screenWidth, screenHeight, "Verlet Integration")
// Raylib.SetTargetFPS(60)
while not <| CBool.op_Implicit (Raylib.WindowShouldClose()) do
    let dt = Raylib.GetFrameTime()

    Raylib.BeginDrawing ()
    Raylib.ClearBackground(Color.Black)
    Raylib.DrawFPS(0,0)
    for circle in circles do
        Circle.update circle dt
        Circle.resolveCollision circle circles
        Circle.resolveScreenBoundaryCollision circle
        Circle.draw   circle

    if guiButton (rect 325f 10f 150f 30f) "New Circles" then
        circles <- List.init circleAmount (fun i -> Circle.randomCircle defaultSpeed)
    if guiButton (rect 100f 10f 200f 30f) (if showVelocity then "Hide Velocity" else "Show Velocity") then
        showVelocity <- not showVelocity
    Raylib.EndDrawing ()

Raylib.CloseWindow()
