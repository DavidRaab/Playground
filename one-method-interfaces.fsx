#!/usr/bin/env -S dotnet fsi

(* One Method interfaces

I see them often, so called interfaced (or classes) that only have one method, and nothing
else. Here i explain to you why those are just functions. We start with something unreleated,
as it first seems.

Consider that we have a list, and we want to double each number in that list. If you are
already familiary with functional programming, you will probably use the `map` function.
But what would you code, if there were no such function?

I would probably write code similar to this:
*)

let numbers = [1..10]

let rec double xs =
    match xs with
    | []      -> []
    | x::rest -> x * 2 :: double rest

(*
This function is not tail-recursive, but we don't mind that here. This functions works fine.
We can pass it a list and it will return us a new list with each number doubled. This is fine,
but somewehere else in our code, we actually need the ability to square each number. So we write:
*)

let rec square xs =
    match xs with
    | []      -> []
    | x::rest -> x * x :: square rest

(*
Even at this point we would start to see the similarity between those two function. But again, let's
write a third function. How about adding +10 to each element of a list?
*)

let rec add10 xs =
    match xs with
    | []      -> []
    | x::rest -> x + 10 :: add10 rest

(*
Ok, we have three functions nearly similar. It seems that we have a pattern here. So, is there a way to
rewrite our functions in a way, that we only can pass the difference between those functions, while the
things that are equal remains the same?

First we need to ask what remains the same, and what changes? As we look at the function the same we always
do is return an empty list, when we have an empty list. And we also recurse on the `rest` of the function.
The recursive function returns a new list, and we always prepend an element to that list.

The difference is, that this element differs. In `double` we do `x * 2` and the result of this computation
is added to the list.

In `square` we do `x * x` and in `add10` we do `x + 10`.

The task now is to create an abstraction for this. People that only have a object-oriented background
sometimes struggle to come up with a solution. The Problem here is that we have three different cases,
but we actually **do** something different in every case. Or in other words, we have different
code in every case. It is not that we just have a value that changes.

For example if we want to provide a generic **add** function that can add any amount to every list, it
would be easy. We could just change `add10` to something like:
*)

let rec addX y xs =
    match xs with
    | []      -> []
    | x::rest -> x + y :: addX y rest

(*
In this case we had a generic way to add any amount to every list. But we are restricted to adding. But
in the world of functional programming, functions itself are also just values that we can pass to a
function. So with this idea we can write:
*)

let rec map f xs =
    match xs with
    | []      -> []
    | x::rest -> f x :: map f rest

(*
Now, with such a function we just can write:

    map (fun x -> x *  2) numbers
    map (fun x -> x *  x) numbers
    map (fun x -> x + 10) numbers

So, now let's see, how we can solve the same problem in C#. At this point i'm restricting it a little
bit. I want you to show the pure object-oriented way to solve that problem. Without all the functional
programming additions that C# already has. We solve that problem from the core. The same way i did
here in F#. I also could just have used `List.map` here.

First, we replicate the three starting problems in C#. We have three functions (static methods) that
we pass a List that returns another new list.
*)

(**csharp
    public class Fn {
        public static List<int> Double(List<int> numbers) {
            var result = new List<int>();
            foreach (var x in numbers ) {
                result.Add(x * 2);
            }
            return result;
        }

        public static List<int> Square(List<int> numbers) {
            var result = new List<int>();
            foreach (var x in numbers ) {
                result.Add(x * x);
            }
            return result;
        }

        public static List<int> Add10(List<int> numbers) {
            var result = new List<int>();
            foreach (var x in numbers ) {
                result.Add(x + 10);
            }
            return result;
        }

        public static string Show(List<int> numbers) {
            return "[" + String.Join(',', numbers) + "]";
        }
    }
*)

(*
We do that just to identify which code stays the same and where it differs. How can we now achieve a
similar abstraction? There comes different solutions to my mind. So here is one i wouldn't pick
but at least it is OO with some shiny inheritance, asbtract members.

First we create an abstract class

    abstract class Mappable<T> {
        public List<T> Execute(List<T> numbers) {
            var result = new List<T>();
            foreach (var x in numbers ) {
                result.Add(this.Map(x));
            }
            return result;
        }
        public abstract T Map(T x);
    }

The idea is that the code that is different becomes its own method that must be implemented
in each derived class. So now we have a `Mappable` class. That abstracts away the List iteration
and creation. Do you remember what we have written when we created the `map` function? We
could just write:

    let doubles = map (fun x -> x *  2) numbers

to get the Doubles of each list. Here is what you have to write in the OO-style.

    class Double : Mappable<int> {
        public override int Map(int x) {
            return x * 2;
        }
    }

    var doubles = new Double().Execute(numbers);

So we have to create a new class that now can just implements the missing part. This is for sure
absolutly bullshit. The abstraction is actually the same amount of code, as if you would directly
write the foreach-loop wherever you need it. That's one reason why you had seen so much for/foreach
iteration code in old OO code. Its not a feasable thing to abstract such code while it is easy and
common in Functional Programming.

Also think of another idea. OO enthusiast always tell the importance of inheritance and code-reuse,
but consider that we never used inheritance at all in the F# version and even solved the problem
better. But, we can improve the implementation of the C# version.

The article was about, how every one method interface is actually just a function. So why not create
an interface named like that?

    interface Function<A,B> {
        B Execute(A a);
    }

This describes just a function. We can execute it. The name `Execute` doesn't really matter. It could
be any name. I mean, just think about it. What can you do with an interface with one method anyway? You
either can execute that one method, or don't.

Now we can create a similar `List.map` function.

    class List {
        public static List<B> map<A,B>(Function<A,B> f, List<A> xs) {
            var result = new List<B>();
            foreach (var x in xs) {
                result.Add( f.Execute(x) );
            }
            return result;
        }
    }

So exactly the same like the F# version, we now can create functions on its own. But there are
wrapped in classes as methods.

    class Double : Function<int,int> {
        public int Execute(int x) {
            return x * 2;
        }
    }

We can use everything like this.

    var doubles = List.map(new Double(), numbers);

So, isn't it the same? Yes and no. The concept is different. Now we can create more generci
functions. Consider that `new Double()` is a function that doesn't iterate over a list and can
be used somewhere else. It compares to a normal static helper method we had written.

But, now let's go to modern C#. Because, do you know that C# already added such a generic interface
like `Function<A,B>`?

In C# you have two interfaces named `Action<...>` and `Func<...>`.

* `Action<A>`   is comparable to: `public void Execute<A>(A x)`
* `Action<A,B>` is comparable to: `public void Execute<A,B>(A x, B y)`
* `Func<A>`     is comparable to: `public A Execute<A>()`
* `Func<A,B>`   is comparable to: 'public B Execute<A>(A x)'

`Action<...>` are those functions that always return a `void` while the types just represent the
types of the Argument. So a `Action<int,string,double>` is just a function with three arguments
of type `int` then `string` and finally `double`.

While `Func<...>` describes a function with a return value. The last type of it is the return type.
So `Fun<int,string,double>` is a function with two arguments `int` and `string` that returns `double`.
In F# functions are defined with arrows `->`, and generic values have a tick (') preprended. `unit` is
the special type that reprents `void`.

So if you see `'a -> 'b -> unit` it is comparable to `Action<A,B>' while `'a -> 'b -> 'c` is
comparable to `Func<A,B,C>`

Technically F# only has `Func` as void is just a type. `Action<A>` turns into `'a -> unit` in F#.

So, let's rewrite our `List.map` in C# again.

    class List {
        public static List<B> map<A,B>(Func<A,B> f, List<A> xs) {
            var result = new List<B>();
            foreach (var x in xs) {
                result.Add( f.Invoke(x) );
            }
            return result;
        }
    }

Now you see `f.Invoke(x)` to execute our function. C# represent Functions through the class
`Delegate`. But we also can write `f(x)` and omit the `.Invoke()` method call.

But C# doesn't allow to inherit from a `Delegate` class, but that's not bad. We instead have
a much niver syntax. We can just write `x => x * 2` that actually creates a `Delegate` object
that has `x * 2` as the `Invoke` function implemented. C# creates an anymous Delegate class
for you. So now we can write:

    var doubles = List.map(x => x * 2, numbers);

in C# too!

That's what modern C# users know and use. But under the hood, this so called "lambda expression"
just creates an anonymous class of type `Delegate` that Implements the `Action` or `Func` interface. You
just provide the `Invoke` Body, and do it inline.

Design Patterns.

Now, think about some design Patterns. For example the Strategy Design Pattern starts with something
like this. Example from Wikipedia: https://en.wikipedia.org/wiki/Strategy_pattern#C#

    interface IBillingStrategy {
        double GetActPrice(double rawPrice);
    }

Can you now see the modern OO Bullshit you are doing over and over again? What you see here is nothing
else as just a

    Func<double,double>

another example? Do you know the Command-Pattern? From Wikipedia: https://en.wikipedia.org/wiki/Command_pattern#Example

    public interface ICommand {
        void Execute();
    }

I hope, you get it now! Some of the so called "Design Patterns" was just a lack that OO languages back
in the days, didn't had the ability to pass functions as arguments, and you had to wrap them
in classes from where you create objects, and the lack of creating them easily inline with
a lambda expression.

Here is another example. Do you know the Ierator Pattern? It is often defined as:

    public interface Iterator<A> {
        bool hasNext();
        A next();
    }

This seems not obious first, but you could implement it as:

    unit -> option<'a>

The purpose of `next` is to calculate the next element. But you don't know if a new
element can be calculated so the pattern to use that pattern (funny?) is to check with
`hasNext` if a new element can be calculated and if true call `next`.

In the F# version you just create a function taking no argument, and it either returns
an value, or not. This is described through the `option`. So you don't need a separate `hasNext()`
you just call the function and check what it returns.
*)

let range start stop =
    let mutable current = start
    fun () ->
        if current <= stop then
            current <- current + 1
            Some current
        else
            None

// unit -> option<int> // returns a function -- iterator
let r1 = range 5 7

printfn "%A" ( r1() ) // Some 6
printfn "%A" ( r1() ) // Some 7
printfn "%A" ( r1() ) // Some 8
printfn "%A" ( r1() ) // None

(*
An iterator is just a function that you initialise with a mutable state. And that is the same as an
object. But use the `Seq` implementation (F#) or LINQ (C#) instead of doing it on your own. It has
some more advantages. Except you want to learn the concepts yourself.
*)


// Tests
printfn "double: %A" (double numbers)
printfn "square: %A" (square numbers)
printfn "add10:  %A" (add10 numbers)
printfn "add5:   %A" (addX 5 numbers)
printfn "map double: %A" (map (fun x -> x * 2) numbers)
printfn "map square: %A" (map (fun x -> x * x) numbers)
printfn "map +10:    %A" (map (fun x -> x + 10) numbers)
