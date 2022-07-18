#!/usr/bin/env -S dotnet fsi

type Point = {
    getX: unit  -> float
    getY: unit  -> float
    setX: float -> unit
    setY: float -> unit
    add:  Point -> unit
    show: unit  -> string
}

let newPoint x y =
    let mutable x = x
    let mutable y = y
    {
        getX = fun () -> x
        getY = fun () -> y
        setX = fun newX -> x <- newX
        setY = fun newY -> y <- newY
        add  = fun p ->
            x <- x + p.getX()
            y <- y + p.getY()
        show = fun () -> sprintf "{X=%f; Y=%f}" x y
    }


let p1 = newPoint 1.0 1.0
let p2 = newPoint 0.5 2.0

p1.add(p2)

printfn "%s %s" (p1.show()) (p2.show())
