#!/usr/bin/env -S dotnet fsi --multiemit-

#load "Lib/Animation.fsx"
#load "Lib/Test.fsx"
open Animation
open Test

// Some Helper functions
let sec x = TimeSpan.FromSeconds x
let ms  x = TimeSpan.FromMilliseconds x

let lerp start stop fraction =
    (start * (1.0 - fraction)) + (stop * fraction)

let is_anim anim dt xs name =
    let anim      = Animation.run anim
    let mutable i = 0
    for x in xs do
        let value = Anim.value dt anim
        Test.is value x (sprintf "%s index %d" name i)
        i <- i + 1

let finishedAfter anim dt value name =
    match Anim.run dt (Animation.run anim) with
    | Running   x    -> Test.fail name
    | Finished (x,t) -> Test.is x value name

let runningAt anim dt value name =
    match Anim.run dt (Animation.run anim) with
    | Running   x    -> Test.is x value name
    | Finished (x,t) -> Test.fail name

// Testing
is_anim (Animation.wrap 1) (sec 1) [1;1;1] "is 1"
is_anim (Animation.wrap 5) (sec 1) [5;5;5] "is 5"

finishedAfter (Animation.wrap 1) (sec 0) 1 "wrap(1) is finished"
finishedAfter (Animation.wrap 5) (sec 0) 5 "wrap(5) is finished"

Test.is
    (Anim.run (sec 1) (Animation.run (Animation.wrap 1)))
    (Finished (1, sec 1))
    "wrap 1 finished with 1 seconds left"

let ten1sec  = Animation.fromLerp (lerp 1 10) (sec  1)
let ten2sec  = Animation.fromLerp (lerp 1 10) (sec  2)
let ten10sec = Animation.fromLerp (lerp 1 10) (sec 10)

is_anim ten1sec  (ms 500) [5.5;  10.0; 10.0] "toTen in 1 sec"
is_anim ten2sec  (ms 500) [3.25;  5.5;  7.75; 10.0; 10.0] "toTen in 2 sec"
is_anim ten10sec (sec 3)  [3.7;   6.4;  9.1;  10.0; 10.0] "toTen in 10 sec"

Test.doneTesting ()