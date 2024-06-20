#!/usr/bin/env -S dotnet fsi

open System.Diagnostics

// This solution translates roughly to Perl solution #4
// F# creates an abstract base class "Shape" with four sub
// classes "Square", "Rectangle", ...
//
// But instead of making area a polymorphic function, F# programmers usually use
// a function to match the shape. F# internally also must create a switching
// branch for the pattern matching. The base shape that F# generates uses
// a enum/integer for detecting is subtype. So no type coercion with
// typical C# (as) is not needed.
//
// This is a typical F# solution that F# programmers might create.

type Shape =
    | Square    of side:float
    | Rectangle of width:float * height:float
    | Triangle  of basef:float * height:float
    | Circle    of radius:float

module Shape =
    // The "inline" is an optimization that inlines the code wherever it is used
    // instead of keeping the function call intact. It makes code faster, but
    // usually also makes the resulting binary bigger. It also allows for more
    // generic code, but this has no effect for that function here.
    let inline area shape =
        match shape with
        | Square     side          -> side   * side
        | Rectangle (width,height) -> width  * height
        | Triangle  (bbase,height) -> bbase  * height * 0.5
        | Circle     radius        -> radius * radius * 3.141592654

let rng = System.Random ()

let sw = Stopwatch.StartNew ()
let shapes = [
    for i=1 to 1000000 do
        yield Square   (rng.NextDouble() )
        yield Rectangle(rng.NextDouble(), rng.NextDouble())
        yield Triangle (rng.NextDouble(), rng.NextDouble())
        yield Circle   (rng.NextDouble() )
]
sw.Stop ()
printfn "Creation Time: %O" sw.Elapsed

sw.Restart ()
let sum =
    List.sumBy Shape.area shapes
sw.Stop

printfn "Sum Timing: %O" sw.Elapsed
