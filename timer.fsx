#!/usr/bin/env -S dotnet fsi

type TimeSpan = System.TimeSpan

[<Struct>]
type TimerResult<'a> =
    | Pending
    | Finished of 'a * TimeSpan

module TimerResult =
    let isFinished tr =
        match tr with
        | Pending    -> false
        | Finished _ -> true

    let isPending tr =
        match tr with
        | Pending    -> true
        | Finished _ -> false

[<Struct>]
type EveryState<'State,'a> =
    | State  of state: 'State
    | Finish of finish:'a

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
            | Finished (x,t) -> Finished (x,t)
        )

module Timer =
    /// wrap any value into a timer
    let wrap x =
        Timer(fun () -> Timed(fun deltaTime -> Finished (x,deltaTime)))

    /// A timer that imidiatelly finishes and returns unit
    let empty =
        wrap ()

    /// Turns a function into a timer that is delayed for the given TimeSpan
    let create delay f =
        Timer(fun () ->
            let mutable elapsedTime = TimeSpan.Zero
            Timed.create (fun deltaTime ->
                elapsedTime <- elapsedTime + deltaTime
                if elapsedTime >= delay then
                    Finished (f (), elapsedTime - delay)
                else
                    Pending
        ))

    /// The same as `create` but expects the seconds instead of a `TimeSpan`
    let seconds seconds f =
        create (TimeSpan.FromSeconds seconds) f

    /// A function is executed for the given duration. A state is passed
    /// through every invocation the function is runned. When finished, returns
    /// the final state.
    let duration duration (state:'State) f =
        Timer(fun () ->
            let mutable elapsedTime = TimeSpan.Zero
            let mutable state       = state
            Timed.create (fun deltaTime ->
                elapsedTime <- elapsedTime + deltaTime
                if elapsedTime <= duration then
                    state <- f state deltaTime
                    Pending
                else
                    Finished (state, elapsedTime - duration)
            )
        )

    /// Calls a function every timeSpan
    let every timeSpan (state:'State) f =
        Timer(fun () ->
            let mutable elapsedTime = TimeSpan.Zero
            let mutable state       = state
            Timed.create (fun dt ->
                elapsedTime <- elapsedTime + dt
                if elapsedTime >= timeSpan then
                    elapsedTime <- elapsedTime - timeSpan
                    match f state timeSpan with
                    | State s  -> state <- s; Pending
                    | Finish x -> Finished (x, elapsedTime)
                else
                    Pending
            )
        )

    /// A function is executed `amount` times every `timeSpan` then it finish with the final
    /// computed `state`.
    let repeat amount timeSpan (state:'State) f =
        let mutable state = state
        every timeSpan 0 (fun counter dt ->
            state <- f state counter dt
            let counter = counter + 1
            if   counter < amount
            then State counter
            else Finish state
        )

    /// maps a function or let's you work on the inside of a Timer.
    let map f timer =
        Timer(fun () ->
            let timed = Timed.get timer
            Timed.create (fun deltaTime ->
                match Timed.run deltaTime timed with
                | Pending        -> Pending
                | Finished (x,t) -> Finished (f x, t)
        ))

    /// binds a function or waits for Timer to finish.
    let bind f timer =
        Timer(fun () ->
            let timedA         = Timed.get timer
            let mutable timedB = Unchecked.defaultof<_>
            Timed.create (fun deltaTime ->
                match Timed.run deltaTime timedA with
                | Pending                    -> Pending
                | Finished (x,remainingTime) ->
                    if obj.ReferenceEquals(timedB,null) then
                        timedB <- Timed.get (f x)
                        Timed.run remainingTime timedB
                    else
                        Timed.run deltaTime timedB
        ))

    /// Delays a timer for the specified TimeSpan
    let delay timeSpan timer =
        create timeSpan id |> bind (fun () ->
            timer
        )

    /// Delays a timer for the specified seconds
    let delaySeconds time timer =
        seconds time id |> bind (fun () ->
            timer
        )

    /// Executes timerA and then timerB in sequence. Returns values of both
    /// timers as Tuple.
    let andThen timerA timerB =
        timerA |> bind (fun a ->
        timerB |> bind (fun b ->
            wrap (a,b)
        ))

    /// Flattens a Timer of Timer into a single Timer
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
            | Finished (f,tf), Finished (a,ta) -> Finished (f a, min tf ta)
            | _                                -> Pending
        )
    )

    let apB timerF timerA = Timer(fun () ->
        let tf = Timed.get timerF
        let ta = Timed.get timerA
        let mutable tb = Unchecked.defaultof<_>
        Timed.create (fun dt ->
            match Timed.run dt tf, Timed.run dt ta with
            | Finished (f,tf), Finished (a,ta) ->
                if obj.ReferenceEquals(tb,null) then
                    tb <- Timed.get (f a)
                    Timed.run (min tf ta) tb
                else
                    Timed.run dt tb
            | _ -> Pending
        )
    )

    /// Mapping for two argument function. Or executes a function when both timers finish.
    /// Both timers run in Parallel.
    let map2 f x y =
        ap (map f x) y

    let map3 f x y z =
        ap (map2 f x y) z

    let map4 f x y z w =
        ap (map3 f x y z) w

    let map5 f x y z a b =
        ap (map4 f x y z a) b

    /// bind for two timers. Both timers run in Parallel and user function returns a new timer.
    let bind2 f x y =
        apB (map f x) y

    let bind3 f x y z =
        apB (map2 f x y) z

    let bind4 f x y z w =
        apB (map3 f x y z) w

    let bind5 f x y z w v =
        apB (map4 f x y z w) v

    /// Runs a sequence of Timers in Parallel and executes the user defined function on every
    /// result. Returns a Timer containing an array of all results.
    let ParallelMap f timers =
        let folder t state =
            map2 (fun x xs -> f x :: xs) t state
        Seq.foldBack folder timers (wrap [])

    /// Switches the Layers. Turns a sequence of timers into a timer sequence.
    let Parallel timers =
        ParallelMap id timers

    /// Turns a sequence of timers into a timer that runs every timer sequential and returns
    /// the result of every timer as an array. Additionally maps the result.
    let sequentialMap f timers = Timer(fun () ->
        let results           = ResizeArray<_>()
        let mutable restTimer = TimeSpan.Zero
        let mutable timers    = List.ofSeq (Seq.map Timed.get timers)
        Timed.create(fun dt ->
            let rec loop dt =
                match timers with
                | []          -> Finished (results.ToArray(), restTimer)
                | timer::rest ->
                    let dt =
                        let newDt = dt + restTimer
                        restTimer <- TimeSpan.Zero
                        newDt
                    match Timed.run dt timer with
                    | Pending        -> Pending
                    | Finished (x,t) ->
                        results.Add (f x)
                        restTimer <- t
                        timers    <- rest
                        loop TimeSpan.Zero
            loop dt
        )
    )

    /// Switches layers and turns a sequence of timers into a timer array. But runs every
    /// timer one after another.
    let sequential timer =
        sequentialMap id timer

    /// A timer that just sleeps for a given time
    let sleep time =
        create time id

    /// A timer that sleeps for the given seconds
    let sleepSeconds sec =
        seconds sec id

    /// Overwrites the result of a Timer
    let set x timer =
        timer |> map (fun _ -> x)

type TimerCE() =
    member _.Bind(t,f)     = Timer.bind f t
    member _.Return(x)     = Timer.wrap x
    member _.ReturnFrom(x) = x
    member _.Zero()        = Timer.empty
    member _.MergeSources(x,y)        = Timer.map2 (fun x y       -> x,y) x y
    member _.MergeSources3(x,y,z)     = Timer.map3 (fun x y z     -> x,y,z) x y z
    member _.MergeSources4(x,y,z,w)   = Timer.map4 (fun x y z w   -> x,y,z,w) x y z w
    member _.MergeSources5(x,y,z,w,a) = Timer.map5 (fun x y z w a -> x,y,z,w,a) x y z w a

let timer = TimerCE()


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
    Timer.bind2 (fun x y ->
        show (Timer.delaySeconds 0.2 (Timer.wrap (x,y)))
    )
        (show (Timer.seconds 0.3 (fun () -> 3)))
        (show (Timer.seconds 0.2 (fun () -> 2)))
)

runUntilFinished 0.1 (
    Timer.bind5 (fun x y z w v ->
        show (Timer.delaySeconds 0.2 (Timer.wrap (x,y,z,w,v)))
    )
        (show (Timer.seconds 0.3 (fun () -> 5)))
        (show (Timer.seconds 0.2 (fun () -> 4)))
        (show (Timer.seconds 0.2 (fun () -> 3)))
        (show (Timer.seconds 0.2 (fun () -> 2)))
        (show (Timer.seconds 0.2 (fun () -> 1)))
)
