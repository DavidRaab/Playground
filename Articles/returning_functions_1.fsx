#!/usr/bin/env -S dotnet fsi

(*
Something we can do in functional programming and that probably
seems unfamiliar to beginners in functional programming is the idea that
we can return a function from a function call.

Here is an example how it can look:
*)

let greet () =
    let mutable counter = 0
    (fun name ->
        printfn "Hello %s, I have greeted %d times." name counter
        counter <- counter + 1
        )

let greeterA = greet ()
let greeterB = greet ()

greeterA "David"
greeterA "Alice"
greeterA "Vivi"

greeterB "Markus"

(* When we run this code, we geet the following lines printed:

    Hello David, I have greeted 0 times.
    Hello Alice, I have greeted 1 times.
    Hello Vivi, I have greeted 2 times.
    Hello Markus, I have greeted 0 times.

It works the following. When we call `greet ()` there is a `mutable counter`
initialized to zero. Then we create and return a function from that call. This
function expects an argument `name`.

So wheen we call `greeterA "David"` we pass `"David"` to that function. The
interesting part is the variable `counter`. Because the function we return from `greet`
reference this variable. It still stays in-memory.

This is called a `Closure`. Whenever we call `greet ()` we create a new variable,
and every function that is returned has its own copy of `counter`. This
is the reason why `greeterB` prints `... 0 times` and not `... 4 times`.

So, how can we achieve the same thing in an Object-Oriented language?

## Object-Oriented version
*)

type Greeter() =
    let mutable counter = 0
    member this.Greet name =
        printfn "Hello %s, I have greeted %d times." name counter
        counter <- counter + 1

let greeterOA = new Greeter()
let greeterOB = new Greeter()

greeterOA.Greet "David"
greeterOA.Greet "Vivi"
greeterOA.Greet "Alice"
greeterOB.Greet "Markus"

(* This code prints

    Hello David, I have greeted 0 times.
    Hello Vivi, I have greeted 1 times.
    Hello Alice, I have greeted 2 times.
    Hello Markus, I have greeted 0 times.

The above is a class in F# in C# it will look like:

    class Greeter {
        private int Counter = 0;

        public void Greet(string name) {
            Console.WriteLine("Hello {0}, I have greeted {1} times.", name, this.Counter);
            this.Counter = this.Counter + 1;
        }
    }

    var greeterA = new Greeter();
    var greeterB = new Greeter();

    greeterA.Greet("David");
    greeterA.Greet("Vivi");
    greeterA.Greet("Alice");
    greeterB.Greet("Markus");

So does it really seems unfamiliar to return a function? In fact whenever you return
an object you are probably not just returning an oject with a single method, maybe it
has a lot more. What is named a closure is exactly the same as returning an object
with some fields on it.

But there is one big difference to it. In a functional language you must not create
a type (in this case a class) before you are doing this. I can return a function
wherever I need it. So the overhead for some patterns is less than in C#. So you will
see some Patterns more often compared to a language where this kind of stuff is harder
todo.

Here are some improvements and suggestions, to myself and you whoever who reads it.

1. Is there a better example, and yet easy to understand, that makes sense?
2. Would you create a class in C# for this small piece of code?
3. Can we achieve the same without returning a function in F#? Maybe just some Data?
4. A class can easily return mutiple methods on an object. How would you do that in F#?
5. Is it a good idea to return multiple methods/functions?
6. Should we stick to a single function? Is this not the core of what **Single-responsibility principle** teaches us?
7. Would you create an IGreeter in the C# version? Do you think this is important?
8. Even if you create an IGreeter interface in C#. In F# you will not need that, and
   you get the same benefits as you would have in C#. Do you know why?
*)
