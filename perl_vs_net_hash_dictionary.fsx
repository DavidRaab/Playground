#!/usr/bin/env -S dotnet fsi

open System.Collections.Generic

(* Perl Hashes vs. .Net Dictionary

One of the most common data-structure used in Perl is a Hash or in other languages
like .Net called a Dictionary. Sometimes comming from a Perl background it makes me
wonder why Hashes are used so few. I have seen much of cases where people use
a List where a Dictionary would make much sense.

Now that i am using .Net, i can understand it more. The Dictionary API in
.Net feels just horrible. Let's pick an easy example. You have a list
of words, and you want that these List of words get grouped by the amount
of characters every word has. A very easy task not so uncommon i would
solve with a Hash in Perl.

So here is the API in Perl. There are two ways to create a Hash, either if
you want it to be a hash or a reference to a hash.

    my %wordMap = ();
    my $wordMap = {}; // reference to hash

Many other languages don't have that distinction or they just hide it from you.
In other languages you usually get the second version. So that's what i'm using here.

So how can I add a new value to that hash? Very easy.

    $wordMap->{Key} = "Value"

Or in general, you just assign a value to a key. How can i change a key
that already exists in a Hash?

    $wordMap->{Key} = "Value"

So what is the difference? Well there is none. You just assign a value to
a key. And either way the key is created or is assaigned a new value. This
makes much sense to me and is one reason why it is used everywhere.

So how looks the complete version in Perl?

    my $wordMap = {};
    for my $word (qw/Hello There who are you? Missing something?/) {
        my $key = length $word;
        push @{$wordMap->{$key}}, $word;
    }

When this code is run `$wordmap` would now look like this.

    my $wordmap = {
        3  => ["who", "are"]
        4  => ["you?"],
        5  => ["Hello","There"],
        7  => ["Missing"],
        10 => ["something?"],
    };

If you understand JSON, you also understand Perl-Datastructures. It is the same, but
we only use `=>` instead of a colon `:`.

But some explanations to the Perl code first. `qw//` is just a way of saying. This is
an array where every element is splitted on whitespace.

The `push` line probably needs some explanation. First we get the key `$wordMap->{$key}`
that sometimes can return nothing or `undef` in Perl. By Putting `@{}` around it we say
that we want to use that as an array and add `push` a value to it. In the case of
`undef` a new array is created and the value is added to that array. Okay, that part
has more todo with dynamic typing or the presence of `undef` and how a language threat
that value. But anyway you get things easily done.

It also has a little bit todo with functional/procedural- vs. object-oriented programming. As
we have `push` as a stand-alone function not as a method. If Arrays would be objects in Perl
and adding to an array would only be accessible by a method. This kind of stuff wouldn't work.

Because code like

    $wordMap->{$key}->push($word);

would then fail to work. As it would return an `undef` and an `undef` has no method `push`
on it, because it is not an array!

So how do you similar in .Net? (I use F# not C#)

You create a Dictionary like this.

    open System.Collections.Generic
    let wordMap = Dictionary<int,string>()

You must think about the types but that is totaly fine in my opinion.

But here comes the problem. How do we add a new key? Easy ...

    wordMap.Add(key,value)

But ... what happens if we try to add a key, that already exists? Well then your code throws
an exception. I don't know you, but I don't want my code to throw errors i want my code to do
something. So how does adding really works? Is it really just calling an `.Add()`?

The answer is no. You always must write at least code like this.

    if wordMap.ContainsKey(key) then
        wordMap.Add(key,value)

it only adds a key when it doesn't exist. So how can we change a key? Easy again, or?

    wordMap.[key] <- value

But ... it throws an exception if a key doesn't exists. I mean, this is completely silly in
my opinion. Why does both of them throw exceptions? Why does `Add` not just overwrite the
key, or `.[key]` let the value be created if needed? It is a pain in the ass. But maybe
it has a lot todo with its static typing. Because sure, if the value should be an array,
you must create an Array, or whatever you put in as a value. So code always turns into.

    if wordMap.ContainsKey(key)
    then wordMap.Add(key, value)
    else wordMap.[key] <- value

This can be inefficent. Sometimes you also want to read a value, and update it in one go.
For example in Perl I could write:

    my $counter = {}
    $counter->{word} = $counter->{word} + 1;

to create something like a counter. If you do `(undef + 1)` then Perl threats `undef` as 0.

In .Net you would at least need to write something like this to do something similar.
*)

let counter = Dictionary<string,int>()

let mutable value = Unchecked.defaultof<_>
if counter.TryGetValue("word", &value)
then counter.["word"] <- value + 1
else counter.Add("word", 0)

(*
I mean, getting the value that doesn't exists makes some sense in .Net with a statically
typed language. For example, what should happen if you do.

    let date = someDic.[key]

and expect it to be a `System.DateTime` object? Sure it cannot or should create a `DateTime`
on its own. But for me, its also not an exception. Maybe returning a `null` or if .Net had
`Option` returning this kind would make more sense, then just throwing an exception.

But at least `Add` could always just overwrite the existing Key instead of throwing an exception.

In Perl you also can ask if a key exists in Perl with the `exist` function and so on, if what
you need to-do is more complicated. But for the common tasks Perl has useful "defaults". If
you threat `undef` as a string it is used as an empty string. If you use it as an Array or Hash,
it is initialized as an empty Array or Hash. Or you just threat `undef` yourself however you need
it.

For any other complex object, you must check with `exist` and do the initialization yourself. But
because of this defaults it is easy to create data-structures. And you probably have done the same
a billion times in JavaScript already.

But, the problem is. The API could be much better. .Net could have choosen a better API, but that's
all they come up with. At least I can add my own functions, to get out of this mess. So I usually add
those functions when i work with Dictionary.
*)

type Dictionary<'a,'b> with
    static member add key value (dic:Dictionary<'a,'b>) =
        if   dic.ContainsKey(key)
        then dic.[key] <- value
        else dic.Add(key,value)

    static member change init key f (dic:Dictionary<'a,'b>) =
        let mutable value = Unchecked.defaultof<_>
        if dic.TryGetValue(key, &value)
        then dic.[key] <- f value
        else dic.Add(key, f init)

(* So here is the solution with `wordMap` in .Net with my helper functions. *)

let words = ["Hello"; "There"; "who"; "are"; "you?"; "Missing"; "something?"]
let wordMap = Dictionary<int,list<string>>()
for word in words do
    Dictionary.change [] (String.length word) (fun array -> word :: array) wordMap

(*
Well `Dictionary.add` is not used here. But you probably still want this.
Here is the same without my helper function. *)

let words' = ["Hello"; "There"; "who"; "are"; "you?"; "Missing"; "something?"]
let wordMap' = Dictionary<int,list<string>>()
for word in words' do
    let key = String.length word
    let mutable value = Unchecked.defaultof<_>
    if wordMap'.TryGetValue(key, &value)
    then wordMap'.[key] <- word :: value
    else wordMap'.Add(key, [word])

(*
And i think writting this is a pain. It is too much code. Its already a "Pattern" in some way that you copy over and
over again and just change the things that has to be changed. But the changes are extremly small.

Why is the default .NET API this way? Okay it has some reason.

1. The Dictionary API was created when .NET/C# didn't had lambda support. So you couldn't pass a function
   as an argument to create something similar like `change`. You could simulate that through classes. But
   I guess nobody wants to create a class just because somewhere in your code you want to add a key to
   a dictionary. That would just be to verbose.
2. Everything in .NET/C# is mutable by default. Also consider that the `change` woudl break if I has used
   `ResizeArray` as the default, instead of `[]` (immutable F# List). Because if you have mutables. The the
   init value would be a shared array for all values. What is not what you want. But with immutable values
   there is no such problem as sharing the inital value.
3. You can easily fix the second problem by providing an `init` function that creates the value instead of
   the value itself. But that just comes down to the first problem that .NET/C# didn't had lambdas. So
   we running in circles.

BUT ... we are now at .NET 6 and C# 10. There is no reason why that Dictionary API is still that horrible.
And because of this, i don't wonder why Dictionary that makes so much sense in a lot of code, is still
used so less.

Btw. here is a full F# example that uses a `Map` (immutable Dictionary)
*)

let words''   = ["Hello"; "There"; "who"; "are"; "you?"; "Missing"; "something?"]
let wordMap'' =
    words'' |> List.fold (fun wm word ->
        Map.change (String.length word) (function
            | None       -> Some [word]
            | Some words -> Some (word :: words)
        ) wm
    ) Map.empty

(*
Consider `List.fold` as looping through every element of a list with the starting state of `Map.empty`.
It calls `Map.change` on the current loop state to change a key. The function you provide gets an `option` that
indicates whether there was a key present or not. You return another `option` to either initialize the
value or even delete a key,value pair. Or just do the operation if the value is present, otherwise do nothing. You
have full flexibility what you need.

But even in F# i probably would add:
*)
module Map =
    let changeValue (init:'Value) (key:'Key) f map =
        Map.change key (function
            | None   -> Some (f init)
            | Some x -> Some (f x)
        ) map

(* so I can write *)

let words'''   = ["Hello"; "There"; "who"; "are"; "you?"; "Missing"; "something?"]
let wordMap''' =
    words''' |> List.fold (fun wm word ->
        Map.changeValue [] (String.length word) (fun words -> word :: words) wm
    ) Map.empty

printfn "%A" wordMap'''