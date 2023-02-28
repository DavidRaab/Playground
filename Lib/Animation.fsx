type TimeSpan = System.TimeSpan

// [<Struct>]
type AnimationResult<'a> =
    | Running  of 'a
    | Finished of 'a * TimeSpan

type Anim<'a>      = Anim      of (TimeSpan -> AnimationResult<'a>)
type Animation<'a> = Animation of (unit     -> Anim<'a>)

module Anim =
    let run deltaTime (Anim f) =
        f deltaTime

    let value deltaTime anim =
        match run deltaTime anim with
        | Running  v     -> v
        | Finished (v,t) -> v

    let inline running x    = Running x
    let inline finished x t = Finished (x,t)

module Animation =
    let wrap x =
        Animation(fun () -> Anim(fun deltaTime -> Finished(x,deltaTime)))

    /// Repeats `x` for `duration` time
    let duration duration x =
        Animation(fun () ->
            let mutable soFar = TimeSpan.Zero
            Anim(fun dt ->
                soFar <- soFar + dt
                if   soFar < duration
                then Anim.running  x
                else Anim.finished x (soFar - duration)
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
                then Anim.running  (f (soFar / duration))
                else Anim.finished (f 1.0) (soFar - duration)
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
                | Running   x    -> Anim.running  (f x)
                | Finished (x,t) -> Anim.finished (f x) t
            )
        )

    let ap (fanim:Animation<'a -> 'b>) (anim:Animation<'a>) : Animation<'b> =
        Animation(fun () ->
            let fanim = run fanim
            let anim  = run anim
            Anim(fun dt ->
                match Anim.run dt fanim, Anim.run dt anim with
                | Running   f,    Running   x    -> Anim.running  (f x)
                | Running   f,    Finished (x,_) -> Anim.running  (f x)
                | Finished (f,_), Running   x    -> Anim.running  (f x)
                | Finished (f,t), Finished (x,u) -> Anim.finished (f x) (min t u)
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
                            Anim.finished x t
                        else
                            current <- ValueNone
                            Anim.running x
            )
        )

    let bind f anim =
        flatten (map f anim)

    /// Like `sequence` but additionally a mapping function is applied to the resulting list
    let traverse f anims =
        let folder a anims =
            map2 (fun x xs -> f x :: xs) a anims
        Seq.foldBack folder anims (wrap [])

    /// Converts a sequence of animations into an animation containing a list
    let sequence anims =
        traverse id anims

    /// Combine two animations by running the first then the second animation
    let append anim1 anim2 =
        Animation(fun () ->
            let anim1 = run anim1
            let anim2 = run anim2

            let mutable first    = true
            let mutable timeLeft = TimeSpan.Zero
            Anim(fun dt ->
                if first then
                    match Anim.run dt anim1 with
                    | Running x      -> Running x
                    | Finished (x,t) ->
                        timeLeft <- t
                        first    <- false
                        Running x
                else
                    let dt = timeLeft + dt
                    timeLeft <- TimeSpan.Zero
                    Anim.run dt anim2
            )
        )

    // concats mutliple animations into a single animation
    let concat anims =
        let anims = Array.ofSeq anims
        if Array.length anims = 0 then failwith "Sequence cannot be empty."
        Array.reduce append anims

    /// Repeats an animation a given time
    let repeat count anim =
        concat (List.replicate count anim)

    /// Applies `duration` to every element and turns it into a single animation
    let concatDuration time xs =
        concat (Seq.map (duration time) xs)

    /// zips two animations
    let zip anim1 anim2 =
        map2 (fun x y -> x,y) anim1 anim2

    /// zips three animations
    let zip3 anim1 anim2 anim3 =
        map3 (fun x y z -> x,y,z) anim1 anim2 anim3

    /// zips three animations
    let zip4 anim1 anim2 anim3 anim4 =
        map4 (fun x y z w -> x,y,z,w) anim1 anim2 anim3 anim4

    /// Animation that runs from start to stop with the given `perSecond`
    let speed start stop perSecond =
        if start <= stop then
            Animation(fun () ->
                let mutable current = start
                Anim(fun dt ->
                    current <- (current + (perSecond * dt.TotalSeconds))
                    if   current < stop
                    then Anim.running  current
                    else Anim.finished stop TimeSpan.Zero
                )
            )
        else
            Animation(fun () ->
                let mutable current = start
                Anim(fun dt ->
                    current <- (current - (perSecond * dt.TotalSeconds))
                    if   current > stop
                    then Anim.running  current
                    else Anim.finished stop TimeSpan.Zero
                )
            )

    /// Animation from `start` to `stop` in the given `duration`
    let range start stop duration =
        fromLerp duration (fun fraction ->
            (start * (1.0 - fraction)) + (stop * fraction)
        )

    /// Animation from `start` to `stop` in the given `duration`
    let rangeFloat32 (start:float32) (stop:float32) duration =
        fromLerp duration (fun fraction ->
            let fraction = float32 fraction
            float32 ((start * (1.0f - fraction)) + (stop * fraction))
        )

    /// Animation from `start` to `stop` in the given `duration`
    let rangeInt (start:int) (stop:int) duration =
        fromLerp duration (fun fraction ->
            round ((float start * (1.0 - fraction)) + (float stop * fraction))
        )

