#load "Tree.fs"

let isEven x = x % 2 = 0

let tree  = Tree.ofList [10;33;60;-100;5;11;-6;34;50;-10]
let evens = Tree.filter isEven tree
let xs    = Tree.toList evens

Tree.sum tree
Tree.sum evens

Tree.sum (Tree.ofList [0.0;1.3;2.6;33.3456;-22.3])

let rec for' i loopTo body =
    if i < loopTo then 
        body i
        for' (i+1) loopTo body
    else
        ()

for' 0 10 (fun i ->
    printfn "%d" i
)

let rec for'' x doRun doIncrement body =
    if doRun x then
        body x
        for'' (doIncrement x) doRun doIncrement body
    else
        ()

for'' 10 (fun x -> x > 0) (fun x -> x-1) (fun x ->
    printfn "%d" x
)

let fromTo start xend body =
    for'' start (fun x -> x<xend) (fun x -> x+1) body

let mutable sum = 0
fromTo 0 10 (fun x ->
    sum <- sum + x
)

let rec for''' acc x doRun doIncrement body =
    if   doRun x
    then for''' (body acc x) (doIncrement x) doRun doIncrement body
    else acc

let add1 x        = x + 1
let sub1 x        = x - 1
let isSmaller y x = x < y
let isGreater y x = x > y

let sum' = for''' 0 10 (isSmaller 20) add1 (fun acc x ->
    printfn "%d" x
    acc + x
)

let list = for''' [] 10 (isSmaller 20) add1 (fun acc x ->
    x :: acc
)

let list' = for''' [] 20 (isGreater 9) sub1 (fun acc x ->
    x::acc
)

let rec while' acc pred body =
    if   pred acc
    then while' (body acc) pred body
    else acc

let acc = while' (0,0) (fun (x,_) -> x < 10) (fun (x,sum) ->
    (x+1, sum+x)
)

let isClose x target =
    abs (x - target) < 0.000001

let squareRoot target =
    let rec loop min max target =
        let  root   = (min + max) / 2.0
        let  square = root * root
        if   isClose square target
        then root
        else
            if   square > target
            then loop min root target
            else loop root max target
    loop 0.0 target target

squareRoot 2.0

// Class
type Person(firstName, lastName) =
    member this.FirstName = firstName
    member this.LastName  = lastName
    member this.PrintName() =
        printfn "Hello, %s %s" this.FirstName this.LastName
    member this.PrintFirst()=
        printfn "Hello %s" this.FirstName

let p1 = Person("David", "Raab")

p1.PrintName()
p1.PrintFirst()


// Clojure - 1 method
let printName firstName lastName =
    printfn "Hello, %s %s" firstName lastName

// Clojure - multiple methods
let person firstName lastName =
    {|
        PrintName  = (fun () -> printfn "Hello, %s %s" firstName lastName)
        PrintFirst = (fun () -> printfn "Hello %s" firstName)
    |}
    

let p2 = person "David" "Raab"
p2.PrintName()
p2.PrintFirst()

type Address = {
    Zip: string
}

// Records + functions
type PersonR = {
    FirstName: string
    LastName:  string
    Addresses: Address list
}

let printNameR x y z person =
    printfn "Hello, %s %s" person.FirstName person.LastName

let p3 = {FirstName="David"; LastName="Raab"; Addresses=[]}
printNameR p3
p3 |> printNameR




let f person y primaryAddress =
    let str     = person.FirstName + person.LastName
    let address = primaryAddress person
    printfn "Zip: %s" address.Zip




type Expression =
    | Literal  of float
    | Addition of Expression * Expression
    | Multi    of Expression * Expression

let num x   = Literal(x)
let add x y = Addition(x,y)
let mul x y = Multi(x,y)

let rec getValue expr =
    match expr with
    | Literal x      -> x
    | Addition (l,r) -> getValue l + getValue r
    | Multi    (l,r) -> getValue l * getValue r

let rec print expr =
    match expr with
    | Literal x      -> (string x)
    | Addition (l,r) -> sprintf "(%s + %s)" (print l) (print r)
    | Multi    (l,r) -> sprintf "(%s * %s)" (print l) (print r)
 
let rec toRacket expr =
    match expr with
    | Literal x      -> (string x)
    | Addition (l,r) -> sprintf "(+ %s %s)" (toRacket l) (toRacket r)
    | Multi    (l,r) -> sprintf "(* %s %s)" (toRacket l) (toRacket r)

let expr = 
    (mul
        (add
            (add (num 1.0) (num 2.0))
            (num 3.0))
        (mul (num 3.0) (num 2.0)))

expr |> print
expr |> getValue
expr |> toRacket


type ListIncreasing<'a> =
    | Empty
    | Value of 'a * ListIncreasing<'a>

let cons h t =
    match t with
    | Empty       -> Value(h,Empty)
    | Value (x,_) ->
        if h < x then
            failwith (sprintf "Not Allowed: %s must be greater %s" (string h) (string x))
        Value(h,t)

let x = (cons 10 (cons 3 (cons 2 Empty)))


[<Measure>] type frame
[<Measure>] type meter
[<Measure>] type second
[<Measure>] type mps = meter / second

let distance = 3000.0<meter>
let timeTook = 300.0<second>

let kmh = distance / timeTook * 3600.0

let xx = kmh + 3.3<mps>


let fps (totalFrames:float<frame>) (totalTime:float<second>) =
    totalFrames / totalTime

let toFps (totalTime:int<second>) =
    totalTime * 24<frame>

toFps (136 * 60<second>)


//3<meter> * 3<meter> * 3<meter> = 3<second>