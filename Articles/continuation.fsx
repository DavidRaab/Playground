#!/usr/bin/env -S dotnet fsi

let square x = x * x

// This is how I would write it
let rec mapr f init =
    match init with
    | []      -> []
    | x::rest -> (f x) :: mapr f rest


// This is a small transformation to understand the next step better
// but is the same as 'mapr'
let mapr2 f init =
    let rec loop xs =
        match xs with
        | []      -> []
        | x::rest -> (f x) :: loop rest
    loop init

// Continuation-Passing-Style (CPS)
let mapc f init =
    let rec loop xs cont =
        match xs with
        | []      -> cont []
        | x::rest ->
            let newCont computedRest =
                cont ((f x) :: computedRest)
            loop rest newCont
    loop init id


(*
    The goal of the continutaion function here is to make the map function tail-recursive.

    Continuation function explained:

    1. We iterate through every element of a list.
    2. In the first iteration `x` represents 1. The goal of the map function
       is to execute `f` on it. So we can do `(f x)`. The problem here is, we need to prepend
       that element on the already computed rest. But we don't have that here! We are at the
       very beginning of the list.
    3. We solve that problem, by creating a new function. Here named `newCont`. We expect that
       the computed result can later be given, so we can delay the prepend.
    4. With `loop` now we pass that `newCont` to the next element. `loop` here is now tail-recursive.
    5. In the next iteration we get `newCont` from the previous iteration. That means, a function
       that would prepend (1 * 1) onto the rest. Now we are at `2`. And we have the same problem.
    6. It again the function would create a `newCont` with (2 * 2) prepended onto the rest.
    7. This continues until we reach the end of the list.
    8. At the end of the list. We now can start our execution of the continuation function. Remember
       that on each iteration, we only have access to the previous continuation function saved in `cont`
    9. We execute it by just calling `cont []`
    10. This would now pass an empty list to the `newCont` created from the last element. So
        we basically exeucte `(f x) :: computedRest` where `x` is `10` and `computedRest` is `[]`
    11. This generated the result `[100]` that is passed again, to the previous `cont` seen at
        this iteration.
    12. So `cont ((f x) :: computedRest)` is executed, or with variable interpolation: `cont ((double 9) :: [100])`
    13. This generates `[81;100]` and gives that as an argument to the previous `cont` again.
    14. This repeats, until we are at the first element again. This will finally call `cont [1;4;9;16;25;36;49;64;81;100]`
    15. Now the loop function returns. But we have to provide an initial `cont` function. This is the `id` function.
        That just returns its argument as-is. (Implementation: let id x = x)
    16. We now have re-written `map` in a tail-recursive way.
    17. BUT: It is SLOW!!! (and produces many garbage collected by the GC)
*)

(*
    Maybe this seems hard to understand, and it is! You have to wrap around everything until you get it. But once
    you understand it. Writting Continuation function can be very easily. Think about it, that way. All we wanted
    to do was to execute `(f x) :: computedRest`.

    + In the recursive style, we need to write `mapr f rest` to get the `computedRest`.
    + In the continuation style, we don't need to do that.

    The transformation in CPS (Continuation-Passing-Style) can be mechanic. You just create a function for the value
    that represents the computedResult, pass this function to `loop` and overall you pass the result to the `cont`.
    Maybe inlining everything can be easier.
*)

let mapc2 f init =
    let rec loop xs cont =
        match xs with
        | []      -> cont []
        | x::rest ->
            loop rest (fun computedRest ->
                cont ((f x) :: computedRest)
            )
    loop init id

(*
    Also remember that I inlined this in the recursive version. I also could have written:

        let computedRest = mapr f rest
        (f x) :: computedRest

    Now compare the `x::rest` pattern matching in both functions.

    `mapr`:
        x::rest ->
            let computedRest = mapr f rest
            (f x) :: computedRest

    `mapc`:
        x::rest ->
            loop rest (fun computedRest ->
                cont ((f x) :: computedRest)
            )
*)

(*  Final:
    In some sense the CP-Style reminds of the monadic `bind` function.
    `cont` is equal to the monadic `return`.
*)


// Tests -- all have to print the same
printfn "%A" (mapr  square [1..10])
printfn "%A" (mapc  square [1..10])
printfn "%A" (mapr2 square [1..10])
printfn "%A" (mapc2 square [1..10])
