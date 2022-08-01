#!/usr/bin/env -S dotnet fsi
// https://www.youtube.com/watch?v=t1e8gqXLbsU
// What is a Monad?
// Code example in F# instead of Haskell

// All Data-Types in this Example
type Maybe<'a> =
    | Nothing
    | Just of 'a

type Expr =
    | Val of int
    | Div of Expr * Expr

// Better Constructurs for Expr
let value x = Val x
let div x y = Div (x,y)

// 1. eval expression, but throws Exception
let rec eval expr =
    match expr with
    | Val x     -> x
    | Div (x,y) -> 
        eval x / eval y

eval (value 1)
eval (div (value 6) (value 2))
eval (div (value 6) (div (value 3) (value 1)))
eval (div (value 6) (value 0))

// So, we make a safeDiv, that returns a maybe instead
// of throwing an exception
let safeDiv x y =
    if y = 0 then
        Nothing 
    else
        Just (x / y)

// 2. eval that useses safeDiv
let rec eval' expr =
    match expr with
    | Val x     -> Just x
    | Div (x,y) ->
        match eval' x with
        | Nothing -> Nothing
        | Just x  -> 
            match eval' y with
            | Nothing -> Nothing
            | Just y  -> safeDiv x y

eval' (value 1)
eval' (div (value 6) (value 2))
eval' (div (value 6) (div (value 3) (value 1)))
eval' (div (value 6) (value 0))

// But the implementation is kind of ugly and harder to
// understand compared to the fiurst version, we now have
// a lot of error-checking, but the pattern of error-checking
// repeats, and we extract this as a function named ´bind´
let bind f m =
    match m with
    | Nothing -> Nothing
    | Just x  -> f x

// We create an additional bind Operator like Haskell has it defined
let (>>=) m f = bind f m

// 3. Re-implementation of eval with ´bind´ function
let rec eval'' expr =
    match expr with
    | Val x     -> Just x
    | Div (x,y) ->
        eval'' x |> bind (fun x ->
        eval'' y |> bind (fun y ->
           safeDiv x y
        ))

eval'' (value 1)
eval'' (div (value 6) (value 2))
eval'' (div (value 6) (div (value 3) (value 1)))
eval'' (div (value 6) (value 0))

// 4. Re-implemntation of eval using ´>>=´ instead of ´bind´
let rec eval''' expr =
    match expr with
    | Val x     -> Just x
    | Div (x,y) ->
        eval''' x >>= (fun x ->
        eval''' y >>= (fun y ->
            safeDiv x y
        ))

eval''' (value 1)
eval''' (div (value 6) (value 2))
eval''' (div (value 6) (div (value 3) (value 1)))
eval''' (div (value 6) (value 0))

// Do you think an operator like >>= makes readability somewhat
// better compared to `|> bind`?
// IMHO: No

// Haskell has `do` Notation. F# has Computation-Expressions
type MaybeBuilder() =
    member _.Bind(m,f)     = bind f m
    member _.Return(x)     = Just x
    member _.ReturnFrom(x) = x

let maybe = MaybeBuilder()

// 5. eval re-implemented again, using maybe Computation-Expression
let rec eva expr = maybe {
    match expr with
    | Val x     -> return x
    | Div (x,y) -> 
        let! x = eva x
        let! y = eva y
        return! safeDiv x y
}

eva (value 1)
eva (div (value 6) (value 2))
eva (div (value 6) (div (value 3) (value 1)))
eva (div (value 6) (value 0))