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
let circleAmount                 = 200
let circleMinSize, circleMaxSize = 10f, 20f
let gravity                      = vec2 0f 1000f
let mutable showVelocity         = false

// Class Alias
type rl        = Raylib
let isSame x y = LanguagePrimitives.PhysicalEquality x y

// Data-structures
[<NoComparison; NoEquality>]
type Circle = {
    mutable OldPosition: Vector2
    mutable Position:    Vector2
    Radius: float32
    Color:  Color
}

module Circle =
    let randomCircle pos = {
        OldPosition = pos
        Position    = pos
        Radius      = (randF circleMinSize circleMaxSize)
        Color       =
            match randI 0 5 with
            | 0 -> Color.DarkBlue
            | 1 -> Color.Orange
            | 2 -> Color.Purple
            | 3 -> Color.SkyBlue
            | 4 -> Color.DarkGreen
    }

    let inline update circle (dt:float32) =
        // Long way:
        let velocity = circle.Position - circle.OldPosition
        circle.OldPosition <- circle.Position
        circle.Position    <- circle.Position + velocity + (gravity * dt * dt)
        // let newPosition     = 2f * circle.Position - circle.OldPosition + (gravity * dt * dt)

    let draw circle =
        rl.DrawCircle (int circle.Position.X, int circle.Position.Y, float32 circle.Radius, circle.Color)
        if showVelocity then
            let velocity = circle.Position - circle.OldPosition
            rl.DrawLine (
                int circle.Position.X, int circle.Position.Y,
                int (circle.Position.X + velocity.X),
                int (circle.Position.Y + velocity.Y),
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
                if distance > neededDistance then
                    ()
                else
                    let toOther     = toOther / distance // normalize vector
                    let overlap     = neededDistance - distance
                    let halfOverlap = toOther * 0.5f * overlap
                    circle.Position <- circle.Position - halfOverlap
                    other.Position  <- other.Position  + halfOverlap

    let w, h = float32 screenWidth, float32 screenHeight
    let resolveScreenBoundaryCollision circle =
        // Collision with Bottom Axis
        if circle.Position.Y > (h - circle.Radius) then
            // circle.OldPosition <- circle.Position
            circle.Position    <- vec2 circle.Position.X (h - circle.Radius)
        // Collision with left Axis
        if circle.Position.X < circle.Radius then
            // circle.OldPosition <- circle.Position
            circle.Position    <- vec2 circle.Radius circle.Position.Y
        // Collision with Right Axis
        if circle.Position.X > (w - circle.Radius) then
            // circle.OldPosition <- circle.Position
            circle.Position    <- vec2 (w - circle.Radius) circle.Position.Y
        // Collision with Up Axis
        if circle.Position.Y < circle.Radius then
            // circle.OldPosition <- circle.Position
            circle.Position    <- vec2 circle.Position.X circle.Radius

// Circles to draw
let mutable circles =
    ResizeArray<_>(
        Seq.init circleAmount (fun i -> Circle.randomCircle (vec2 (randF 0f 1200f) (randF 0f 800f)))
    )

// Game Loop
rl.InitWindow(screenWidth, screenHeight, "Verlet Integration")
rl.SetTargetFPS(60)

while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse()

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Spawn circles
    if mouse.Left = Down then
        circles.Add(Circle.randomCircle mouse.Position)

    // Update Circles
    let subSteps = 4.0f
    let dt       = dt / subSteps
    for i=1 to int subSteps do
        for circle in circles do
            Circle.resolveScreenBoundaryCollision circle
            Circle.resolveCollision circle circles
            Circle.update circle dt

    for circle in circles do
        Circle.draw circle

    // Draw GUI
    rl.DrawFPS(0,0)
    rl.DrawText(System.String.Format("Circles: {0}", circles.Count), 1000, 10, 24, Color.Yellow)
    if guiButton (rect 325f 10f 150f 30f) "New Circles" then
        circles <- ResizeArray<_>( Seq.init circleAmount (fun i ->
            Circle.randomCircle (vec2 (randF 0f 1200f) (randF 0f 800f))
        ))
    if guiButton (rect 100f 10f 200f 30f) (if showVelocity then "Hide Velocity" else "Show Velocity") then
        showVelocity <- not showVelocity

    rl.EndDrawing ()

rl.CloseWindow()
