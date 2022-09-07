#!/usr/bin/env -S dotnet fsi

type TimeSpan = System.TimeSpan

[<Struct>]
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

    let empty =
        wrap ()

    // A function is delayed for the given TimeSpan
    let create delay f =
        Timer(fun () ->
            let mutable elapsedTime = TimeSpan.Zero
            Timed.create (fun deltaTime ->
                elapsedTime <- elapsedTime + deltaTime
                if elapsedTime >= delay then
                    Finished (f ())
                else
                    Pending
        ))

    // A function is runned for a duration
    // It runs on every Timed.run until the duration is reached, then it returns Finished
    // Through the calls a state is passed and finally returned
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
                    Finished state
            )
        )

    // Same as create, but already expects seconds (in float) instead of TimeSpan
    let seconds seconds f =
        create (TimeSpan.FromSeconds seconds) f

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
            let mutable timedB = Unchecked.defaultof<_>
            Timed.create (fun deltaTime ->
                match Timed.run deltaTime timedA with
                | Pending    -> Pending
                | Finished x ->
                    if obj.ReferenceEquals(timedB,null) then
                        timedB <- Timed.get (f x)
                        Timed.run TimeSpan.Zero timedB
                    else
                        Timed.run deltaTime timedB
        ))

    let delay time timer =
        seconds time id |> bind (fun () ->
            timer
        )

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

    let bind2 f x y = Timer(fun () ->
        let ta = Timed.get x
        let tb = Timed.get y
        let mutable tc = Unchecked.defaultof<_>
        Timed.create (fun dt ->
            if obj.ReferenceEquals(tc, null) then
                match Timed.run dt ta, Timed.run dt tb with
                | Finished a, Finished b ->
                    tc <- Timed.get (f a b)
                    Timed.run TimeSpan.Zero tc
                | _ -> Pending
            else
                Timed.run dt tc
        )
    )

    // It's like traverse
    // It turns a sequence of timers into a Timer that runs in parallel. Executing the f function before
    // the result of every timer is put into the result array.
    // It's like: Timer.Parallel timers |> Timer.map f
    let ParallelMap f timers = Timer(fun () ->
        let timeds = Array.ofSeq (Seq.map Timed.get timers)
        let res    = Array.replicate (Array.length timeds) Pending
        Timed.create (fun dt ->
            for idx=0 to timeds.Length-1 do
                res.[idx] <- Timed.run dt timeds.[idx]

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
            | []          -> Finished (results.ToArray())
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

    // A Timer that just sleeps for some time
    let sleep sec =
        seconds sec id

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
    let mutable timeSoFar = TimeSpan.Zero
    let stepTime          = TimeSpan.FromSeconds stepTime
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


let helloTimer = Timer.seconds 0.7 (fun () -> printfn "Hello"; 1)
let worldTimer = Timer.seconds 1.3 (fun () -> printfn "World"; 2)
let helloWorldTimer = Timer.andThen helloTimer worldTimer

let helloToT =
    Timer.flatten (
        Timer.seconds 0.7 (fun () ->
            Timer.seconds 0.3 (fun () -> printfn "Hello")
        )
    )

let numA   = Timer.seconds 0.3 (fun () -> 3)
let numB   = Timer.seconds 0.2 (fun () -> 2)
let numC   = Timer.seconds 0.7 (fun () -> 5)
let numD   = Timer.seconds 0.4 (fun () -> 5)
let numSum = Timer.map4 (fun x y z w -> x + y + z + w) numA numB numC numD
let numsP  = Timer.Parallel   [numA;numB;numC]
let numsS  = Timer.sequential [numA;numB;numC]

let sumTimers x y = timer {
    let! x = x
    and! y = y
    return x + y
}

let helloWorldX = timer {
    do! Timer.sleep 0.5
    printfn "Hello"
    do! Timer.sleep 0.5
    printfn "World"
    return ()
}

// runUntilFinished helloTimer
runUntilFinished 0.2 helloWorldTimer
// runUntilFinished helloToT
// runUntilFinished (Timer.map (fun (x,y) -> x + y) helloWorldTimer)
// runUntilFinished numSum
runUntilFinished 0.2 (numsP |> Timer.map (fun xs -> printfn "Sum: %d" (Array.sum xs)))
runUntilFinished 0.2 (numsP |> Timer.set "Parallel")
runUntilFinished 0.2 (numsS |> Timer.set "Sequential")

runUntilFinished 0.2
    (sumTimers
        (Timer.seconds 0.3 (fun () -> 2))
        (Timer.seconds 0.3 (fun () -> 2)))

runUntilFinished 0.2 helloWorldX

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
    Timer.wrap "Delayed" |> Timer.delay 0.8
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
