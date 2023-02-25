type TimeSpan = System.TimeSpan

// [<Struct>]
type AnimationResult<'a> =
    | Running  of 'a
    | Finished of 'a * TimeSpan

module AnimationResult =
    let isRunning = function
        | Running   _    -> true
        | Finished (_,_) -> false

    let isFinished = function
        | Running   _    -> false
        | Finished (_,_) -> true

type Anim<'a>      = Anim      of (TimeSpan -> AnimationResult<'a>)
type Animation<'a> = Animation of (unit     -> Anim<'a>)

module Anim =
    let run deltaTime (Anim f) =
        f deltaTime

    let value deltaTime anim =
        match run deltaTime anim with
        | Running  v     -> v
        | Finished (v,t) -> v

module Animation =
    let wrap x =
        Animation(fun () -> Anim(fun deltaTime -> Finished(x,deltaTime)))

    /// Runs an animation by returning Anim<'a>
    let run (Animation f) =
        f ()

    /// turns a function accepting a TimeSpan into an animation
    let create f =
        Animation(fun () -> Anim f)

    /// Transforms a lerping function into an animation. It takes `duration` time
    /// to run from 0.0 to 1.0. This is passed to the lerping function that must
    /// return the actual value.
    let fromLerp f (duration:TimeSpan) =
        Animation(fun () ->
            let mutable soFar = TimeSpan.Zero
            Anim(fun dt ->
                soFar <- soFar + dt
                if   soFar < duration
                then Running  (f (soFar / duration))
                else Finished (f 1.0, duration - soFar)
            )
        )

    /// Turns a whole animation into a list by simulating it with `stepTime`
    let toList anim stepTime =
        let anim = run anim
        [
            let mutable running = true
            while running do
                match Anim.run stepTime anim with
                | Running x ->
                    yield x
                | Finished (x,_) ->
                    running <- false
                    yield x
        ]


module Lerp =
    let int (start:int) (stop:int) fraction =
        let start = float start
        let stop  = float stop
        int ((start * (1.0 - fraction)) + (stop * fraction))

    let float start stop fraction =
        ((start * (1.0 - fraction)) + (stop * fraction))

