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

    let mapResult f anim =
        match anim with
        | Running   x    -> Running  (f x)
        | Finished (x,t) -> Finished (f x, t)

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
    let fromLerp (duration:TimeSpan) f =
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

    let ap (fanim:Animation<'a -> 'b>) (anim:Animation<'a>) : Animation<'b> =
        Animation(fun () ->
            let fanim = run fanim
            let anim  = run anim
            Anim(fun dt ->
                match Anim.run dt fanim with
                | Running f      -> Running  (f (Anim.value dt anim))
                | Finished (f,t) -> Finished (f (Anim.value dt anim), t)
            )
        )

    let map2 f anim1 anim2 =
        ap (map f anim1) anim2

    let map3 f anim1 anim2 anim3 =
        ap (ap (map f anim1) anim2) anim3

    let map4 f anim1 anim2 anim3 anim4 =
        ap (ap (ap (map f anim1) anim2) anim3) anim4

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
                    | Running x      -> Running x
                    | Finished (x,t) ->
                        if finished then
                            Finished (x,t)
                        else
                            current <- ValueNone
                            Running x
            )
        )

    let bind f anim =
        flatten (map f anim)

    /// converts a sequence into an animation
    let ofSeq xs =
        let data = Array.ofSeq xs
        if Array.length data = 0 then failwith "Sequence cannot be empty."
        Animation(fun () ->
            let mutable idx = -1
            let last        = Array.length data - 2
            Anim(fun dt ->
                if idx < last then
                    idx <- idx + 1
                    Running  (Array.item  idx    data)
                else
                    Finished (Array.item (idx+1) data, dt)
            )
        )

    /// Combine two animations by running the first then the second animation
    let append anim1 anim2 =
        flatten (ofSeq [anim1; anim2])

    /// Repeats an animation a given time
    let repeat count anim =
        flatten (ofSeq (List.replicate count anim))

    /// A sequence is turned into an Animation where every element
    /// is repeated for `time` amount.
    let ofSeqDuration time xs =
        bind (duration time) (ofSeq xs)


module Lerp =
    let int (start:int) (stop:int) fraction =
        let start = float start
        let stop  = float stop
        int ((start * (1.0 - fraction)) + (stop * fraction))

    let float start stop fraction =
        ((start * (1.0 - fraction)) + (stop * fraction))

