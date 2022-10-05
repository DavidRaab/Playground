#!/usr/bin/env -S dotnet fsi

#load "FSExtensions.fsx"
#load "data-structures/Heap.fs"
open FSExtensions
open Heap

// Generate a shuffled Array
let array = Array.shuffle [|1..100|]
printfn "Shuffled Array: %A\n" array

let ascending (x:int) y =
    x.CompareTo(y)

// Comparer return -1 0 1 (actually less then or greater zero)
// -1 if first element comes before second element
//  0 if both are equal
//  1 if second element comes before first element
let descending (x:int) y =
    if x > y then -1 elif x < y then 1 else 0

// Add shuffled Array to Heap
let heapASC = Heap<int>(ascending)
let heapDSC = Heap<int>(descending)

heapASC.AddMany array
heapDSC.AddMany array
printfn "HeapASC:\n%O" heapASC
printfn "HeapDSC:\n%O" heapDSC

// Remove Minimum until heap is empty and return it as an Array
// Important: heapASC|heapDSC is empty after this operation
let sortedASC = heapASC.ToArray()
let sortedDSC = heapDSC.ToArray()

// Print Sorted Array
printfn "SortedASC: %A\n" sortedASC
printfn "SortedDSC: %A\n" sortedDSC


// Some other Complex-data, a tuple, should be sorted by first key
let data = [
    5 ,"o"; 6 ," "; 8 ,"o"; 2 ,"e"
    9 ,"r"; 7 ,"W"; 3 ,"l"; 11,"d"
    12,"!"; 4 ,"l"; 1 ,"H"; 10,"l"
]

let dataASC = Heap(fun (x,_) (y,_) -> ascending x y)
dataASC.AddMany data
// Print GraphViz Dot File
printfn "%s" (dataASC.Dot (sprintf "%A"))
// Turn Heap into Array, use only the second element of tuple and concatenate result
let str = dataASC.ToArray() |> Array.map snd |> String.concat ""
printfn "Data: %s" str
