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
    let run   deltaTime (Anim f) = f deltaTime
    let value deltaTime anim =
        match run deltaTime anim with
        | Running  v     -> v
        | Finished (v,t) -> v

    // let create f =
    //     let mutable finished = ValueNone
    //     Anim(fun dt ->
    //         match finished with
    //         | ValueNone
    //         | ValueSome (Running (_,_)) ->
    //             finished <- ValueSome (f dt)
    //             finished
    //         | ValueSome (Finished (v,t)) ->
    //             finished
    //     )

module Animation =
    let wrap x =
        Animation(fun () -> Anim(fun deltaTime -> Finished(x,deltaTime)))

    let empty =
        wrap ()

    /// Runs an animation by returning Anim<'a>
    let run (Animation f) =
        f ()

    /// turns a function accepting a TimeSpan into an animation
    let create f =
        Animation(fun () -> Anim f)

    /// Transforms a lerping function into an animation
    let fromLerp f (duration:TimeSpan) =
        Animation(fun () ->
            let mutable soFar = TimeSpan.Zero
            Anim(fun dt ->
                soFar <- soFar + dt
                if soFar < duration then
                    let fraction = soFar / duration
                    Running  (f fraction)
                else
                    Finished (f 1.0, duration - soFar)
            )
        )
