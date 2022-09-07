#!/usr/bin/env -S dotnet fsi

type TimeSpan = System.TimeSpan

type TimerResult<'a> =
    | Pending
    | Finished of 'a

module TimerResult =
    let isFinished tr =
        match tr with
        | Pending    -> false
        | Finished _ -> true

    let isPending tr =
        match tr with
        | Pending    -> true
        | Finished _ -> false

    let map f timed =
        match timed with
        | Pending    -> Pending
        | Finished x -> Finished (f x)

type Timed<'a> = Timed of (TimeSpan -> TimerResult<'a>)
type Timer<'a> = Timer of (unit -> Timed<'a>)

module Timed =
    let get (Timer f)           = f ()
    let run deltaTime (Timed f) = f deltaTime
    let create f =
        let mutable finished = Pending
        Timed(fun dt ->
            match finished with
            | Pending ->
                finished <- f dt
                finished
            | Finished x -> Finished x
        )

module Timer =
    // wrap any value into a timer
    let wrap x =
        Timer(fun () -> Timed(fun deltaTime -> Finished x))

    let map f timer =
        Timer(fun () ->
            let timed = Timed.get timer
            Timed.create (fun deltaTime ->
                match Timed.run deltaTime timed with
                | Pending    -> Pending
                | Finished x -> Finished (f x)
        ))

    let bind f timer =
        Timer(fun () ->
            let timedA         = Timed.get timer
            let mutable timedB = ValueNone
            Timed.create (fun deltaTime ->
                match Timed.run deltaTime timedA with
                | Pending    -> Pending
                | Finished x ->
                    match timedB with
                    | ValueNone ->
                        let timed = Timed.get (f x)
                        timedB <- ValueSome timed
                        Timed.run TimeSpan.Zero timed
                    | ValueSome timed ->
                        Timed.run deltaTime timed
        ))

    // A function is delayed for the given TimeSpan
    let delay delay f =
        Timer(fun () ->
            let mutable elapsedTime = TimeSpan.Zero
            Timed.create (fun deltaTime ->
                elapsedTime <- elapsedTime + deltaTime
                if elapsedTime >= delay then
                    Finished (f ())
                else
                    Pending
        ))

    // Same as delay, but already expects seconds (in float) instead if TimeSpan
    let seconds seconds f =
        delay (TimeSpan.FromSeconds seconds) f

    // Executes timerA and when it finishes then timerB -- this is sequential
    let andThen timerA timerB =
        timerA |> bind (fun a ->
        timerB |> bind (fun b ->
            wrap (a,b)
        ))

    let flatten timer =
        timer |> bind (fun x ->
            x |> bind (fun y ->
            wrap y
        ))

    // Apply, but it runs in Parallel
    // This is what i expect in this model
    let ap timerF timerA = Timer(fun () ->
        let tf = Timed.get timerF
        let ta = Timed.get timerA
        Timed.create (fun dt ->
            match Timed.run dt tf, Timed.run dt ta with
            | Finished f, Finished a -> Finished (f a)
            | _                      -> Pending
        )
    )

    // map2 -- it runs in parallel - so both timer are executed and as both are resolved the f function is run
    let map2 f x y =
        ap (map f x) y

    // map3 -- again in parallel - f function is run as soon all three timers are resolved
    //      -- so all three timers run in paralel. If you have a timer with "0.2", "0.5" and "1.0" seconds
    //         delay you get the result after 1.0 seconds NOT 1.7 seconds
    let map3 f x y z =
        ap (map2 f x y) z

    let map4 f x y z w =
        ap (map3 f x y z) w

    let map5 f x y z a b =
        ap (map4 f x y z a) b

    // It's like traverse
    // It turns a sequence of timers into a Timer that runs in parallel. Executing the f function before
    // the result of every timer is put into the result array.
    // It's like: Timer.Parallel timers |> Timer.map f
    let ParallelMap f timers = Timer(fun () ->
        let timeds = Array.ofSeq (Seq.map Timed.get timers)
        Timed.create (fun dt ->
            let res = Array.map (Timed.run dt) timeds
            if Array.forall TimerResult.isFinished res then
                Finished (Array.map (fun x ->
                    match x with
                    | Finished x -> f x
                    | Pending    -> failwith "Cannot happen"
                ) res)
            else Pending
        )
    )

    // Turns a sequence of timers in a new timer that runs every timer in Parallel. Returning
    // an array of all results.
    let Parallel timers =
        ParallelMap id timers

    // Like ParallelMap, but every timer runs only after the previous timer finished.
    let sequentialMap f timers = Timer(fun () ->
        let results        = ResizeArray<_>()
        let mutable timers = List.ofSeq (Seq.map Timed.get timers)
        Timed.create(fun dt ->
            match timers with
            | []            -> Finished (results.ToArray())
            | timer::rest ->
                match Timed.run dt timer with
                | Pending    -> Pending
                | Finished x ->
                    results.Add (f x)
                    timers <- rest
                    Pending
        )
    )

    let sequential timer =
        sequentialMap id timer


let runUntilFinished timer =
    let mutable timeSoFar = TimeSpan.Zero
    let stepTime          = TimeSpan.FromSeconds 0.05
    let timed             = Timed.get timer

    printfn "Starting..."
    printfn "  Time: %O" timeSoFar
    let rec loop () =
        timeSoFar <- timeSoFar + stepTime
        printfn "  Time: %O" timeSoFar
        match Timed.run stepTime timed with
        | Pending    -> loop ()
        | Finished x -> printfn "Finished: %A" x
    loop ()


let helloWorld () =
    printfn "Hello World!"

let helloTimer = Timer.seconds 0.7 (fun () -> printfn "Hello"; 1)
let worldTimer = Timer.seconds 1.3 (fun () -> printfn "World"; 2)

let helloWorldTimer = Timer.andThen helloTimer worldTimer

let helloToT =
    Timer.flatten (
    Timer.seconds 0.7 (fun () ->
        Timer.seconds 0.3 (fun () -> printfn "Hello")))

let numA   = Timer.seconds 0.3 (fun () -> 3)
let numB   = Timer.seconds 0.2 (fun () -> 2)
let numC   = Timer.seconds 0.7 (fun () -> 5)
let numD   = Timer.seconds 0.4 (fun () -> 5)
let numSum = Timer.map4 (fun x y z w -> x + y + z + w) numA numB numC numD
let numsP  = Timer.Parallel   [numA;numB;numC]
let numsS  = Timer.sequential [numA;numB;numC]


// runUntilFinished helloTimer
// runUntilFinished helloWorldTimer
// runUntilFinished helloToT
// runUntilFinished (Timer.map (fun (x,y) -> x + y) helloWorldTimer)
// runUntilFinished numSum
runUntilFinished numsP
runUntilFinished numsS
