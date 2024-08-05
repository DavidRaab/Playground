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
let mutable showVelocity         = false

[<Struct>]
type Vec2 = {
    X: float
    Y: float
}

module Vec2 =
    let create x y = { X = x; Y = y }

    let add a b = {
        X = a.X + b.X
        Y = a.Y + b.Y
    }

    let sub a b = {
        X = a.X - b.X
        Y = a.Y - b.Y
    }

    let multiply a scalar = {
        X = a.X * scalar
        Y = a.Y * scalar
    }

    let length a =
        sqrt(a.X * a.X + a.Y * a.Y)

    let divide a scalar = {
        X = a.X / scalar
        Y = a.Y / scalar
    }

    let normalize a =
        divide a (length a)

    let f32 a =
        Vector2(float32 a.X, float32 a.Y)

    let fromF32 (v:Vector2) =
        create (float v.X) (float v.Y)

type Vec2 with
    static member (-) (a:Vec2, b:Vec2) =
        Vec2.sub a b
    static member (+) (a:Vec2, b:Vec2) =
        Vec2.add a b
    static member (*) (a:Vec2, scalar:float) =
        Vec2.multiply a scalar
    static member (/) (a:Vec2, scalar:float) =
        Vec2.divide a scalar

// Class Alias
type rl        = Raylib
let isSame x y = LanguagePrimitives.PhysicalEquality x y
let gravity    = Vec2.create 0.0 1000.0

// Data-structures
[<NoComparison; NoEquality>]
type Circle = {
    mutable OldPosition: Vec2
    mutable Position:    Vec2
    Radius: float
    Color:  Color
}

module Circle =
    let randomCircle pos = {
        OldPosition = pos
        Position    = pos
        Radius      = float (randF circleMinSize circleMaxSize)
        Color       =
            match randI 0 5 with
            | 0 -> Color.DarkBlue
            | 1 -> Color.Orange
            | 2 -> Color.Purple
            | 3 -> Color.SkyBlue
            | 4 -> Color.DarkGreen
    }

    let inline update circle (dt:float) =
        // Long way:
        let velocity = Vec2.sub circle.Position circle.OldPosition
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
                let distance       = Vec2.length toOther
                let neededDistance = circle.Radius + other.Radius
                if distance > neededDistance then
                    ()
                else
                    let toOther     = toOther / distance // normalize vector
                    let overlap     = neededDistance - distance
                    let halfOverlap = toOther * 0.5 * overlap
                    circle.Position <- circle.Position - halfOverlap
                    other.Position  <- other.Position  + halfOverlap

    let w, h = float32 screenWidth, float32 screenHeight
    let resolveScreenBoundaryCollision circle =
        // Collision with Bottom Axis
        if circle.Position.Y > (float h - circle.Radius) then
            // circle.OldPosition <- circle.Position
            circle.Position    <- Vec2.create circle.Position.X (float h - circle.Radius)
        // Collision with left Axis
        if circle.Position.X < circle.Radius then
            // circle.OldPosition <- circle.Position
            circle.Position    <- Vec2.create circle.Radius circle.Position.Y
        // Collision with Right Axis
        if circle.Position.X > (float w - circle.Radius) then
            // circle.OldPosition <- circle.Position
            circle.Position    <- Vec2.create (float w - circle.Radius) circle.Position.Y
        // Collision with Up Axis
        if circle.Position.Y < circle.Radius then
            // circle.OldPosition <- circle.Position
            circle.Position    <- Vec2.create circle.Position.X circle.Radius

// Circles to draw
let mutable circles =
    ResizeArray<_>(
        Seq.init circleAmount (fun i -> Circle.randomCircle (Vec2.create (rand 0 1200) (rand 0 800)))
    )

// Game Loop
rl.InitWindow(screenWidth, screenHeight, "Verlet Integration")
rl.SetTargetFPS(60)

while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = float <| rl.GetFrameTime()
    let mouse = getMouse()

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Spawn circles
    if mouse.Left = Down then
        circles.Add(Circle.randomCircle (Vec2.fromF32 mouse.Position))

    // Update Circles
    let subSteps = 4.0
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
            Circle.randomCircle (Vec2.create (rand 0 1200) (rand 0 800))
        ))
    if guiButton (rect 100f 10f 200f 30f) (if showVelocity then "Hide Velocity" else "Show Velocity") then
        showVelocity <- not showVelocity

    rl.EndDrawing ()

rl.CloseWindow()
