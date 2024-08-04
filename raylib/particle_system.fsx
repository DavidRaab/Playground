#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib_RaylibHelper.fsx"
open Raylib_cs
open Lib_RaylibHelper
open System.Numerics

let screenWidth, screenHeight = 1200, 800

// Usually a Texture in not demo purpose
type Sprite =
    | Rect
    | Circle

// A single particle is usually spawned by an emiter. It lifes for some
// time and during lifetime applies some movement, rotation to it.
type Particle = {
    mutable Position:    Vector2
    mutable Rotation:    float32
    mutable ElapsedTime: float32
    mutable Sprite:      Sprite
    mutable Velocity:    Vector2
    mutable Torque:      float32
    mutable LifeTime:    float32
}

// An Emiter spawn particles from its Position into a direction
type Emiter = {
    Position:  Vector2
    Direction: Vector2
    FOV:       float32 // Degree
    // Particles: Particle array
}

module Particles =
    let maxParticles = 20_000
    let mutable activeParticles = 0
    let particles = Array.init maxParticles (fun i -> {
        Position    = vec2 0f 0f
        Rotation    = 0f
        ElapsedTime = 0f
        Sprite      = Rect
        Velocity    = vec2 0f 0f
        Torque      = 0f
        LifeTime    = 1f
    })

    // Only iterates through the active particles
    let inline iter ([<InlineIfLambda>] f) =
        if activeParticles > 0 then
            for idx=0 to activeParticles do
                f particles.[idx]

    /// Initialize a new particle. Every field should be explicitly set. No
    /// cleanup or reset is done.
    let initParticle f =
        if activeParticles < maxParticles-1 then
            activeParticles <- activeParticles + 1
            f particles.[activeParticles]

    let inline swap x y =
        let tmp = particles.[x]
        particles.[x] <- particles.[y]
        particles.[y] <- tmp

    /// Deactivates a particle. Usually called when its ElapsedTime reached its lifetime
    let deactivateParticle idx =
        swap idx activeParticles
        activeParticles <- activeParticles - 1

    let updateParticles (dt:float32) =
        if activeParticles > 0 then
            let mutable idx     = 0
            let mutable running = true
            while running do
                if idx >= activeParticles || idx >= maxParticles then
                    running <- false
                else
                    // Update particle
                    let p = particles.[idx]
                    p.ElapsedTime <- p.ElapsedTime + dt
                    p.Position    <- p.Position + (p.Velocity * dt)
                    p.Rotation    <- p.Rotation + (p.Torque * dt)

                    // when particle lifetime is reached, we swap with last element.
                    // But then we need to recheck current idx again.
                    if p.ElapsedTime >= p.LifeTime then
                        deactivateParticle idx
                    else
                        idx <- idx + 1




rl.InitWindow(screenWidth, screenHeight, "Hello, World!")
// rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt = rl.GetFrameTime()
    rl.DrawFPS(0,0)

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Initialize x Particles each frame
    for i=0 to 100 do
        let sprite   = if rng.NextSingle () < 0.5f then Rect else Circle
        Particles.initParticle (fun p ->
            p.Position    <- vec2 (float32 screenWidth / 2f) (float32 screenHeight / 2f)
            p.Sprite      <- sprite
            p.ElapsedTime <- 0f
            p.LifeTime    <- nextF 0.5f 2f
            p.Rotation    <- 0f
            p.Torque      <- nextF -45f 45f
            p.Velocity    <- (vec2 (nextF -1f 1f) (nextF -1f 1f)) * 200f
        )

    // Update particles
    Particles.updateParticles dt

    // Draw particles
    Particles.iter (fun p ->
        match p.Sprite with
        | Rect ->
            rl.DrawRectangle(
                int p.Position.X, int p.Position.Y, 5, 5, Color.DarkBlue
            )
            // rl.DrawRectanglePro(
            //     (rect p.Position.X p.Position.Y 10f 10f),
            //     (vec2 (p.Position.X + 5f) (p.Position.Y + 5f)),
            //     p.Rotation,
            //     Color.DarkBlue
            // )
        | Circle ->
            rl.DrawCircle(int p.Position.X, int p.Position.Y, 3f, Color.Yellow)
    )

    Raylib.DrawText(System.String.Format("Particles {0}", Particles.activeParticles), 1000, 10, 24, Color.Yellow)
    rl.EndDrawing ()

rl.CloseWindow()
