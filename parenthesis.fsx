#!/usr/bin/env -S dotnet fsi

(*
Parenthesis amoung non-LISP programmers are very often discouraged. The are seen as
not easy to read. The crazy thing. Other non-LISP Code is full of them. And if
you count `[]` and `{}` to parenthesis too, what I do. Then you even end in more
parentesis than normal LISP. The only thing is the position.

Let's look at the following F# code. It uses a `Vector2` and mainly classes
with Tuple syntax. This style is common in C, C++, C#, Java, Perl, Python, ...
you get it.
*)

state.Add (Rect.create (Vector2(float32 x * 11f, float32 y * 11f + yOffset)) Color.White)

(*
so, not every call is in this tuple syntax. But the call to create the `Vector2` is.
I am used to read this kind of inlining, and I think it becomes natural in a functional
language that returns values instead of modifing them.

But I still have problem reading those line, becaue the parenthesis, don't match up
nicely. Just let me extract the `Vector2` first. I write:

    Vector2(float32 x * 11f, float32 y * 11f + yOffset)

Why is this line so hard to read?

1. Because I must do a `float32` conversation here.
2. **and** the arguments are only separated by a comma `,`

And I think a comma is a very low visual clue that can be easily overseen. And
still after all of that I have to put parenthesis around `(Vector2 ...)` because
if I don't do that the language will see the code like this.

    Function    1.Argument 2.Argument     3.Argument
    Rect.create Vector2    (float32 ....) Color.White

This is why this line just seems completely fucked up in my opinion.

So I did create a F# like function where the function is curried by default
and just separated by empty spaces. Now the `Vector2` line can be written as following.

    Vector2 (float32 x * 11f) (float32 y * 11f + yOffSet)

This is still considered LISP-like. Even if the sorounding parenthesis are omitted. But
ML-Like languages (ML, F#, Haskell, ...) are allowed to omit these parenthesis if possible.

But still you see them arounf `(float32 x * 11f)`. And in my opinion it is much easier to see
that `Vector2` is called with 2 arguments. I can cleary and visualy separate those
arguments, and understand that those are computed by another expression.

When I insert the `Vector2` line again i now end up with.

    state.Add (Rect.create (Vector2.create (float32 x * 11f) (float32 y * 11f + yOffset)) Color.White)

And in my opinion its easier to read. Here is a more simplified example to see the difference.

    state.Add (Rect.create (Vector2(x,y)) Color.White)
    
vs.

    state.Add (Rect.create (Vector2.create x y) Color.White)

and still consider that `state.Add` and `Rect.create` are already ML/LISP-like, otherwise
the first line would be written.

    state.Add(Rect.create(Vector2(x, y), Color.White))
    
The amount of parenthesis are the same as a full LISP-like syntax. The only difference is
the position. It is `(func x y)` instead of `func(x,y)` but the first version is
much easier to read. Especially in code where every argument can become another more
complex expression.

I mean, i don't get it in mathematics I also would write:

    (3 * (2 + 5))

and every first-year student will understand it. In programming we put the function first.
But still

    (* 3 (+ 2 5))

would be understandable. How makes it even sense to put the function before the parenthesis?

    *(3, +(2,5))

Who ever came up with this kind of shit?
*)
