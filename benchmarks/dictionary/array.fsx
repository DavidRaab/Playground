#!/usr/bin/env -S dotnet fsi

#load "Benchmark.fs"
open System.Numerics
open Benchmark

let rng = System.Random ()

// Here i want to benchmark the iterative performance of an array of
// classes (pointer-type) vs structs (value types)

type TransformP = {
    Position: Vector2
    Scale:    Vector2
    Rotation: float32
}

[<Struct>]
type TransformS = {
    Position: Vector2
    Scale:    Vector2
    Rotation: float32
}

// constructors
let createTransformP () : TransformP = {
    Position = Vector2(1f , rng.NextSingle())
    Rotation = rng.NextSingle ()
    Scale    = Vector2.One
}

let createTransformS () : TransformS = {
    Position = Vector2(1f , rng.NextSingle())
    Rotation = rng.NextSingle ()
    Scale    = Vector2.One
}


// Initialize two arrays
let arrayP = ResizeArray()
let arrayS = ResizeArray()

let amount = 50_000
for i=1 to amount do
    arrayP.Add(createTransformP ())
for i=1 to amount do
    arrayS.Add(createTransformS ())

// just iterate through both and count items
let (countP, timeP) = countIt 1000 (fun () ->
    let mutable count = 0f
    for t in arrayP do
        count <- count + t.Position.X
    count
)

let (countS, timeS) = countIt 1000 (fun () ->
    let mutable count = 0f
    for t in arrayS do
        count <- count + t.Position.X
    count
)

// print results
printfn "Pointer Count: %f Time: %O" countP timeP
printfn "Struct  Count: %f Time: %O" countS timeS

// Try to simulate memory fragmentation
// randomly delete entries from countP and create new ones to add. Currently
// all TransformP should be close in memory as they are created in a loop.
// but during typical runtime transforms will usually created and deleted.
//
// on the other hand, a struct is always packed together. No memory fragementation
// will happen. but for .net it means a struct always must be fully copied on
// index read or index set. So it has a general less overhead compared to
// arrayP, but performance stays the same even after some runing time of the
// application.

let lastIdx = arrayP.Count - 1
let swap x y (array:ResizeArray<_>) =
    let tmp = array.[x]
    array.[x] <- array.[y]
    array.[y] <- tmp

// randomly swap one item with last item. delete last item, and create new one.
for i=1 to 200_000 do
    let rngIdx = rng.Next(lastIdx) // random index but always one less than lastIdx
    swap rngIdx lastIdx arrayP
    arrayP.RemoveAt(lastIdx)
    arrayP.Add( createTransformP() )

// also do the same with arrayS, but actually it should have no impact
for i=1 to 200_000 do
    let rngIdx = rng.Next(lastIdx) // arrayP and arrayS is same size, otherwise would be bug
    swap rngIdx lastIdx arrayS
    arrayS.RemoveAt(lastIdx)
    arrayS.Add( createTransformS() )

// now repeat arrayP Benchmark
let (countP2, timeP2) = countIt 1000 (fun () ->
    let mutable count = 0f
    for t in arrayP do
        count <- count + t.Position.X
    count
)
let (countS2, timeS2) = countIt 1000 (fun () ->
    let mutable count = 0f
    for t in arrayS do
        count <- count + t.Position.X
    count
)

printfn "memdefragP Count: %f Time: %O" countP2 timeP2
printfn "memdefragS Count: %f Time: %O" countS2 timeS2

(*
Results on my machine

arrayP starts really fast, usually faster than arrayS. This makes sense
as at the beginning all TransformP should be closely allocated on memory.
also a reference should be a littlt bit smaller from a memory than the whole
thing as a structure. So iterating is somehow fast and faster than arrayS

but after memory defrag things changes. After swapping and inserting 200_000
elements arrayP becomes slower. It makes sense because its an array of pointer
where each structure is now scattered across memory. The swapping also ensures
that no element can be read linear from memory.

on the other hand arrayS stays the same. in fact it becomes even faster after
200_000 insertion and swapping. This is because an array of struct is always
a continous amount of memory. But it makes reading/writing a little bit slower
as now reading and writing always needs to copy a whole struct.

I guess the performance improvements happens because .Net can somehow optimize
arrays of structs or it just shows that processor caching works very well.

Btw. the performance of iterating an ResizeArray<_> is basically the same
as a struct array on Stack in plain C. But only with -O1. With -O2 C code becomes
double as fast. But i guess its more because it can somehow optimize reading
of fields, as I only read a single field of the structure.

But anyway performance in .Net with ResizeArray<_> seems very good to me.
*)