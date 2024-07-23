#!/usr/bin/env -S dotnet fsi
#load "Benchmark.fs"
open Benchmark
type Dictionary<'a,'b> = System.Collections.Generic.Dictionary<'a,'b>

let amount = 50_000

let rng = System.Random()

// genereate dic
let dic = Dictionary()
for i=1 to amount do
    ignore <| dic.Add(i, rng.NextDouble ())

// benchmark for loop
let (sum1,time1) = countIt 1000 (fun () ->
    let mutable sum = 0
    for kv in dic do
        sum <- sum + kv.Key
    sum
)
printfn "Dic1 Sum %d Elapsed %O" sum1 time1

// benchmark keys
let(sum2,time2) = countIt 1000 (fun () ->
    let mutable sum = 0
    for key in dic.Keys do
        let value = dic.[key]
        sum <- sum + key
    sum
)
printfn "Dic2 Sum %d Elapsed %O" sum2 time2

// generate array
let array = ResizeArray()
for i=1 to amount do
    ignore <| array.Add(i, rng.NextDouble() )

// array performance
let (sum3,time3) = countIt 1000 (fun () ->
    let mutable sum = 0
    for (k,v) in array do
        sum <- sum + k
    sum
)
printfn "Arr1 Sum %d Elapsed %O" sum3 time3

