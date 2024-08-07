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

[<NoComparison; NoEquality>]
type VerletStructure = {
    Points: Point list
    Sticks: Stick list
}

module Verlet =
    let point color radius position = {
        OldPosition  = position
        Position     = position
        Acceleration = Vector2.Zero
        Radius       = radius
        Color        = color
    }

    let stick first second = {
        First  = first
        Second = second
        Length = Vector2.Distance(first.Position, second.Position)
    }

    let velocity point =
        point.Position - point.OldPosition

    let newLength length stick = {
        stick with Length = length
    }

    let updatePoint point (dt:float32) =
        // let velocity    = point.Position - point.OldPosition
        // let newPosition = point.Position + velocity + (point.Acceleration * dt * dt)
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

    let addForce point force =
        point.Acceleration <- point.Acceleration + force

    let triangle color x y z =
        let a = point color 10f x
        let b = point color 10f y
        let c = point color 10f z
        {
            Points = [a; b; c]
            Sticks = [stick a b; stick b c; stick c a]
        }

    let quad color x y z w =
        let a = point color 10f x
        let b = point color 10f y
        let c = point color 10f z
        let d = point color 10f w
        {
            Points = [a;b;c;d]
            Sticks = [stick a b; stick b c; stick c d; stick d a; stick a c; stick b d]
        }

    let rectangle color x y w h =
        let tl = vec2 x y
        let tr = tl + (vec2 w 0f)
        let bl = tl + (vec2 0f h)
        let br = tl + (vec2 w h)
        quad color tl tr bl br

// The World to Draw
let mutable points = ResizeArray<_>()
let mutable sticks = ResizeArray<_>()

let addStructure { Points = ps; Sticks = ss } =
    points.AddRange ps
    sticks.AddRange ss

let resetWorld () =
    points.Clear()
    sticks.Clear()
    addStructure <| Verlet.triangle  Color.Yellow (vec2 400f 400f) (vec2 600f 200f) (vec2 500f 500f)
    addStructure <| Verlet.triangle  Color.Brown  (vec2 100f 100f) (vec2 100f 200f) (vec2 200f 300f)
    addStructure <| Verlet.quad      Color.Blue   (vec2 300f 300f) (vec2 400f 300f) (vec2 500f 500f) (vec2 200f 500f)
    addStructure <| Verlet.rectangle Color.DarkGray 600f 300f 100f 250f

    let r1 = Verlet.rectangle Color.DarkGreen  500f 500f 50f 50f
    let r2 = Verlet.rectangle Color.DarkPurple 100f 100f 100f 100f
    sticks.Add(Verlet.stick r1.Points.[0] r2.Points.[0] |> Verlet.newLength 50f)

    addStructure r1
    addStructure r2

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
        processDrag currentDrag points toRect mouse
    match currentDrag with
    | NoDrag                -> ()
    | InDrag (point,offset) ->
        point.Position <- mouse.Position

    for point in points do
        Verlet.addForce point gravity
        Verlet.applyScreen point
        Verlet.updatePoint point dt

    for i=1 to 2 do
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
