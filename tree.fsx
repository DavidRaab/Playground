#!/usr/bin/env -S dotnet fsi --optimize+

#load "Lib/Tree234.fs"
open Tree234

let timeIt f =
    let sw = System.Diagnostics.Stopwatch.StartNew()
    let x = f ()
    sw.Stop()
    printfn "Elapsed: %O" (sw.Elapsed)
    x

let rng = System.Random()
let shuffle array =
    if Array.isEmpty array then [||] else
        let array = Array.copy array
        let max = Array.length array
        for i=0 to max-1 do
            let ni = rng.Next max
            let tmp = array.[ni]
            array.[ni] <- array.[i]
            array.[i]  <- tmp
        array

let numbers = shuffle [|1..1_000_000|]
// printfn "Numbers: %A" numbers

printfn "Create TreeA ..."
let treeA = timeIt (fun () -> Array.fold (fun t x -> Tree.add  x t) Tree.empty numbers)
printfn "Create TreeB ..."
let treeB = timeIt (fun () -> Array.fold (fun t x -> Tree.add' x t) Tree.empty numbers)


let isSorted xs =
    match xs with
    | [] -> true
    | xs ->
        List.fold (fun (sorted,p) x ->
            (sorted && p < x, x)
        ) (true,List.head xs) (List.tail xs)
        |> fst

for tree in [treeA;treeB] do
    // for treeA in treeA do
    // printfn "%s" (Tree.show treeA)
    printfn "Tree List: %A" (Tree.toList tree)
    printfn "IsSorted %b" (isSorted (Tree.toList tree))
    printfn "DepthA %d" (Tree.depth tree)
    printfn ""


