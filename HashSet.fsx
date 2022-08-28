#!/usr/bin/env -S dotnet fsi

type HashSet<'a> = System.Collections.Generic.HashSet<'a>

let a = HashSet [1..10]
let b = HashSet [5..10]
let c = HashSet [6;7;11]

// Set [6;7]
printfn "%A" (Set.intersectMany [(Set a); (Set b); (Set c)])

let intersect (x:HashSet<'a>) (y:HashSet<'a>) =
    let smaller, greater =
        if x.Count < y.Count then x,y else y,x

    let newHashSet = HashSet<'a>()
    for x in smaller do
        if greater.Contains x then
            ignore (newHashSet.Add x)

    newHashSet

// HashSet [5;6;7;8;9;10]
printfn "%A" (List.ofSeq (intersect a b))

// HashSet [6;7]
printfn "%A" (List.ofSeq (intersect (intersect a b) c))

let clone (set:HashSet<'a>) =
    let nh = HashSet()
    for x in set do
        nh.Add x |> ignore
    nh

let intersectMany (sets:seq<HashSet<'a>>) =
    if Seq.isEmpty sets then
        HashSet()
    else
        let smallest = clone (sets |> Seq.minBy (fun set -> set.Count))
        for set in sets do
            smallest.IntersectWith set
        smallest

printfn "%A" (List.ofSeq (intersectMany [a;b;c]))

printfn "a: %A" (List.ofSeq a)
printfn "b: %A" (List.ofSeq b)
printfn "c: %A" (List.ofSeq c)
