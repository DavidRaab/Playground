#!/usr/bin/env -S dotnet fsi

let str1 = "foo123"
let str2 = "123"

(* Usually for performance reason and lack of language features. (A language has no option or can return multiple arguments)
   A out is used. The idea is to provide a pointer as the last argument where the functions writes the result.
   The function itself usually returns a boolean indicating if it was sucessfull (in C usually an error-code where 0 indicates success)
*)
let isInt (x:string) =
    let mutable out = Unchecked.defaultof<int>
    if System.Int32.TryParse(x, &out) then
        printfn "Is Integer: %d" out
    else
        printfn "Not an Integer: %s" x

isInt str1
isInt str2

(* In F# this case is nicer as yout don't must initialize the out parameter like above.
   You can omit the out and the value is created for you. Instead the function returns
   a (struct) tuple in F# with both the boolean and the out parameter.
*)
let isInt' (x:string) =
    match System.Int32.TryParse(x) with
    | true,  out -> printfn "Is Integer: %d" out
    | false, out -> printfn "Not an Integer: %s" x

isInt' str1
isInt' str2