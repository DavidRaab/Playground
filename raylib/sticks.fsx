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
let mutable useGravity           = false
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

type BreakableStick = {
    Stick:  Stick
    Factor: float32
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

    let breakableStick first second factor = {
        Stick  = stick first second
        Factor = factor
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

    let shouldBreak bstick =
        let axis = bstick.Stick.First.Position - bstick.Stick.Second.Position
        axis.Length() > (bstick.Stick.Length * bstick.Factor)

    let w, h = float32 screenWidth, float32 screenHeight
    let applyScreen point =
        // Collision with Bottom Axis
        if point.Position.Y > (h - point.Radius) then
            point.Position.Y <- h - point.Radius
            // Adds a friction to the ground by moving the position 5% against
            // the velocity (this is not frame-rate independent) but this kind
            // of simulation anyway should be run in a fixed update loop. So
            // i don't care for that demo here.
            let velocity = -(velocity point)
            point.Position <- point.Position + (velocity * 0.05f)
        // Collision with left Axis
        if point.Position.X < point.Radius then
            point.Position.X <- point.Radius
        // Collision with Right Axis
        if point.Position.X > (w - point.Radius) then
            point.Position.X <- w - point.Radius
        // Collision with Up Axis
        if point.Position.Y < point.Radius then
            point.Position.Y <- point.Radius

    let addForce force point =
        point.Acceleration <- point.Acceleration + force

    let placeAt (pos:Vector2) vstruct =
        for point in vstruct.Points do
            point.OldPosition <- point.OldPosition + pos
            point.Position    <- point.Position    + pos

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
type Pinned = { Point: Point; PinnedPosition: Vector2 }

let mutable points  = ResizeArray<_>()
let mutable sticks  = ResizeArray<_>()
let mutable bsticks = ResizeArray<_>()
let mutable pinned  = ResizeArray<_>()

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

    // Generates two boxes sticked together
    let r1 = Verlet.rectangle Color.DarkGreen  600f 200f 100f 100f
    let r2 = Verlet.rectangle Color.DarkPurple 740f 340f  50f  50f
    bsticks.Add({ Stick = Verlet.stick r1.Points.[3] r2.Points.[0] |> Verlet.newLength 50f; Factor = 3f })
    addStructure r1
    addStructure r2

    // Pin the two boxes to a position
    pinned.Add({
        Point          = r1.Points.[0]
        PinnedPosition = vec2 600f 200f
    })

    let tri = Verlet.triangle Color.Gold (vec2 0f 0f) (vec2 100f 0f) (vec2 50f 100f)
    Verlet.placeAt (vec2 800f 100f) tri

    addStructure tri

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
        let toRect (p:Point) =
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
        if useGravity then
            Verlet.addForce gravity point
        Verlet.applyScreen point
        Verlet.updatePoint point dt

    for i=1 to 2 do
        // Crap: But works, just for testing
        let toBeDeleted = ResizeArray<_>()
        for bstick in bsticks do
            if Verlet.shouldBreak bstick then
                toBeDeleted.Add(bstick)
            else
                Verlet.updateStick bstick.Stick
        for del in toBeDeleted do
            bsticks.Remove(del) |> ignore

        // update sticks
        for stick in sticks do
            Verlet.updateStick stick

    for pin in pinned do
        pin.Point.Position    <- pin.PinnedPosition
        pin.Point.OldPosition <- pin.PinnedPosition

    // Draw Point & Sticks
    for stick in sticks do
        let a,b = stick.First, stick.Second
        rl.DrawLine(int a.Position.X, int a.Position.Y, int b.Position.X, int b.Position.Y, Color.DarkGray)

    for bstick in bsticks do
        let a,b = bstick.Stick.First, bstick.Stick.Second
        let g = Color.DarkGray
        let r = Color.Red
        let n =
            let len = (a.Position - b.Position).Length()  - bstick.Stick.Length
            let max = bstick.Stick.Length * bstick.Factor - bstick.Stick.Length
            len / max
        let f32 = float32
        let cv = Vector3.Lerp(Vector3(f32 g.R, f32 g.G, f32 g.B), Vector3(f32 r.R, f32 r.G, f32 r.B), n)
        let c  = color (byte cv.X) (byte cv.Y) (byte cv.Z) 255uy
        rl.DrawLine(int a.Position.X, int a.Position.Y, int b.Position.X, int b.Position.Y, c)

    for point in points do
        rl.DrawCircle(int point.Position.X, int point.Position.Y, point.Radius, point.Color)

    // Draw GUI
    rl.DrawFPS(0,0)
    rl.DrawText(System.String.Format("Points: {0} Sticks: {1}", points.Count, sticks.Count), 800, 10, 24, Color.Yellow)
    if guiButton (rect 100f 10f 200f 30f) (if useGravity then "Disable Gravity" else "Enable Gravity") then
        useGravity <- not useGravity
    if guiButton (rect 325f 10f 150f 30f) "Reset World" then
        resetWorld ()

    rl.EndDrawing ()

rl.CloseWindow()
