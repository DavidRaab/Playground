#!/usr/bin/env -S dotnet fsi

#load "FSExtensions.fsx"
open FSExtensions
open System.Collections.Generic

let (|IsSmaller|_|) target x = if x < target then Some x else None

let rec findShare share target =
    // printfn "findShare %A %d" share target
    match share,target with
    |  _, 0               -> Some []
    |  _, IsSmaller 0 _   -> None
    | [], _               -> None
    | first::rest, target ->
        match findShare rest (target-first) with
        | None          -> findShare rest target
        | Some solution -> Some (first :: solution)

let rec findAllShares share target =
    match share,target with
    |  _, 0               -> Some [[]]
    |  _, IsSmaller 0 _   -> None
    | [], _               -> None
    | first::rest, target ->
        match findShare rest (target-first) with
        | None          -> findAllShares rest target
        | Some solution ->
            let solution = first :: solution
            match findAllShares rest target with
            | None           -> Some [solution]
            | Some solution2 -> Some (solution::solution2)

let removeMany es xs =
    xs |> List.filter (fun x -> not (List.contains x es))

let share  = [9;12;14;17;23;32;34;40;42;49]
let share2 = [8;11;13;16;22;31;33;39;41;48]
findShare     share 66
findAllShares share 66

let res1 = findShare share (List.sum share / 2)
let res2 = res1 |> Option.map (fun res1 -> removeMany res1 share)

printfn "Share1: %A" res1
printfn "Share2: %A" res2


(* Syntax for using map, compared to piping or operators *)

// List.map is the idea to turn a single argument of a function to a list.
// Here it is the first argument we want to turn to a list, while the second
// remains untouched. So we use a lambda. `xs` in the lambda represent
// one element of the list we pass to List.map, in this case `share` or `share2`
// that are lists again.
let f1 = List.map (fun xs -> findShare xs 66) [share;share2]

// As every function is curried in F#, you theoretically don't need the lambda.
// `List.map` just turns the first argument of any function to a list.
let f2 = List.map findShare [share;share2]

// But with
// a two argument function, it return a list of functions that still waits
// for the integer
// Here: list<(int -> option<list<int>>)>

// This is usally unwanted. In functional programming we use a `apply` function
// to pass in the next argument to this kind of type, but F# be default don't provide
// such a function. You can create such a function, or use the List Comprehesion
// feature in F#
let f3 = [for f in List.map findShare [share;share2] -> f 66]

// But I think its ugly, verbose and harder to understand compared to just using a lambda.
// But `List.map` always works great if you want to turn the last argument of a function
// to a list. Then you can just Partial Apply all the other arguments.

let f4 = List.map (findShare share)     [1..100]
let f5 = List.map (findAllShares share) [1..100]

// To work with functions with any arity (Function with any amount of arguments). Programmers
// sometimes use operators. In F# its common to use `<!>` for the `map` operation, and `<*>`
// for the `apply` operation. Here are two implementation of these operators.

let (<!>)       = List.map
let (<*>) fs xs = List.collect (fun f -> List.map f xs) fs

// Hint: In this case I implemented `<*>` a little bit different. It applies all values of a list
// to all functions in a list. So you get the Cartesian Product of all arguments to a function.

// Now you can write it like this

let o1 = findShare <!> [share;share2] <*> [20..30]

// With a three argument function like
let add3 x y z = x + y + z

// Now you could write
let a1 = add3 <!> [2;4] <*> [6;7] <*> [20;30]

// This sounds great, but in my opinion, i don't like it at all. The problem in F# is
// that we don't have Type Classes. So operators like `<!>` or `<*>` are always implementations
// for a specific type. But the idea of functions like `map`, `apply` are not bound to
// a specific type. We can have a `Option.map`, `Async.map` or any other kind of `map`
// function for a generic type. But we only can define `<!>` or `<*>` for one specific
// kind of data in F#.

// Another reason why i don't like it is; That we can solve this problem differently
// by just providing `map2`, `map3`, `map4`, ... and so on beforehand. With such functions
// in place. Lifting two or three arguments just looks like normal function application.

let o2 = List.lift2 findShare [share;share2] [20..30]
let o3 = List.lift3 add3      [2;4]          [6;7]    [20;30]

// Instead of adding `<!>` and `<*>` between the arguments, we just add `List.lift2`
// or `List.lift3` to the front of a function call. Everything else, remains the same.

// You can compare it to normal function application. Not much of a difference.

let add2 x y = x + y

let a2 =            add2 2     6
let a3 = List.lift2 add2 [2;4] [6;7]
let a4 =            add3 2     6     20
let a5 = List.lift3 add3 [2;4] [6;7] [20;30]

// And it also works great for any other types. Assume we want to use `add3`
// But we have lists, options or asyncs.

let m1 =              add3 1              2              3
let m2 = Option.map3  add3 (Some 1)       (Some 2)       (Some 3)
let m3 = List.lift3   add3 [1;2;3]        [4;5;6]        [7;8;9]
let m4 = Async.map3   add3 (Async.wrap 1) (Async.wrap 2) (Async.wrap 3)

// Another idea is to just use those function to create a new function with
// all its arguments lifted. So for example

let add3Option = Option.map3 add3
let add3List   = List.lift3  add3
let add3Async  = Async.map3  add3

// I don't think this is often used to create a new function and give it a new name
// But if needed, you can have it.

List.lift2 (sprintf "%d %d") [1..3] [5..7]
List.lift2 (sprintf "%d %d") [1..3] [5..10]
