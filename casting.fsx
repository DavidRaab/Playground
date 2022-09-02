#!/usr/bin/env -S dotnet fsi

// Interface
type IFoo =
    abstract member Print : unit -> unit

type Foo<'a>(value:'a) =
    member this.Value = value

    interface IFoo with
        member this.Print () =
            printfn "Hello %A" this.Value

let a = Foo(3)
let b = Foo("Hello")

for x in [a :> IFoo; b :> IFoo] do
    x.Print ()


// SRTP -- statically typed duck typing
type Bar<'a>(value:'a) =
    member this.Value    = value
    member this.Print () =
        printfn "Hello %A" this.Value

// Print function expects that x must implement method Print as `unit -> unit`
// It checks at compilation-time without need to define an interface
let inline printer x =
    (^Printer : (member Print: unit -> unit) x)

let w = Bar(3)
let x = Bar("Hello")

printer w
printer x

