#!/usr/bin/env -S dotnet fsi --multiemit-

#load "Lib/Animation.fsx"
#load "Lib/Test.fsx"
open Animation
open Test

// Some Helper functions
let sec x = TimeSpan.FromSeconds x
let ms  x = TimeSpan.FromMilliseconds x

// Animation.wrap
Test.is (Animation.toList (Animation.wrap 1) (sec 1)) [1] "wrap(1) is [1]"
Test.is (Animation.toList (Animation.wrap 5) (sec 0)) [5] "wrap(5) is [5]"

Test.is
    (Anim.run (sec 1) (Animation.run (Animation.wrap 1)))
    (Finished (1, sec 1))
    "wrap 1 finished with 1 seconds left"

// Animation that runs from 1.0 to 10.0 ...
let toTen = Animation.fromLerp (Lerp.float 1 10)

Test.is
    (Animation.toList (toTen (sec 1)) (ms 500))
    [5.5; 10]
    "Lerp.float 1 10 in 1 seconds"

Test.is
    (Animation.toList (toTen (sec 2)) (ms 500))
    [3.25;  5.5;  7.75; 10.0]
    "Lerp.float 1 10 in 2 seconds"

Test.is
    (Animation.toList (toTen (sec 10)) (ms 500))
    [1.45; 1.9; 2.35; 2.8; 3.25; 3.7; 4.15; 4.6; 5.05; 5.5; 5.95; 6.4; 6.85; 7.3; 7.75; 8.2; 8.65; 9.1; 9.55; 10.0]
    "Lerp.float 1 10 in 10 seconds"

// Animation with Lerp.int
Test.is
    (Animation.toList (Animation.fromLerp (Lerp.int 0 5) (sec 1)) (ms 100))
    [0;1;1;2;2;3;3;4;4;5]
    "Lerp.int 0 5 with 100ms"


Test.doneTesting ()