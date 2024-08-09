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

(*
Instead of Stick, BreakableStick and so on it is better to think of different
kind of constraints. Stick is really a Length constraint keeping an exact length.
there also could be a min,max constraint keeping it in a certain range. Angle
constraint that keeps something in an angle. Push away constrain that just
keeps a certain kind of distance and so on. For this I would probably rework
the data-structures.
*)
[<NoComparison; NoEquality>]
type Stick = {
    First:  Point
    Second: Point
    Length: float32
}

[<NoComparison; NoEquality>]
type BreakableStick = {
    Stick:  Stick
    Factor: float32
}

[<NoComparison; NoEquality>]
type VerletStructure = {
    Points: Point list
    Sticks: Stick list
}

[<NoComparison; NoEquality>]
type Pinned = {
    Point:          Point
    PinnedPosition: Vector2
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
            if useGravity then
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

    let pinFirst structure =
        match structure with
        | { Points = []          } -> None
        | { Points = first::rest } -> Some { Point = first; PinnedPosition = first.Position }

    let triangle color (x:Vector2) y z =
        let a,b,c = point color 10f x, point color 10f y, point color 10f z
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

    let ropePoints points =
        let rec loop points =
            match points with
            | []      -> { Points = [];      Sticks = [] }
            | [point] -> { Points = [point]; Sticks = [] }
            | current :: rest ->
                let rope = loop rest
                let next = List.head rope.Points
                {
                    Points = current :: rope.Points
                    Sticks = (stick current next) :: rope.Sticks
                }
        loop points

    let rope color radius steps (start:Vector2) stop =
        let point  = point color radius
        let moveby = Vector2.Divide(stop - start, (float32 steps + 1f))
        ropePoints [
            yield  point start
            yield! List.init steps (fun i -> point (start + (moveby * (1f + float32 i))))
            yield  point stop
        ]

// The World to Draw
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
    bsticks.Clear()
    pinned.Clear()

    // Some basic shapes
    addStructure <| Verlet.triangle  Color.Yellow (vec2 400f 400f) (vec2 600f 200f) (vec2 500f 500f)
    addStructure <| Verlet.triangle  Color.Brown  (vec2 100f 100f) (vec2 100f 200f) (vec2 200f 300f)
    addStructure <| Verlet.quad      Color.Blue   (vec2 300f 300f) (vec2 400f 300f) (vec2 500f 500f) (vec2 200f 500f)
    addStructure <| Verlet.rectangle Color.DarkGray 600f 300f 100f 250f

    // Generates two boxes sticked together and pinned at a place
    let r1 = Verlet.rectangle Color.DarkGreen  600f 200f 100f 100f
    let r2 = Verlet.rectangle Color.DarkPurple 740f 340f  50f  50f
    bsticks.Add({
        Stick  = Verlet.stick r1.Points.[3] r2.Points.[0] |> Verlet.newLength 50f
        Factor = 3f
    })
    addStructure r1
    addStructure r2
    pinned.Add({
        Point          = r1.Points.[0]
        PinnedPosition = vec2 600f 200f
    })

    // Testing placeAt
    let tri = Verlet.triangle Color.Gold (vec2 0f 0f) (vec2 100f 0f) (vec2 50f 100f)
    Verlet.placeAt (vec2 800f 100f) tri
    addStructure tri

    // Generate ropes
    let ropes = [
        let cl = lerpColor Color.Green Color.Maroon
        Verlet.rope (cl   0f) 5f  0 (vec2 100f 100f) (vec2 300f 100f)
        Verlet.rope (cl 0.2f) 5f  2 (vec2 150f 100f) (vec2 350f 100f)
        Verlet.rope (cl 0.4f) 5f  4 (vec2 200f 100f) (vec2 400f 100f)
        Verlet.rope (cl 0.6f) 5f  6 (vec2 250f 100f) (vec2 450f 100f)
        Verlet.rope (cl 0.8f) 5f  8 (vec2 300f 100f) (vec2 500f 100f)
        Verlet.rope (cl   1f) 5f 10 (vec2 350f 100f) (vec2 550f 100f)
    ]

    // Pins the first point of every rope
    List.iter (Verlet.pinFirst >> Option.iter pinned.Add) ropes
    List.iter addStructure ropes

    // One free rope to play with
    addStructure (Verlet.rope Color.Lime 5f 16 (vec2 600f 100f) (vec2 1100f 100f))

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
        processDrag currentDrag points (fun p -> Circle (p.Position,p.Radius)) mouse
    match currentDrag with
    | NoDrag                   -> ()
    | StartDrag (point,offset)
    | InDrag    (point,offset) -> point.Position <- mouse.Position
    | EndDrag _                -> ()

    for point in points do
        if useGravity then
            Verlet.addForce gravity point
        Verlet.applyScreen point
        Verlet.updatePoint point dt

    for i=1 to 2 do
        if bsticks.Count > 0 then
            for idx=bsticks.Count-1 downto 0 do
                if Verlet.shouldBreak bsticks.[idx] then
                    bsticks.RemoveAt(idx)
                else
                    Verlet.updateStick bsticks.[idx].Stick

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
        let n =
            let len = (a.Position - b.Position).Length()  - bstick.Stick.Length
            let max = bstick.Stick.Length * bstick.Factor - bstick.Stick.Length
            len / max
        let c = lerpColor Color.DarkGray Color.Red n
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
