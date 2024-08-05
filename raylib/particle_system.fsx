#!/usr/bin/env -S dotnet fsi
#r "nuget:Raylib-cs"
#load "Lib_RaylibHelper.fsx"
open Raylib_cs
open Lib_RaylibHelper
open System.Numerics

let screenWidth, screenHeight = 1200, 800

type Sprite = {
    Texture: Texture2D
    Source:  Rectangle
}

// A single particle is usually spawned by an emiter. It lifes for some
// time and during lifetime applies some movement, rotation to it.
type Particle = {
    mutable Sprite:      Sprite
    mutable Position:    Vector2
    mutable Rotation:    float32
    mutable ElapsedTime: float32
    mutable Velocity:    Vector2
    mutable Torque:      float32
    mutable LifeTime:    float32
}

// An Emiter spawn particles from its Position into a direction
type Emiter = {
    Position:  Vector2
    Direction: Vector2
    FOV:       float32 // Degree
}

module Particles =
    let maxParticles = 50_000
    let mutable activeParticles = 0
    let particles = Array.init maxParticles (fun i -> {
        Sprite      = Unchecked.defaultof<Sprite>
        Position    = vec2 0f 0f
        Rotation    = 0f
        ElapsedTime = 0f
        Velocity    = vec2 0f 0f
        Torque      = 0f
        LifeTime    = 1f
    })

    // Only iterates through the active particles
    let inline iter ([<InlineIfLambda>] f) =
        if activeParticles > 0 then
            for idx=0 to activeParticles-1 do
                f particles.[idx]

    /// Initialize a new particle. Every field should be explicitly set. No
    /// cleanup or reset is done.
    let initParticle f =
        if activeParticles < maxParticles-1 then
            f particles.[activeParticles]
            activeParticles <- activeParticles + 1

    let inline swap x y =
        let tmp = particles.[x]
        particles.[x] <- particles.[y]
        particles.[y] <- tmp

    /// Deactivates a particle. Usually called when its ElapsedTime reached its lifetime
    let deactivateParticle idx =
        swap idx (activeParticles-1)
        activeParticles <- activeParticles - 1

    let updateParticles (dt:float32) =
        if activeParticles > 0 then
            let mutable idx     = 0
            let mutable running = true
            while running do
                if idx >= activeParticles then
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

// Genereates a Texture Atlas in Memory. This emulates later behaviour and
// performance better than calling DrawCircle() and DrawRectangle()
let sprites =
    let atlas = rl.LoadRenderTexture(20, 10)
    rl.BeginTextureMode(atlas)
    rl.DrawRectangle(0, 0, 10, 10, Color.DarkBlue)
    rl.DrawCircle(17, 5, 5f, Color.Yellow)
    rl.EndTextureMode()
    [|
        { Texture = atlas.Texture; Source = rect  0f 0f 10f 10f }
        { Texture = atlas.Texture; Source = rect 10f 0f 10f 10f }
    |]

// rl.SetTargetFPS(60)
while not <| CBool.op_Implicit (rl.WindowShouldClose()) do
    let dt = rl.GetFrameTime()
    rl.DrawFPS(0,0)

    rl.BeginDrawing ()
    rl.ClearBackground(Color.Black)

    // Update particles
    Particles.updateParticles dt

    // Initialize x Particles each frame
    for i=0 to 200 do
        let sprite = if rng.NextSingle () < 0.5f then sprites.[0] else sprites.[1]
        Particles.initParticle (fun p ->
            p.Sprite      <- sprite
            p.Position    <- vec2 (float32 screenWidth / 2f) (float32 screenHeight / 2f)
            p.ElapsedTime <- 0f
            p.LifeTime    <- nextF 0.5f 3f
            p.Rotation    <- 0f
            p.Torque      <- nextF -45f 45f
            p.Velocity    <- (vec2 (nextF -1f 1f) (nextF -1f 1f)) * 200f
        )

    // Draw particles
    Particles.iter (fun p ->
        rl.DrawTexturePro(
            p.Sprite.Texture, p.Sprite.Source, (rect p.Position.X p.Position.Y 10f 10f), Vector2.Zero, p.Rotation, Color.White
        )
    )

    Raylib.DrawText(System.String.Format("Particles {0}", Particles.activeParticles+1), 1000, 10, 24, Color.Yellow)
    rl.EndDrawing ()

rl.CloseWindow()
