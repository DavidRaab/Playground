open System.Threading

let mutable counter = 0
let mutable lockObj = ref 0

let increase amount = async {
    for i in 1 .. amount do
        let mutable repeat = true
        while repeat do
            let init  = counter
            let value = init + 1
            repeat <- init <> Interlocked.CompareExchange(&counter, value, init)
}

let increase' amount =
    for i in 1 .. amount do
        counter <- counter + 1

let increase'' amount =
    for i in 1 .. amount do
        lock lockObj (fun () ->
            counter <- counter + 1
        )


let measure fn =
    let sw = System.Diagnostics.Stopwatch.StartNew ()
    fn ()
    sw.Stop ()
    printfn "Time: %A" sw.Elapsed

let main () = async {
    printfn "Start 1"
    let! s1 = Async.StartChild (increase 100_000_000)
    printfn "Start 2"
    let! s2 = Async.StartChild (increase 100_000_000)

    do! s1
    do! s2

    printfn "Result: %d" counter
}

let main' () =
    let t1 = Thread(ThreadStart(fun () -> increase'' 100_000_000))
    let t2 = Thread(ThreadStart(fun () -> increase'' 100_000_000))
    t1.Start()
    t2.Start()
    t1.Join()
    t2.Join()

let main'' () = async {
    let! s1 = Async.StartChild (async { increase'' 100_000_000 })
    let! s2 = Async.StartChild (async { increase'' 100_000_000 })
    do! s1
    do! s2
    printfn "Counter: %d" counter
}


measure (fun () -> Async.RunSynchronously (main ()))
measure (fun () -> increase' 200_000_000)
measure main'
measure (fun () -> Async.RunSynchronously (main'' ()))
