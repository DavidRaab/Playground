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
let test_toTen =
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
    (Animation.toList (ms 100) (Animation.ofSeq [1..5]))
    [1..5]
    "Animation.ofList"

Test.throws
    (fun () -> Animation.ofSeq [] |> ignore)
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
            (Animation.ofSeq [
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
                    (Animation.ofSeq [
                        Animation.duration (ms 200) 1
                        Animation.duration (ms 200) 2
                        Animation.duration (ms 200) 3
                    ]))))
    [1;1;2;2;3;3;1;1;2;2;3;3;1;1;2;2;3;3]
    "Animation.repeat"

let test_map2 =
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

let test_zip =
    let map2 =
        (Animation.toList
            (ms 100)
            (Animation.zip
                (Animation.fromLerp (ms 300) (Lerp.float 3 1))
                (Animation.fromLerp (ms 300) (Lerp.float 1 3))))

    let fsts = List.map fst map2
    let snds = List.map snd map2

    Test.floatList fsts [2.333333333; 1.666666667; 1.0] "Animation.zip first"
    Test.floatList snds [1.666666667; 2.333333333; 3.0] "Animation.zip second"

Test.is
    (Animation.toList
        (ms 100)
        (Animation.ofSeqDuration (ms 200) [1;2;3]))
    [1;1;2;2;3;3]
    "Animation.ofSeqDuration"

let test_longest =
    Test.is
        (Animation.toList
            (ms 100)
            (Animation.zip
                (Animation.fromLerp (ms  500) (Lerp.float 0 10))
                (Animation.fromLerp (ms 1000) (Lerp.float 50 100))))
        [
            ( 2, 55); ( 4, 60); ( 6, 65); ( 8, 70); (10, 75)
            (10, 80); (10, 85); (10, 90); (10, 95); (10, 100)
        ]
        "1. longest running animation is used"

    Test.is
        (Anim.run (ms 1000)
            (Animation.run
                (Animation.zip4
                    (Animation.fromLerp (ms 100) (Lerp.int 0 1))
                    (Animation.fromLerp (ms 400) (Lerp.int 0 2))
                    (Animation.fromLerp (ms 700) (Lerp.int 0 3))
                    (Animation.fromLerp (ms 300) (Lerp.int 0 4)))))
        (Finished ((1,2,3,4), (ms 300)))
        "2. check timeLeft from longest animation"

let test_timeLeft =
    Test.is
        (Anim.run (ms 300) (Animation.run (Animation.duration (ms 250) 1)))
        (Finished (1,ms 50))
        "1. 50ms is left when 250ms animation is runned for 300ms"

    Test.is
        (Anim.run (ms 350)
            (Animation.run
                (Animation.zip
                    (Animation.duration (ms 250) 1)
                    (Animation.duration (ms 300) 2))))
        (Finished ((1,2),(ms 50)))
        "2. 50ms is left when 250ms animation is runned for 300ms"

let test_speed =
    // test velocity towards increasing value
    let toTen = Animation.speed 0.0 10.0 2.0
    Test.floatList
        (Animation.toList (ms 1000) toTen)
        [2.0; 4.0; 6.0; 8.0; 10.0]
        "from 0.0 to 10.0 with +2.0/sec and steptime 1000ms"

    Test.floatList
        (Animation.toList (ms 500) toTen)
        [1.0; 2.0; 3.0; 4.0; 5.0; 6.0; 7.0; 8.0; 9.0; 10.0]
        "from 0.0 to 10.0 with +2.0/sec and steptime 500ms"

    // test velocity with decreasing value
    let toZero = Animation.speed 10.0 0.0 2.0
    Test.floatList
        (Animation.toList (ms 1000) toZero)
        [8.0; 6.0; 4.0; 2.0; 0.0]
        "from 10.0 to 0.0 with -2.0/sec and steptime 1000ms"

    Test.floatList
        (Animation.toList (ms 500) toZero)
        [9.0; 8.0; 7.0; 6.0; 5.0; 4.0; 3.0; 2.0; 1.0; 0.0]
        "from 10.0 to 0.0 with -2.0/sec and steptime 500ms"

    // Helpers to extract from triplet list
    let firsts  xs = List.map (fun (x,y,z) -> x) xs
    let seconds xs = List.map (fun (x,y,z) -> y) xs
    let thirds  xs = List.map (fun (x,y,z) -> z) xs

    // Check speed with zip3
    let threes =
        (Animation.toList (ms 1000)
            (Animation.zip3
                (Animation.speed 0 100 10)
                (Animation.speed 100 0 10)
                (Animation.speed 20 30  1)))

    Test.floatList (firsts threes)  [10;20;30;40;50;60;70;80;90;100] "1. firsts  of zip3 of speed"
    Test.floatList (seconds threes) [90;80;70;60;50;40;30;20;10;0]   "1. seconds of zip3 of speed"
    Test.floatList (thirds threes)  [21;22;23;24;25;26;27;28;29;30]  "1. thirds  of zip3 of speed"

    // Again speed with zip3 but different running times of animations
    let threes =
        (Animation.toList (ms 1000)
            (Animation.zip3
                (Animation.speed 0 100 15)
                (Animation.speed 100 0 20)
                (Animation.speed 20 30  1)))

    "2. firsts  of zip3 of speed"
    |> Test.floatList (firsts threes)  [15.0; 30.0; 45.0; 60.0; 75.0; 90.0; 100.0; 100.0; 100.0; 100.0]

    "2. seconds of zip3 of speed"
    |> Test.floatList (seconds threes) [80.0; 60.0; 40.0; 20.0; 0.0; 0.0; 0.0; 0.0; 0.0; 0.0]

    "2. thirds  of zip3 of speed"
    |> Test.floatList (thirds threes)  [21;22;23;24;25;26;27;28;29;30]

Test.doneTesting ()