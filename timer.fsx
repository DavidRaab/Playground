#!/usr/bin/env -S dotnet fsi

#load "Lib/Timer.fs"
open Timer

let timer = TimerCE()
type TimeSpan = System.TimeSpan

let runUntilFinished stepTime timer =
    let stepTime          = TimeSpan.FromSeconds stepTime
    let timed             = Timed.get timer

    printfn "Starting..."
    let rec loop timeSoFar =
        printfn "  Time: %O" timeSoFar
        match Timed.run stepTime timed with
        | Pending        -> loop (timeSoFar + stepTime)
        | Finished (x,t) -> printfn "Finished: %A" x
    loop stepTime

let runTime stepTime timeFrame timer =
    let stepTime  = TimeSpan.FromSeconds stepTime
    let timeFrame = TimeSpan.FromSeconds timeFrame
    let timed     = Timed.get timer

    printfn "Starting..."
    let rec loop timeSoFar =
        printfn "  Time: %O" timeSoFar
        match Timed.run stepTime timed with
        | Pending        -> ()
        | Finished (x,t) -> printfn "Finished: %A" x
        if timeSoFar < timeFrame then
            loop (timeSoFar + stepTime)
    loop stepTime


// Test(s|ing)

let helloTimer = Timer.seconds 0.7 (fun () -> printfn "Hello"; 1)
let worldTimer = Timer.seconds 1.3 (fun () -> printfn "World"; 2)
let helloWorldTimer = Timer.andThen helloTimer worldTimer

let helloToT =
    Timer.flatten (
        Timer.seconds 0.7 (fun () ->
            Timer.seconds 0.3 (fun () -> printfn "Hello")
        )
    )

/// prints the value inside the timer
let show timer = Timer.map (fun x -> printfn "%A" x; x) timer

let numA   = show (Timer.delaySeconds 0.3 (Timer.wrap 3))
let numB   = show (Timer.seconds      0.2 (fun ()  -> 2))
let numC   = show (Timer.delaySeconds 0.7 (Timer.wrap 7))
let numD   = show (Timer.seconds      0.4 (fun ()  -> 5))

let sumTimers x y = timer {
    let! x = x
    and! y = y
    return x + y
}

// runUntilFinished 0.5 helloWorldTimer
// runUntilFinished 0.5 helloToT
// runUntilFinished 0.5 (Timer.map  (fun (x,y)   -> x + y) helloWorldTimer)
// runUntilFinished 0.2 (Timer.map4 (fun x y z w -> x + y + z + w) numA numB numC numD)
runUntilFinished 0.1 (Timer.Parallel   [numA;numB;numC] |> Timer.set "Parallel 0.3 0.2 0.7")
// runUntilFinished 0.5 (Timer.sequential [numA;numB;numC] |> Timer.set "Sequential 0.3 0.2 0.7")
runUntilFinished 0.1 (
    Timer.set "Empty Parallel" (
        Timer.sequential [
            show (Timer.Parallel [])
            show (Timer.delaySeconds 0.3 (Timer.wrap ["Foo"]))
        ]
    )
)

runUntilFinished 0.5 (Timer.sequential [] |> Timer.set "Empty Sequential")

runUntilFinished 0.2
    (sumTimers
        (Timer.seconds 0.3 (fun () -> 2))
        (Timer.seconds 0.3 (fun () -> 2)))

runUntilFinished 0.2 (timer {
    do! Timer.sleepSeconds 0.5
    printfn "Hello"
    do! Timer.sleepSeconds 0.5
    printfn "World"
    return "Hello World CE"
})

runUntilFinished 0.2 (timer {
    let! x = Timer.seconds 0.5 (fun () -> 5)
    and! y = Timer.seconds 0.5 (fun () -> 5)
    and! z = Timer.seconds 0.5 (fun () -> 5)
    return x + y + z
})

runUntilFinished 0.2 (
    Timer.bind2 (fun x y -> Timer.wrap (x + y)) numA numB
)

runUntilFinished 0.2 (
    Timer.wrap "Delayed" |> Timer.delaySeconds 0.8
)

// Simulating a 60fps gameTime
runUntilFinished (1.0 / 60.0) (
    Timer.duration (TimeSpan.FromSeconds 1.0) (0,0.0) (fun (counter,state) dt ->
        let newState = state + (dt.TotalSeconds * 3.0)
        printfn "%d -> Set X Position %f" counter newState
        (counter+1,newState)
    )
    |> Timer.map (fun (counter,state) ->
        printfn "Final: Set X Position: %f" 3.0
        3.0
    )
)

runUntilFinished 0.25 (
    Timer.Parallel [
        Timer.wrap 3
        Timer.wrap 2
        Timer.wrap 1
    ]
)

runUntilFinished 0.25 (
    Timer.repeat 3 (TimeSpan.FromSeconds 1.0) 100 (fun state counter dt ->
        let newState = state + (counter * 10)
        printfn "CountUp %d" newState
        newState
    )
    |> Timer.bind (fun counter ->
        Timer.every (TimeSpan.FromSeconds 1.0) counter (fun counter dt ->
            printfn "CountDown %d" counter
            if counter > 100
            then State (counter-10)
            else Finish counter
        )
    )
)

runUntilFinished 0.1 (
    Timer.bind2 (fun x y -> show (Timer.delaySeconds 0.2 (Timer.wrap (x,y))))
        (show (Timer.seconds 0.3 (fun () -> 3)))
        (show (Timer.seconds 0.2 (fun () -> 2)))
)

runUntilFinished 0.1 (
    let f x y z w v =
        show (Timer.delaySeconds 0.2 (Timer.wrap (x,y,z,w,v)))

    // Executes f after all timers are run, and passes the return values of
    // each timer to f. f then returns another new timer
    Timer.bind5 f
        (show (Timer.seconds 0.3 (fun () -> 5)))
        (show (Timer.seconds 0.2 (fun () -> 4)))
        (show (Timer.seconds 0.2 (fun () -> 3)))
        (show (Timer.seconds 0.2 (fun () -> 2)))
        (show (Timer.seconds 0.2 (fun () -> 1)))
)
