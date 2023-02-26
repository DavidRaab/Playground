#!/usr/bin/env -S dotnet fsi --multiemit-

#load "Lib/Animation.fsx"
#load "Lib/Test.fsx"
open Animation
open Test

// Some Helper functions
let sec x = TimeSpan.FromSeconds x
let ms  x = TimeSpan.FromMilliseconds x

// Animation.wrap
Test.is (Animation.toList (sec 1) (Animation.wrap 1)) [1] "wrap(1) is [1]"
Test.is (Animation.toList (sec 0) (Animation.wrap 5)) [5] "wrap(5) is [5]"

Test.is
    (Anim.run (sec 1) (Animation.run (Animation.wrap 1)))
    (Finished (1, sec 1))
    "wrap 1 finished with 1 seconds left"

// Animation that runs from 1.0 to 10.0 ...
let testToTen =
    let toTen dt = Animation.fromLerp dt (Lerp.float 1 10)

    Test.floatList
        (Animation.toList (ms 500) (toTen (sec 1)))
        [5.5; 10]
        "Lerp.float 1 10 in 1 seconds"

    Test.floatList
        (Animation.toList (ms 500) (toTen (sec 2)) )
        [3.25;  5.5;  7.75; 10.0]
        "Lerp.float 1 10 in 2 seconds"

    Test.floatList
        (Animation.toList (ms 500) (toTen (sec 10)))
        [1.45; 1.9; 2.35; 2.8; 3.25; 3.7; 4.15; 4.6; 5.05; 5.5; 5.95; 6.4; 6.85; 7.3; 7.75; 8.2; 8.65; 9.1; 9.55; 10.0]
        "Lerp.float 1 10 in 10 seconds"

// Animation with Lerp.int
Test.is
    (Animation.toList (ms 100) (Animation.fromLerp (sec 1) (Lerp.int 0 5)))
    [0;1;1;2;2;3;3;4;4;5]
    "Lerp.int 0 5 with 100ms"

Test.floatList
    (Animation.toList (ms 100) (Animation.map (fun x -> x * 2.0) (Animation.fromLerp (sec 1) (Lerp.float 1 3))))
    [2.4; 2.8; 3.2; 3.6; 4.0; 4.4; 4.8; 5.2; 5.6; 6.0]
    "Animation.map"

// Animation from 1 to 3 downto 1
Test.floatList
    (Animation.toList
        (ms 250)
        (Animation.append
            (Animation.fromLerp (sec 1) (Lerp.float 1 3))
            (Animation.fromLerp (sec 1) (Lerp.float 3 1) )))
    [1.5; 2.0; 2.5; 3.0; 2.5; 2.0; 1.5; 1.0]
    "Animation.append"

Test.is
    (Animation.toList (ms 100) (Animation.ofList [1..5]))
    [1..5]
    "Animation.ofList"

Test.throws
    (fun () -> Animation.ofList [] |> ignore)
    "Animation.ofList throws on empty list"

Test.is
    (Animation.toList
        (ms 100)
        (Animation.append
            (Animation.duration (ms 300) 1)
            (Animation.duration (ms 300) 2)))
    [1;1;1;2;2;2]
    "Animation.append"

Test.is
    (Animation.toList
        (ms 100)
        (Animation.flatten
            (Animation.ofList [
                Animation.duration (ms 300) 1
                Animation.duration (ms 300) 2
                Animation.duration (ms 300) 3
            ])))
    [1;1;1;2;2;2;3;3;3]
    "Animation.flatten and Animation.duration"

Test.is
    (Animation.toList
        (ms 100)
        (Animation.repeat 3
            (Animation.flatten
                    (Animation.ofList [
                        Animation.duration (ms 200) 1
                        Animation.duration (ms 200) 2
                        Animation.duration (ms 200) 3
                    ]))))
    [1;1;2;2;3;3;1;1;2;2;3;3;1;1;2;2;3;3]
    "Animation.repeat"

let testMap2 =
    let map2 =
        (Animation.toList
            (ms 100)
            (Animation.map2 (fun x y -> (x,y))
                (Animation.duration (ms 300) "anim1")
                (Animation.fromLerp (ms 300) (Lerp.float 1 3))))

    let fsts = List.map fst map2
    let snds = List.map snd map2

    Test.is        fsts (List.replicate 3 "anim1")      "Animation.map2 first"
    Test.floatList snds [1.666666667; 2.333333333; 3.0] "Animation.map2 second"

Test.doneTesting ()