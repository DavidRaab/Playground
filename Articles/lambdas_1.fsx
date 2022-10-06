#!/usr/bin/env -S dotnet fsi

let start () =
    printfn "Starting..."

let draw txt =
    printfn "Drawing %s" txt

let stop () =
    printfn "Stoping..."

(*
Sometimes in code there are Pattern like. Initialize something,
do something in between and then close something. For example
in MonoGame there is spriteBatch that always looks like.

    spriteBatch.Begin()
    spriteBatch.Draw() // Multiple Draw calls
    spriteBatch.End()

We can make life easier, by creating a higher-order function
that expects the stuff beteen something. For example.
*)

let between code =
    start ()
    code ()
    stop ()

between (fun _ ->
    draw "Hello"
    draw "World"
)

(* If you want to keep some result of the lamda, sure *)

let between' code =
    start ()
    let ret = code ()
    stop()
    ret


(*
You also can pass arguments to code, for example in MonoGame
we could even initialize the spriteBatch

    let graphicsDevice = ...

    let between code =
        let sb = SpiteBatch(graphicsDevice)
        sb.Begin()
        code sb
        sb.End()

    between (fun sb ->
        sb.Draw(...)
        sb.Draw(...)
    )

Or in other words. You can initialize something. Provide a default config.
and then pass everything to a lambda that the user can work with. After the
user  has done its work you can provide a cleanup.

The above stuff is implemented in C#/.Net as the IDisposable interface with the
using "keyword".

In F# "using" is a function expecting an IDisposable object and a function that
does something with it. "using" returns whatever you return in your lambda.

Bad languages need to extend the language itself even for the easiest patterns
out there.
*)