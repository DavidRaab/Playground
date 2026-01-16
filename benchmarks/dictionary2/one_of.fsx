#!/usr/bin/env -S dotnet fsi

let inline bench amount msg ([<InlineIfLambda>] f) =
    let sw             = System.Diagnostics.Stopwatch.StartNew ()
    let mutable result = Unchecked.defaultof<_>
    for i=1 to amount do
        result <- f ()
    sw.Stop ()
    let callsPerSecond = (float amount) / (float sw.ElapsedMilliseconds) * 1000.0
    printfn "%s %.2f/sec" msg callsPerSecond
    result, (callsPerSecond)


let words = [
    "yes";"yep";"hello";"world";"cool";"no";
    "what";"shakira";"maybe";"whatever"
]

let hash () =
    let mutable count = 0
    let valid         = dict ["yes", 1; "no",1; "maybe",1]
    for i=1 to 100_000 do
        for word in words do
            if valid.ContainsKey word then
                count <- count + 1

let array3 () =
    let mutable count = 0
    let valid         = [| "yes"; "no"; "maybe" |]
    for i=1 to 100_000 do
        for word in words do
            if Array.contains word valid then
                count <- count + 1

let array5 () =
    let mutable count = 0
    let valid         = [| "test"; "yes"; "no"; "cools"; "maybe"|]
    for i=1 to 100_000 do
        for word in words do
            if Array.contains word valid then
                count <- count + 1

let array7 () =
    let mutable count = 0
    let valid         = [| "test"; "whoop"; "yes"; "cools"; "n64"; "no"; "maybe" |]
    for i=1 to 100_000 do
        for word in words do
            if Array.contains word valid then
                count <- count + 1

let array10 () =
    let mutable count = 0
    let valid         = [|
        "test"; "whoop"; "yes"; "cools"; "n64";
        "no";     "ps1"; "maybe"; "rows"; "xxx" |]
    for i=1 to 100_000 do
        for word in words do
            if Array.contains word valid then
                count <- count + 1

bench 1000 "Hash:" hash
bench 1000 "Hash:" hash
bench 1000 "Array  3:" array3
bench 1000 "Array  5:" array5
bench 1000 "Array  7:" array7
bench 1000 "Array 10:" array10
