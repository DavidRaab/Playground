#!/usr/bin/env -S dotnet fsi

(* Testing the idea of having an array of mutable structs.
   It would be good to have a get that returns a pointer to
   the the array, but it always returns a copy.

   So far (as i see) you can only modify a mutable struct
   by fully referencing (setting) it in one go like down
*)

[<Struct>]
type Vector3 = {
    mutable X: float
    mutable Y: float
    mutable Z: float
}

let create x y z =
    { X = x; Y = y; Z = z }

let data = [|
    create 1.0 1.0 1.0
    create 2.0 2.0 2.0
|]

// Doesn't work as vec is a copy
let mutable vec = data.[0]
vec.X <- 2.0
printfn "%A" data

// Work as intended
data.[0].X <- 2.0
printfn "%A" data
