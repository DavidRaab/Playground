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

    /// Repeats `x` for `duration` time
    let duration duration x =
        Animation(fun () ->
            let mutable soFar = TimeSpan.Zero
            Anim(fun dt ->
                soFar <- soFar + dt
                if soFar < duration then
                    Running x
                else
                    let remaining = duration - soFar
                    Finished (x,remaining)
            )
        )

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
    let toList stepTime anim =
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

    /// returns a new animation whose values are applied to the given function.
    let map f anim =
        Animation(fun () ->
            let anim = run anim
            Anim(fun dt ->
                match Anim.run dt anim with
                | Running   x    -> Running  (f x)
                | Finished (x,t) -> Finished (f x, t)
            )
        )

    /// Flattens an Animation of Animations into a single animation
    let flatten anim =
        Animation(fun () ->
            let anim             = run anim
            let mutable finished = false
            let mutable current  = ValueNone
            Anim(fun dt ->
                match current with
                | ValueNone ->
                    match Anim.run dt anim with
                    | Running a ->
                        let a = run a
                        current <- ValueSome a
                        Anim.run dt a
                    | Finished (a,_) ->
                        let a = run a
                        current  <- ValueSome a
                        finished <- true
                        Anim.run dt a
                | ValueSome a ->
                    match Anim.run dt a with
                    | Running x      ->
                        Running x
                    | Finished (x,t) ->
                        if finished then
                            Finished (x,t)
                        else
                            current <- ValueNone
                            Running x
            )
        )

    /// Turns a list of animations into an animation
    let ofList xs =
        if List.isEmpty xs then failwith "list cannot be empty in List.ofList"
        Animation(fun () ->
            let mutable xs   = xs
            let mutable last = Unchecked.defaultof<_>
            Anim(fun dt ->
                match xs with
                | []         -> Finished (last, TimeSpan.Zero)
                | head::[]   ->
                    last <- head
                    xs   <- []
                    Finished (head, TimeSpan.Zero)
                | head::tail ->
                    last <- head
                    xs   <- tail
                    Running head
            )
        )

    /// Combine two animations by running the first then the second animation
    let append anim1 anim2 =
        Animation(fun () ->
            let anim1 = run anim1
            let anim2 = run anim2

            let mutable first = true
            let mutable left  = TimeSpan.Zero
            Anim(fun dt ->
                if first then
                    match Anim.run dt anim1 with
                    | Running x      -> Running x
                    | Finished (x,t) ->
                        left  <- t
                        first <- false
                        Running x
                else
                    if left = TimeSpan.Zero then
                        Anim.run dt anim2
                    else
                        let dt = left + dt
                        left <- TimeSpan.Zero
                        Anim.run dt anim2
            )
        )

    // let flatten anims =
    //     Animation(fun () ->
    //         let anim = run anims
    //         Anim(fun dt ->
    //         )
    //     )

    let bind f anim =
        flatten (map f anim)

    // let append anim1 anim2 =
    //     flatten (ofList [anim1; anim2])


module Lerp =
    let int (start:int) (stop:int) fraction =
        let start = float start
        let stop  = float stop
        int ((start * (1.0 - fraction)) + (stop * fraction))

    let float start stop fraction =
        ((start * (1.0 - fraction)) + (stop * fraction))

