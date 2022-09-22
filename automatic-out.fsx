#!/usr/bin/env -S dotnet fsi

let str1 = "foo123"
let str2 = "123"

let isInt (x:string) =
    let mutable out = Unchecked.defaultof<int>
    if System.Int32.TryParse(x, &out) then
        printfn "Is Integer: %d" out
    else
        printfn "Not an Integer: %s" x

isInt str1
isInt str2

let isInt' (x:string) =
    match System.Int32.TryParse(x) with
    | true,  out -> printfn "Is Integer: %d" out
    | false, out -> printfn "Not an Integer: %s" x

isInt' str1
isInt' str2