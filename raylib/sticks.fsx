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
let gravity                      = vec2 0f 1000f
let mutable showVelocity         = false

// Data-structures
[<NoComparison; NoEquality>]
type Point = {
    mutable OldPosition:  Vector2
    mutable Position:     Vector2
    mutable Acceleration: Vector2
    Radius: float32
    Color:  Color
}

[<NoComparison; NoEquality>]
type Stick = {
    First:  Point
    Second: Point
    Length: float32
}

module Verlet =
    let point color radius position = {
        OldPosition  = position
        Position     = position
        Acceleration = Vector2.Zero
        Radius       = radius
        Color        = color
    }

    let stick first second length = {
        First  = first
        Second = second
        Length = length
    }

    let updatePoint point (dt:float32) =
        let newPosition     = 2f * point.Position - point.OldPosition + (point.Acceleration * dt * dt)
        point.OldPosition  <- point.Position
        point.Position     <- newPosition
        point.Acceleration <- Vector2.Zero

    let updateStick stick =
        let axis       = stick.First.Position - stick.Second.Position
        let distance   = axis.Length ()
        let n          = axis / distance
        let correction = stick.Length - distance

        stick.First.Position  <- stick.First.Position  + (n * correction * 0.5f)
        stick.Second.Position <- stick.Second.Position - (n * correction * 0.5f)

    let w, h = float32 screenWidth, float32 screenHeight
    let applyScreen point =
        // Collision with Bottom Axis
        if point.Position.Y > (h - point.Radius) then
            point.Position.Y <- h - point.Radius
        // Collision with left Axis
        if point.Position.X < point.Radius then
            point.Position.X <- point.Radius
        // Collision with Right Axis
        if point.Position.X > (w - point.Radius) then
            point.Position.X <- w - point.Radius
        // Collision with Up Axis
        if point.Position.Y < point.Radius then
            point.Position.Y <- point.Radius

    let applyGravity point =
        point.Acceleration <- point.Acceleration + gravity

    let triangle color x y z =
        let a = point color 10f x
        let b = point color 10f y
        let c = point color 10f z

        let dab = Vector2.Distance(x,y)
        let dbc = Vector2.Distance(y,z)
        let dca = Vector2.Distance(z,x)

        let points = [a; b; c]
        let sticks = [stick a b dab; stick b c dbc; stick c a dca]

        points,sticks

// The World to Draw
let mutable points = ResizeArray<_>()
let mutable sticks = ResizeArray<_>()

let addPS (p,s) =
    points.AddRange p
    sticks.AddRange s

let resetWorld () =
    points.Clear()
    sticks.Clear()
    addPS <| Verlet.triangle Color.Yellow (vec2 400f 400f) (vec2 600f 200f) (vec2 500f 500f)
    addPS <| Verlet.triangle Color.Brown  (vec2 100f 100f) (vec2 100f 200f) (vec2 200f 300f)

resetWorld()

let mutable currentDrag = NoDrag

// Game Loop
rl.SetConfigFlags(ConfigFlags.Msaa4xHint)
rl.InitWindow(screenWidth, screenHeight, "Verlet Integration")
rl.SetTargetFPS(60)

while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse()

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Handles Drag of Points
    currentDrag <-
        let toRect p =
            let x = p.Position.X - p.Radius
            let y = p.Position.Y - p.Radius
            let w = p.Radius * 2f
            let h = p.Radius * 2f
            rect x y w h
        processDrag currentDrag points toRect mouse (fun point offset ->
            point.Position <- mouse.Position
        )

    for point in points do
        Verlet.applyGravity point
        Verlet.applyScreen point
        Verlet.updatePoint point dt

    for stick in sticks do
        Verlet.updateStick stick

    for stick in sticks do
        let a,b = stick.First, stick.Second
        rl.DrawLine(int a.Position.X, int a.Position.Y, int b.Position.X, int b.Position.Y, Color.DarkGray)

    for point in points do
        rl.DrawCircle(int point.Position.X, int point.Position.Y, point.Radius, point.Color)

    // Draw GUI
    rl.DrawFPS(0,0)
    rl.DrawText(System.String.Format("Points: {0} Sticks: {1}", points.Count, sticks.Count), 800, 10, 24, Color.Yellow)
    if guiButton (rect 325f 10f 150f 30f) "Reset World" then
        resetWorld ()

    rl.EndDrawing ()

rl.CloseWindow()
