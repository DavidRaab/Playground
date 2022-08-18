#!/usr/bin/env -S dotnet fsi

(* `map` operates on functions!

Some people, me including, had problems understanding the `map` function. Don't get me wrong. As
a Perl programmer `map` is built into the core language and I used and understanded it very well.

But I didn't understand `Async.map`, `Option.map`, `Foo.map` and so on. The problem with this was that
i misunderstood `map`. The conecption I learned (from others) was something like:

> map is a higher-order function that executes a function on every element on a list producing
> a new list this way.

While this is absolutely correct. It is a poor explanation to understand any other kind of `map`
function. This description only holds for the `List.map` function. But doesn't explain `map` for
any other type.

The first thing you have to realize is. That `map` is actually not about a `list`, `option` and so on.
While you must implement it for the different types. It really is about the function you pass as the first
argument. `map` really is about transforming a function. So let's get started.

## `map` as a two argument function

The type of `List.map` looks like this

    ('a -> 'b) -> list<'a> -> list<'b>

When we look at `map` as a two argument function it does exactly what I described first. We look at the function
this way.

    (1. Argument)   (2. Argument)   (Return Value)
    ('a -> 'b)   -> list<'a>     -> list<'b>

So it takes a function and a list. And it just executes the function for every element. But looking this way
on `Async.map` or `Option.map` it was hard for me to really understood what those functions does in those cases.
Even more, if i want to implement them myself. What is the correct implementation of it?

Maybe for `option<'a>` it is not so hard, as you can just think of option as either a one-element list or an empty
list. But for `Async.map` i just didn't get it for a long time. I got enlightenment when I throw away
what I already knew, and looked at `map` as a one-argument function.

## `map` as a one-argument function

One important thing in F# is, that every function is actually just a one argument function. There doesn't exists
functions with multiple arguments. Even the `map` function above is actually a one-argument function, that returns
a new function. You can look at it, this way.

    (1. Argument)    (Return Value)
    ('a -> 'b)    -> (list<'a> -> list<'b>)

The additional parenthesis around `list<'a> -> list<'b>` are not needed here as the arrow `->` is right-associative
by default. But here I added them for clarity. So what you really have is a `map` function that gets a `('a -> 'b)`
function as input and returns `list<'a> -> list<'b>` as an output.

What `map` really does is just transform the input function. It somehow wraps whatever you have as input and output,
and puts a `list` around it.

You have a function `async<url> -> async<option<string>>` if you pass that to `List.map` you get a
new function with the signature: `list<async<url>> -> list<async<option<string>>>`

## map transformer

Because of this you always can think of `map` as a function transformer. So when you have a function
that squares an int.
*)

// int -> int
let square x = x * x

(* you can turn this function into a new function that squares a list of ints *)

// list<int> -> list<int>
let squareList = List.map square

(*
## map on inner-type

You also can think of `map` as a function that lets you work on the inner-type. This is more
appropiate with the two-argument version. For example if you have an `list<int>` and you have
a function that does something with an `int` then you can use `List.map` for this.

This goes hand in hand with other types. You got an `Async<int>` and you have a function that
does something with an `int` but not `Async<int>` then you just use `Async.map`.

You have a `Foo<int>` and want to work with the `int` inside the `Foo` type? Just use `Foo.map`

## Conclusion

Think of `X.map` is either one of the above. You either **upgrade** a function and put wrappers around
the input and output types of whatever `X` is.

Or think of this function as something that works on the inside of the type. This means, something is
unwrapped and wrapped again. Like a list is unwrapped (unfoled) and then wraped (folded) again. But
you don't have to know how that is done (even for other types). Just think of it as don't caring
for the wrapped type at all. Just do something of what's inside of the type.

But defenitely never think of `map` as a function that iterates through every element of a list. This
is what you probably know because that's all what other languages usually provides. But you have to
abondon this idea because its wrong and just stands in your way to really understand what `map` is all
about.
*)