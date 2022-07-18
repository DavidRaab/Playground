
type Res<'a> =
    | Ok of 'a
    | Err

let map f res =
    match res with
    | Ok x -> Ok (f x)
    | Err  -> Err

let ap f res =
    match f,res with
    | Err,  Err  -> Err
    | Ok f, Err  -> Err
    | Err,  Ok _ -> Err
    | Ok f, Ok x -> Ok (f x)

let addTogether x y z = x + y + z


let (<!>) f x = map f x
let (<*>) f x = ap f x

let map2 f x y =
    f <!> x <*> y

let map3 f x y z =
    f <!> x <*> y <*> z

let map4 f x y z w =
    f <!> x <*> y <*> z <*> w

map3 addTogether (Ok 1) (Ok 2) (Ok 3)