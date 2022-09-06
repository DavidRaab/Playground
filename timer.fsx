#!/usr/bin/env -S dotnet fsi

type TimeSpan = System.TimeSpan

type TimerResult<'a> =
    | Pending
    | Finished of 'a

type Timed<'a> = Timed of (TimeSpan -> TimerResult<'a>)
type Timer<'a> = Timer of (unit -> Timed<'a>)

module Timed =
    let get (Timer f)           = f ()
    let run deltaTime (Timed f) = f deltaTime

module Timer =
    let wrap x =
        Timer(fun () -> Timed(fun deltaTime -> Finished x))

    let map f timer =
        Timer(fun () ->
            let timed = Timed.get timer
            Timed(fun deltaTime ->
                match Timed.run deltaTime timed with
                | Pending    -> Pending
                | Finished x -> Finished (f x)
        ))

    let bind f timer =
        Timer(fun () ->
            let timedA         = Timed.get timer
            let mutable timedB = ValueNone
            Timed(fun deltaTime ->
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

    let delay delay f =
        Timer(fun () ->
            let mutable elapsedTime = TimeSpan.Zero
            let mutable finished    = Pending
            Timed(fun deltaTime ->
                match finished with
                | Pending ->
                    elapsedTime <- elapsedTime + deltaTime
                    if elapsedTime >= delay then
                        finished <- Finished (f ())
                    finished
                | Finished x -> Finished x
        ))

    let andThen timerA timerB =
        timerA |> bind (fun a ->
        timerB |> bind (fun b ->
            wrap (a,b)
        ))

let runUntilFinished timer =
    let mutable timeSoFar = TimeSpan.Zero
    let stepTime          = TimeSpan.FromSeconds 0.1
    let timed             = Timed.get timer

    printfn "Time: %O" timeSoFar
    let rec loop () =
        timeSoFar <- timeSoFar + stepTime
        printfn "Time: %O" timeSoFar
        match Timed.run stepTime timed with
        | Pending    -> loop ()
        | Finished x -> printfn "Finished: %A" x
    loop ()


let helloWorld () =
    printfn "Hello World!"

let helloTimer = Timer.delay (TimeSpan.FromSeconds 0.7) (fun () -> printfn "Hello"; 1)
let worldTimer = Timer.delay (TimeSpan.FromSeconds 1.3) (fun () -> printfn "World"; 2)

let helloWorldTimer = Timer.andThen helloTimer worldTimer


// runUntilFinished helloTimer
runUntilFinished helloWorldTimer
