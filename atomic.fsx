#!/usr/bin/env -S dotnet fsi --multiemit-

#load "Lib/Test.fsx"
#load "Lib/Atomic.fsx"
open Test
open Atomic

let radius = Atomic.value 1.0
let area   = Atomic.map (fun r -> r * r * System.Math.PI) radius

let nearly x y =
    (abs (x - y)) < 0.000001

// printfn "Radius: %f" (Atomic.get radius)

Test.is (Atomic.get radius) 1.0 "Radius 1.0"
Test.is (Atomic.get area) (System.Math.PI) "Area"

Atomic.set 2.0 radius
Test.ok (nearly (Atomic.get radius) 2.0)        "Radius 2.0"
Test.ok (nearly (Atomic.get area) 12.566370614) "Area of 2.0"

Atomic.set 3.0 radius
Test.ok (nearly (Atomic.get radius) 3.0)        "Radius 3.0"
Test.ok (nearly (Atomic.get area) 28.274333882) "Area of 3.0"

Atomic.set 5.0 area
Test.ok (nearly (Atomic.get radius) 3.0)        "still radius 3.0"
Test.ok (nearly (Atomic.get area) 28.274333882) "still area of 3.0"

type Vector2 = Vector2 of float * float

let x = Atomic.value 5.0
let y = Atomic.value 7.0

let xy  = Atomic.map2 (fun x y -> x + y) x y
let vec = Atomic.map2 (fun x y -> Vector2(x,y)) x y

Test.is (Atomic.get x) 5.0 "x is 5.0"
Test.is (Atomic.get y) 7.0 "y is 7.0"
Test.is (Atomic.get xy) (12.0) "x + y is 12.0"
Test.is (Atomic.get vec) (Vector2(5.0,7.0)) "Vector2 of x,y"

Atomic.set 10.0 x

Test.is (Atomic.get x) 10.0 "x is now 10.0"
Test.is (Atomic.get xy) 17.0 "xy is now 17.0"
Test.is (Atomic.get vec) (Vector2(10,7.0)) "Vector2 of x,y"

Test.doneTesting ()