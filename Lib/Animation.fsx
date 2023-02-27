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
                    Anim.running  (Array.item  idx    data)
                else
                    Anim.finished (Array.item (idx+1) data) dt
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

    let traverse f anims =
        let folder a anims =
            map2 (fun x xs -> f x :: xs) a anims
        Seq.foldBack folder anims (wrap [])

    /// Converts a sequence of animations into an animation containing a list
    let sequence anims =
        traverse id anims

    /// Animation from `start` to `stop` in the given `duration`
    let rangeFloat start stop duration =
        fromLerp duration (fun fraction ->
            (start * (1.0 - fraction)) + (stop * fraction)
        )

    /// Animation from `start` to `stop` in the given `duration`
    let rangeInt (start:int) (stop:int) duration =
        fromLerp duration (fun fraction ->
            int ((float start * (1.0 - fraction)) + (float stop * fraction))
        )

