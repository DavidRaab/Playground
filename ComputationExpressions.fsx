type Maybe<'a> =
    | Just of 'a
    | Nothing

module Maybe =
    let map f maybe =
        match maybe with
        | Just x  -> Just (f x)
        | Nothing -> Nothing
    
    let bind f maybe =
        match maybe with
        | Just x  -> f x
        | Nothing -> Nothing

module Lst =
    let rec map f lst =
        match lst with
        | []     -> []
        | x::lst -> (f x) :: (map f lst)

    let rev xs =
        let rec loop xs ys =
            match xs with
            | []      -> ys
            | x::tail -> loop tail (x :: ys)
        loop xs []
    
    let rev' xs =
        let mutable result = []
        for x in xs do
            result <- x :: result
        result
    
    let map' f xs =
        let rec loop xs ys =
            match xs with
            | []      -> ys
            | x::tail -> loop tail ((f x) :: ys)
        loop (rev xs) []
    
    let concat lst1 lst2 =
        let rec loop xs ys =
            match xs with
            | []    -> ys
            | x::xs -> loop xs (x :: ys)
        loop (rev lst1) lst2
    
    let rec reduce f xs =
        match xs with
        | []         -> failwith "Ooops"
        | [x]        -> x
        | x::y::rest -> reduce f ((f x y) :: rest)
    
    let flatten xs =
        reduce concat xs
    
    let rec bind f lst =
        match lst with
        | []     -> []
        | x::lst -> (f x) @ (bind f lst)
    
    let bind' f lst =
        flatten (map' f lst)

Lst.concat [1;2;3] [4;5;6]

[3;3;3] |> Lst.map'  (fun x -> List.init x id)
[3;3;3] |> Lst.bind  (fun x -> List.init x id)
[3;3;3] |> Lst.bind' (fun x -> List.init x id)

type LstBuilder() =
    member this.Bind(xs, f)    = Lst.bind' f xs
    member this.Return(x)      = [x]
    member this.ReturnFrom(xs) = xs
    member this.Zero()         = []

type MaybeBuilder() =
    member this.Bind(m, f)     = Maybe.bind f m
    member this.Return(x)      = Just x
    member this.ReturnFrom(mx) = mx
    member this.Zero()         = Nothing

let lst   = new LstBuilder()
let maybe = new MaybeBuilder()

let ret x = [x]
[1;2;3] |> Lst.bind (fun x ->
    [4;5;6] |> Lst.bind (fun y ->
        [7;8;9] |> Lst.bind (fun z ->
            ret (x,y,z)
    )))

let product' = lst {
    let! x = [1;2;3]
    let! y = [4;5;6]
    let! z = [7;8;9]
    return (x,y,z)
}

let product'' = lst {
    let! x = [1;2;3]
    let! y = [4;5;6]
    let! z = [7;8;9]
    if z > 5 then
        return (x,y,z)
}

let product = seq {
    for x in seq {1;2;3} do
    for y in seq {4;5;6} do
    for z in seq {7;8;9} do
        if z > 5 then
            yield x,y,z
}

Seq.iter (printfn "%A") product


// Maybe
let smaller10 x = if x < 10 then Just x else Nothing
let greater0  x = if x > 0  then Just x else Nothing

let may' =
    smaller10 5 |> Maybe.bind (fun foo ->
    greater0  2 |> Maybe.bind (fun bar -> 
        Just (foo * bar)
    ))

let may'' = maybe {
    let! foo = smaller10 5
    let! bar = greater0 2
    return foo * bar
}

let from0to10 x = maybe {
    let! foo = smaller10 x
    let! bar = greater0 x
    return true
}

let square x = maybe {
    match! from0to10 x with
    | true  -> return  (sqrt (float x))
    | false -> return! Nothing
}

let maybeMap f m = maybe {
    let! value = m
    return f value
}

let maybeMap' f m =
    m |> Maybe.bind (fun value ->
        Just (f value)
    )

maybeMap  (fun x -> x * 2) (Just 10)
maybeMap' (fun x -> x * 2) (Just 10)

square 40
square 5

let from0to10' x =
    match smaller10 x with
    | Just foo ->
        match greater0 x with
        | Just bar -> Just true
        | Nothing  -> Nothing
    | Nothing -> Nothing

type Movie   = { Name: string }
let m1 = box {Name="Matrix"}
let m2 = box {Name="Foobar"}
let m3 = box 3
let movies   = [m1;m2;m3]

for x in movies do
    match x with
    | :? Movie -> printfn "%A" (unbox x)
    | :? int   -> printfn "%d" (unbox x)
    | _        -> printfn "Unknown Type"
