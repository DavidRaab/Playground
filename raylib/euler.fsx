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
let circleAmount                 = 100
let gravity                      = vec2 0f 500f
let circleMinSize, circleMaxSize = 5f, 15f
let mutable showVelocity         = false

// Class Alias
type rl = Raylib

// Helper functions
let isSame x y    = LanguagePrimitives.PhysicalEquality x y
let rng           = System.Random ()
let nextI min max = rng.Next(min,max)
let nextF min max = min + (rng.NextSingle() * (max-min))

let vectorMax max (vector:Vector2) =
    let length = vector.Length ()
    if length > max
    then vector * (max / length)
    else vector

// Data-structures
type Circle = {
    mutable Position: Vector2
    mutable Velocity: Vector2
    Mass:   float32
    Radius: float32
    Color:  Color
}

module Circle =
    let randomCircle pos =
        let radius = nextF circleMinSize circleMaxSize
        {
            Position     = pos
            Velocity     = Vector2.Zero
            // Mass depends on object size, but could be different. For visualaization
            // it makes sense to think a bigger object has more Mass.
            Mass         = radius
            Radius       = radius
            Color        =
                match nextI 0 5 with
                | 0 -> Color.DarkBlue
                | 1 -> Color.Orange
                | 2 -> Color.Purple
                | 3 -> Color.SkyBlue
                | 4 -> Color.DarkGreen
        }

    let update circle (dt:float32) =
        let acceleration = gravity

        // Okay this is still Euler Method and not Verlet. Looking further into it.
        // But one important aspect is to update Velocity first before upting the
        // Position. Updating Velocity first has its own name "Semi-implicit Euler Method"
        circle.Velocity <- circle.Velocity + (acceleration * dt)    |> vectorMax 1000f
        circle.Position <- circle.Position + (circle.Velocity * dt)

        // Adding some friction to the velocity so it becomes less over time
        let friction    = 2f
        let negVec      = -circle.Velocity * friction * dt
        circle.Velocity <- circle.Velocity + negVec

    let draw circle =
        rl.DrawCircle (int circle.Position.X, int circle.Position.Y, circle.Radius, circle.Color)
        if showVelocity then
            rl.DrawLine (
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
                    let relSpeed    = circle.Velocity.Length () - other.Velocity.Length ()
                    let toOther     = toOther / distance // normalize vector
                    let overlap     = (neededDistance - distance)
                    let halfOverlap = (toOther * overlap) / 2f
                    let mass        = circle.Mass + other.Mass
                    circle.Position <- circle.Position - halfOverlap
                    circle.Velocity <- -toOther * ((2f * other.Mass / mass) * relSpeed)
                    other.Position  <- other.Position + halfOverlap
                    other.Velocity  <-  toOther * ((2f * circle.Mass / mass) * relSpeed)

    let resolveScreenBoundaryCollision circle =
        let w = float32 screenWidth
        let h = float32 screenHeight
        let pos = circle.Position
        // Collision with Bottom Axis
        if pos.Y > (h - circle.Radius) then
            circle.Position <- vec2 pos.X (h - circle.Radius)
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 0f -1f) * 0.2f
        // Collision with left Axis
        if pos.X < circle.Radius then
            circle.Position <- vec2 circle.Radius pos.Y
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 1f 0f) * 0.5f
        // Collision with Right Axis
        if pos.X > (w - circle.Radius) then
            circle.Position <- vec2 (w - circle.Radius) pos.Y
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 -1f 0f) * 0.5f
        // Collision with Up Axis
        if pos.Y < circle.Radius then
            circle.Position <- vec2 pos.X circle.Radius
            circle.Velocity <- Vector2.Reflect(circle.Velocity, vec2 0f 1f) * 0.5f



// Circles to draw
let mutable circles =
    ResizeArray<_>(
        Seq.init circleAmount (fun i ->
            Circle.randomCircle (vec2 (randf 0f 1200f) (randf 0f 800f))
        )
    )

// Game Loop
rl.InitWindow(screenWidth, screenHeight, "Verlet Integration")
rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt    = rl.GetFrameTime()
    let mouse = getMouse None

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)
    rl.DrawFPS(0,0)

    if mouse.Left = Down then
        circles.Add(Circle.randomCircle mouse.Position)

    // a simulation with 60fps means every movement of every circle is updated
    // every 1/60. A computer/game/program needs to calculate how much something
    // moved in this time-frame. Adding substeps of 2 for example means that
    // on each frame the update runs twice with half the frame-time. So
    // even when game runs at 60 fps, its simulated as running at 120 fps.
    // This way the simulation becomes better, collision detection works better
    // with fast moving objects and so on. But it also costs much performance.
    //
    // Instead of running everything at multiple-times of fps someone could
    // implemented continous collision detection for objects that need it
    // while everything else just runs at fps or better a fixed update time.
    let subSteps = 2f
    let dt = dt / subSteps
    for i=1 to int subSteps do
        for circle in circles do
            Circle.update circle dt
            Circle.resolveCollision circle circles
            Circle.resolveScreenBoundaryCollision circle

    for circle in circles do
        Circle.draw circle

    rl.DrawText(System.String.Format("Circles: {0}", circles.Count), 1000, 10, 24, Color.Yellow)

    if guiButton (rect 325f 10f 150f 30f) "New Circles" then
        circles <- ResizeArray<_>( Seq.init circleAmount (fun i ->
            Circle.randomCircle (vec2 (randf 0f 1200f) (randf 0f 800f))
        ))
    if guiButton (rect 100f 10f 200f 30f) (if showVelocity then "Hide Velocity" else "Show Velocity") then
        showVelocity <- not showVelocity

    rl.EndDrawing ()

rl.CloseWindow()
