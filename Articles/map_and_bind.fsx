#!/usr/bin/env -S dotnet fsi

(* Difference between map and bind

To explain this, i am using the List type. First two different functions.
One that transforms an element, another one that returns a list.
*)

let square  x = x * x
let repeat3 x = List.replicate 3 x

printfn "%A" (square  4) // 16
printfn "%A" (repeat3 4) // [4;4;4]

(*
The idea of `map` is always to operate on the *inside* of a generic type. So if you have a
`list<int>` you provide a function that operates on an `int` whatever it returns is wraped
again in a `list`.

So if you have a function `int -> string` and use `List.map` on a `list<int>` you get a `List<string>`
back. `map` let's you write a function on the inside of the type without carring for the
outer type. For different `map` function you just replace the `list` type.

So the `Async.map` (not defined by default in F#) function does the same with an `Async<'a>`.
It let's you pass a function that just sees the `'a` and does the async handling for you.
You can return something new and you get a back a new Async Computation.

This benefit of `map` is that you can re-use all your functions without carrying for a list,
option, async, ...

    List.map   square someList   // list<int>
    Async.map  square someAsync  // Async<int>
    Option.map square maybeInt   // option<int>

So `map` provides a way to re-use your `square` function without that you need to care for the
outer type, and how it is handled.
*)

(* Problem:
But there is a common problem: What happens if you for example pass a `'a -> list<'a>` function
to `List.map`? You can do that and maybe that is exactly what you wanted, but sometimes this is not
the case. First let's see what happens by just thinking about the types.

So mapping a `'a -> list<'a>` function onto a `List<int>` will extract the `int` and passes this `int`
to the `'a -> list<'a>` function. This function now produce a new `list<int>` for every `int` in your
list. This list is then again wrapped in an overall list.

You can just think of `map` as replacing the types. So the return type of your function you pass to `f`
replaces the type in your original list. If you call `map` with the following types.

    ('a -> list<'a>) -> list<int> -> list<list<int>>

Then `int` will be replaced by `list<'a>`. And `'a` is `int` in this example. So you get `list<list<int>>`
*)

printfn "%A" (List.map square  [1;2;3]) // [1;4;9]
printfn "%A" (List.map repeat3 [1;2;3]) // [[1;1;1]; [2;2;2]; [3;3;3]]

(* Here is some other way to visualize what `map` does.

    List.map square  [1;2;3] = [ square 1;  square 2;  square 3]
    List.map repeat3 [1;2;3] = [repeat3 1; repeat3 2; repeat3 3]

Maybe that list of list is exactly what you wanted. But for other types this is often not
the case. And here comes `bind` into play. The idea of bind is to handle exactly such functions.
Instead of wrapping the return value again so you get a double wraped value, it wraps it only once.

    val listFunc  : 'a -> list<'b>
    val asyncFunc : 'a -> Async<'b>
    val optFunc   : 'a -> option<'b>

    List.map  listFunc aList // list<list<'b>>
    List.bind listFunc aList // List<'b>

    Async.map  asyncFunc anAsync // Async<Async<'b>
    Async.bind asyncFunc anAsync // Async<'b>

    Option.map  optFunc maybeValue // option<option<'b>
    Option.bind optFunc maybeValue // option<'b>

Now you hopefully can better understand the purpose of `map` and `bind` and how to use them,
what `map` and `bind` means for different types. And when to use one of them. It all depends
on the function `f` you want to execute on your type and the return value.

Because the way how `bind` works, there is an interesting law that can be derived from. Usually
when you build a library around a type, you will also have a `flatten` function to flat such
types in a type. It has the signature:

    val flatten : list<list<'a>> -> list<'a>

In F# for `list` this is the `List.concat` function. So, you always can do

    List.map listFunc aList |> List.concat

and it must be the same as

    List.bind listFunc aList

On a `list` the `bind` function is named `List.collect` in F#. So here you can see it:
*)

printfn "bind:       %A" (List.collect repeat3 [1;2;3])                // [1; 1; 1; 2; 2; 2; 3; 3; 3]
printfn "map_concat: %A" (List.map     repeat3 [1;2;3] |> List.concat) // [1; 1; 1; 2; 2; 2; 3; 3; 3]

(*
This is an important aspect because if you implement `map`, `bind` yourself on your types, you can choose
how you implement it yourself. Sometimes it is easier to implement `map` and `flatten` instead of `bind`.

There is again another law. As you also can implement `map` with `bind` the results of this implementations
must always be the same. But if you want to implement `map` with `bind` we need another function.

I name this function a constructor that just turns a normal value into your type. From the *Monadic* perspective
this function is named `return`. Here I name it `wrap` because `return` is a reserved word in F#:
*)

let wrapList   x = [x]
let wrapAsync  x = async { return x }
let wrapOption x = Some x

(*
Such wrapper functions are very easy and not really much to talk about. You usually just call a constructor
function on some value. Once you have such a function you can just use `bind` and inside of it use `wrap`.
*)

let mapList   f xs  = xs    |> List.collect (fun x -> wrapList   (f x))
let mapAsync  f asy = async {  let! x = asy        in return     (f x)}
let mapOption f opt = opt   |> Option.bind  (fun x -> wrapOption (f x))

printfn "mapList:   %A" (mapList   square [1..4])         // [1; 4; 9; 16]
printfn "mapAsync:  %A" (mapAsync  square (wrapAsync 4))  // Async<int>  || It is a new async equal to (wrapAsync 16)
printfn "mapOption: %A" (mapOption square (wrapOption 4)) // Some 16

(*
The async function uses the `async` computation expression. Inside of it lines with `let!` are like calling
`bind` and `return` is just the `wrap`. As you can see in the Async Computation Expression this is where the `return`
keyword is used to wrap your type.

map can be implemented by:  `bind` and `wrap`
bind can be implemented by: `map`  and `flatten`
*)

let flattenAsync asy = async {
    let! x = asy
    let! y = x
    return y
}

let bindList   f xs  = List.concat    (List.map   (fun x -> f x) xs )
let bindAsync  f asy = flattenAsync   (mapAsync   (fun x -> f x) asy)
let bindOption f opt = Option.flatten (Option.map (fun x -> f x) opt)
