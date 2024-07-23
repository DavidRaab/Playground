#!/usr/bin/env -S dotnet fsi

type Vec2 = {
    X: int
    Y: int
}

let sw = System.Diagnostics.Stopwatch ()
sw.Start ()

let mutable i = 0
while i < 100000000 do
    let v = { X = 10; Y = 10 }
    i <- i + 1

System.GC.Collect ()

sw.Stop ()
printfn "%O" sw.Elapsed


