module Tree

type Tree<'a> =
    | Node of 'a * Tree<'a> * Tree<'a>
    | Leaf

let node x l r =
    Node(x,l,r)

let leaf = Leaf

let rec insert x tree =
    match tree with
    | Leaf         -> node x leaf leaf
    | Node(nx,l,r) ->
        if   x < nx 
        then node nx (insert x l) r
        else node nx l (insert x r)


let rec fold f (acc:'State) tree =
    match tree with
    | Leaf        -> acc
    | Node(x,l,r) -> 
        let right = fold f acc r
        let inner = f right x
        fold f inner l

let rec foldBack f tree (acc:'State) =
    match tree with
    | Leaf        -> acc
    | Node(x,l,r) -> f x (foldBack f l acc) (foldBack f r acc)

let filter pred tree =
    let folder acc x =
        if pred x then insert x acc else acc
    fold folder Leaf tree

let ofList xs = List.fold (fun acc x -> insert x acc) Leaf xs

let toList tree =
    fold (fun acc x -> x :: acc) [] tree

let inline sum tree =
    foldBack
        (fun x l r -> x + l + r)
        tree
        LanguagePrimitives.GenericZero

