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
let circleSizes                  = [| 7f; 13f; 21f |]
let gravity                      = vec2 0f 1000f
let mutable showVelocity         = false

// Data-structures
[<NoComparison; NoEquality>]
type Circle = {
    mutable OldPosition:  Vector2
    mutable Position:     Vector2
    mutable Acceleration: Vector2
    Radius: float32
    Color:  Color
}

module Circle =
    let randomCircle pos = {
        OldPosition  = pos
        Position     = pos
        Acceleration = Vector2.Zero
        Radius       = randomOf circleSizes
        Color =
            match randI 0 5 with
            | 0 -> Color.DarkBlue
            | 1 -> Color.Orange
            | 2 -> Color.Purple
            | 3 -> Color.SkyBlue
            | 4 -> Color.DarkGreen
    }

    let inline update circle (dt:float32) =
        // Long way:
        // let velocity = circle.Position - circle.OldPosition
        // circle.Position    <- circle.Position + velocity + (gravity * dt * dt)
        let newPosition = 2f * circle.Position - circle.OldPosition + (circle.Acceleration * dt * dt)
        circle.OldPosition  <- circle.Position
        circle.Position     <- newPosition

        // Took me some time why people apply Acceleration and then clear it.
        // Here is the idea: During game computation a game object can be exposed
        // to several forced (besides just gravity). All of that forces would
        // just add their force to the Acceleration. During update a fraction
        // of that force is applied. Then it must be cleared, because the object
        // can be moved outside a force field. Just adding forces to the
        // acceleration and clearing it every frame makes it easy to apply
        // all kind of forces.
        circle.Acceleration <- Vector2.Zero

    let draw circle =
        let inline ir x = int (round x)
        rl.DrawCircle (ir circle.Position.X, ir circle.Position.Y, float32 circle.Radius, circle.Color)
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
            if not (isSame circle other) then
                let toOther        = other.Position - circle.Position
                let distance       = toOther.Length ()
                let neededDistance = circle.Radius + other.Radius
                if distance < neededDistance then
                    let toOther     = toOther / distance // normalize vector
                    let overlap     = neededDistance - distance
                    // The simulation becomes better when the collision is not
                    // fully resolved in one step. The idea is to just move
                    // the object into the target position, but not set the
                    // target position. But this can require multiple subSteps
                    // in the gameLoop.
                    let correction  = 0.25f * overlap * toOther
                    circle.Position <- circle.Position - correction
                    other.Position  <- other.Position  + correction

    let w, h = float32 screenWidth, float32 screenHeight
    let resolveScreenBoundaryCollision circle =
        // Collision with Bottom Axis
        if circle.Position.Y > (h - circle.Radius) then
            circle.Position.Y <- h - circle.Radius
        // Collision with left Axis
        if circle.Position.X < circle.Radius then
            circle.Position.X <- circle.Radius
        // Collision with Right Axis
        if circle.Position.X > (w - circle.Radius) then
            circle.Position.X <- w - circle.Radius
        // Collision with Up Axis
        if circle.Position.Y < circle.Radius then
            circle.Position.Y <- circle.Radius

// Circles to draw.
let circles =
    ResizeArray<_>(
        Seq.init circleAmount (fun i -> Circle.randomCircle (vec2 (randF 0f 1200f) (randF 0f 800f)))
    )

// Game Loop
rl.SetConfigFlags(ConfigFlags.Msaa4xHint)
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
    let subSteps = 2.0f
    let dt       = dt / subSteps
    for i=1 to int subSteps do
        for circle in circles do
            // Add Gravity
            circle.Acceleration <- circle.Acceleration + gravity
            Circle.update circle dt
            Circle.resolveScreenBoundaryCollision circle
            Circle.resolveCollision circle circles

    // draw always should be in its own loop. even when every circle is iterated
    // once it can be processed multiple times. Because collision detection can
    // move the same circle multiple times, even inside a single loop iteration.
    // So it only makes sense to draw all circles once they all have been processed
    // completely. Think of it like in Conways Game of Life. The whole State of
    // all circles has to be advanced forward until the final position of a circle
    // is really known.
    for circle in circles do
        Circle.draw circle

    // Draw GUI
    rl.DrawFPS(0,0)
    rl.DrawText(System.String.Format("Circles: {0}", circles.Count), 1000, 10, 24, Color.Yellow)
    if guiButton (rect 325f 10f 150f 30f) "New Circles" then
        circles.Clear()
        circles.AddRange(
            Seq.init circleAmount (fun i ->
                Circle.randomCircle (vec2 (randF 0f 1200f) (randF 0f 800f))
        ))
    if guiButton (rect 100f 10f 200f 30f) (if showVelocity then "Hide Velocity" else "Show Velocity") then
        showVelocity <- not showVelocity

    rl.EndDrawing ()

rl.CloseWindow()
