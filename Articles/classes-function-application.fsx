#!/usr/bin/env -S dotnet fsi

(*
Classes are just function application.
Consider the following class in F#
*)

[<StructuredFormatDisplay("PositionC({x},{y})")>]
type Position(x,y) =
    member this.x = x
    member this.y = y

    member this.Add(p : Position) =
        new Position(this.x + p.x, this.y + p.y)

    member this.Subtract(p: Position) =
        new Position(this.x - p.x, this.y - p.y)

let p1 = Position(3,5) // "new" keyword is not needed in F#
let p2 = Position(7,3)
let p3 = p1.Add(p2)

printfn "%A" p3 // Position(10,10)

(*
All we do here is the same as an `add` function getting four arguments. Whenever
we create an object and store values as a member/attribute. It is the same
as storing or keeping the values in memory.

All the methods, can now access those member and/or change them. But you also
just can create a function with all those arguments in one go.
*)

let add (x1,y1) (x2,y2) =
    x1 + x2, y1 + y2

(* You can pass all arguemnts at once *)
add (3,5) (7,5)

(* Or just partial apply some of them *)
let p3Add = add (3,5)
p3Add (7,5)

(* But in this case it makes sense t create a record type so our type and its values get a name *)
[<StructuredFormatDisplay("PositionR({X},{Y})")>]
type Pos = { X: int; Y: int }

(* Now we have functions getting two positions. Instead of getting two tuples *)

module Pos =
    let create x y = {X=x; Y=y}
    let add a b    = create (a.X + b.X) (b.Y + b.Y)
    let sub a b    = create (a.X - b.X) (b.Y - b.Y)

let pos1 = Pos.create 3 5
let pos2 = Pos.create 7 5
let pos3 = Pos.add pos1 pos2
let pos4 = Pos.sub pos1 pos2

printfn "%A" pos3 // {X=10; Y=10}
printfn "%A" pos4 // {X=-4; Y=0}

(* The general idea is that you always can transform something like

    let obj = new SomeObject(a,b,c)
    let ret = obj.someMethod(d,e,f)

into

    let ret = someFunc a b c d e f

and sometimes it makes sense to create a type of those attributes you initially pass to
an class constructor. So your code turns into

    let record = Record.create a b c
    let ret    = someFunc d e f record

Even if you don't see this in C++, Java, C# or a lot of other languages. Remember that whenever you
call a method on an object it hiddenly also passes a `this` object. You can think of this `this` object
as nothing else as a common data-structure/hash/Dictionary/Map and so on that just keeps track of
your attributes.

This is important as basically every object method call like.

    obj.method(a,b,c)

is the same as

    func obj a b c

In F# it is common to put the `obj` parameter as the last argument. This has to do with how
the language work with partial application and piping. But you can choose any other order if
you want.

This kind of transformation becomes more visible if you have those one method interfaces. Because
if you do.

    let obj = new Bla(x,y,z)
    obj.DoSomething(a,b,c)

and the usual way is to instantiate and directly call the `DoSomething` method on it. Then
you also can just replace it by a static Method or just a function. In F# you can do that anyway
even if you not directly want to execute `DoSomething` or change the other values. For example

    let common = func x y z
    let d1     = common a b c
    let d2     = common d e f
    let d3     = common g h i

would be the same as

    let obj = new Bla(x,y,z)
    let d1  = obj.DoSomething(a,b,c)
    let d2  = obj.DoSomething(d,e,f)
    let d3  = obj.DoSomething(g,h,i)

but you get this Partial Application for free in F# with every function! You can achieve the same
in C# for example if you have a static method with three arguments.

    class Fun {
        public static int add(int a, int b, int c, int d, int e, int f) {
            return a + b + c + d + e + f;
        }
    }

You can get a Partial Applied version from it this way.

    Func<int,int,int,int> add10 = (d,e,f) => Fun.add(2,2,6,d,e,f);

and just execute it

    Console.WriteLine( add10(2,3,7) );
    Console.WriteLine( add10(10,10,10) );
    Console.WriteLine( add10(2,3,9) );
*)