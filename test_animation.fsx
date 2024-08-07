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
    (Animation.execute (sec 1) (Animation.wrap 1))
    (Anim.finished 1 (sec 1))
    "wrap 1 finished with 1 seconds left"

// Animation that runs from 1.0 to 10.0 ...
let test_toTen =
    let toTen = Animation.range 1 10

    Test.floatList
        (Animation.toList (ms 500) (toTen (sec 1)))
        [5.5; 10]
        "Animation.range 1 10 in 1 seconds"

    Test.floatList
        (Animation.toList (ms 500) (toTen (sec 2)) )
        [3.25;  5.5;  7.75; 10.0]
        "Animation.range 1 10 in 2 seconds"

    Test.floatList
        (Animation.toList (ms 500) (toTen (sec 10)))
        [1.45; 1.9; 2.35; 2.8; 3.25; 3.7; 4.15; 4.6; 5.05; 5.5; 5.95; 6.4; 6.85; 7.3; 7.75; 8.2; 8.65; 9.1; 9.55; 10.0]
        "Animation.range 1 10 in 10 seconds"

Test.is
    (Animation.toList (ms 100) (Animation.rangeI 0 1 (sec 1)))
    [0;0;0;0;0;1;1;1;1;1]
    "Animation.rangeI 0 1 with 100ms"

Test.is
    (Animation.toList (ms 100) (Animation.rangeI 0 2 (sec 2)))
    [0;0;0;0;0;1;1;1;1;1;1;1;1;1;2;2;2;2;2;2]
    "Animation.rangeI 0 2 with 100ms"

Test.is
    (Animation.toList (ms 100) (Animation.rangeI 0 3 (sec 3)))
    [0;0;0;0;0;1;1;1;1;1;1;1;1;1;2;2;2;2;2;2;2;2;2;2;2;3;3;3;3;3]
    "Animation.rangeI 0 3 with 100ms"

Test.floatList
    (Animation.toList (ms 100) (Animation.map (fun x -> x * 2.0) (Animation.range 1 3 (sec 1))))
    [2.4; 2.8; 3.2; 3.6; 4.0; 4.4; 4.8; 5.2; 5.6; 6.0]
    "Animation.map"

// Animation from 1 to 3 downto 1
Test.floatList
    (Animation.toList
        (ms 250)
        (Animation.append
            (Animation.range 1 3 (sec 1))
            (Animation.range 3 1 (sec 1))))
    [1.5; 2.0; 2.5; 3.0; 2.5; 2.0; 1.5; 1.0]
    "Animation.append"

Test.is
    (Animation.toList (ms 100) (Animation.concatDuration (ms 100) [1..5]))
    [1..5]
    "Animation.ofList"

Test.throws
    (fun () -> Animation.concat [] |> ignore)
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
        (Animation.concat [
            Animation.duration (ms 300) 1
            Animation.duration (ms 300) 2
            Animation.duration (ms 300) 3
        ]))
    [1;1;1;2;2;2;3;3;3]
    "Animation.concat and Animation.duration"

Test.is
    (Animation.toList
        (ms 100)
        (Animation.repeat 3
            (Animation.concat [
                Animation.duration (ms 200) 1
                Animation.duration (ms 200) 2
                Animation.duration (ms 200) 3
            ])))
    [1;1;2;2;3;3; 1;1;2;2;3;3; 1;1;2;2;3;3]
    "Animation.repeat"

let test_map2 =
    let map2 =
        (Animation.toList
            (ms 100)
            (Animation.map2 (fun x y -> (x,y))
                (Animation.duration (ms 300) "anim1")
                (Animation.range 1 3 (ms 300))))

    let fsts = List.map fst map2
    let snds = List.map snd map2

    Test.is        fsts (List.replicate 3 "anim1")      "Animation.map2 first"
    Test.floatList snds [1.666666667; 2.333333333; 3.0] "Animation.map2 second"

let test_zip =
    let map2 =
        (Animation.toList
            (ms 100)
            (Animation.zip
                (Animation.range 3 1 (ms 300))
                (Animation.range 1 3 (ms 300))))

    let fsts = List.map fst map2
    let snds = List.map snd map2

    Test.floatList fsts [2.333333333; 1.666666667; 1.0] "Animation.zip first"
    Test.floatList snds [1.666666667; 2.333333333; 3.0] "Animation.zip second"

Test.is
    (Animation.toList
        (ms 100)
        (Animation.concatDuration (ms 200) [1;2;3]))
    [1;1;2;2;3;3]
    "Animation.concatDuration"

let test_longest =
    Test.is
        (Animation.toList
            (ms 100)
            (Animation.zip
                (Animation.range 0   10 (ms  500))
                (Animation.range 50 100 (ms 1000))))
        [
            ( 2, 55); ( 4, 60); ( 6, 65); ( 8, 70); (10, 75)
            (10, 80); (10, 85); (10, 90); (10, 95); (10, 100)
        ]
        "1. longest running animation is used"

    Test.is
        (Animation.execute (ms 1000)
            (Animation.zip4
                (Animation.rangeI 0 1 (ms 100))
                (Animation.rangeI 0 2 (ms 400))
                (Animation.rangeI 0 3 (ms 700))
                (Animation.rangeI 0 4 (ms 300))))
        (Anim.finished (1,2,3,4) (ms 300))
        "2. check timeLeft from longest animation"

let test_timeLeft =
    Test.is
        (Animation.execute (ms 300) (Animation.duration (ms 250) 1))
        (Anim.finished  1 (ms 50))
        "1. 50ms is left when 250ms animation is runned for 300ms"

    Test.is
        (Animation.execute (ms 350)
            (Animation.zip
                (Animation.duration (ms 250) 1)
                (Animation.duration (ms 300) 2)))
        (Anim.finished (1,2) (ms 50))
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

let test_traverse =
    let aseq = [
        Animation.range 1 10 (sec 1)
        Animation.range 1 5  (sec 1)
        Animation.range 10 1 (sec 1)
    ]

    Test.is
        (Animation.toList (ms 100) (Animation.traverse (fun x -> x * 2.0) aseq))
        [
            [ 3.8; 2.8; 18.2]; [ 5.6;  3.6; 16.4]; [ 7.4; 4.4; 14.6]; [ 9.2; 5.2; 12.8];
            [11.0; 6.0; 11.0]; [12.8;  6.8;  9.2]; [14.6; 7.6;  7.4]; [16.4; 8.4;  5.6];
            [18.2; 9.2;  3.8]; [20.0; 10.0;  2.0]
        ]
        "Animation.traverse"

    Test.is
        (Animation.toList (ms 100) (Animation.sequence aseq))
        [
            [1.9; 1.4; 9.1]; [2.8; 1.8; 8.2]; [3.7; 2.2; 7.3]; [4.6; 2.6; 6.4];
            [5.5; 3.0; 5.5]; [6.4; 3.4; 4.6]; [7.3; 3.8; 3.7]; [8.2; 4.2; 2.8];
            [9.1; 4.6; 1.9]; [10.0; 5.0; 1.0]
        ]
        "Animation.sequence"

Test.float32List
    (Animation.toList (ms 200) (Animation.rangeF 1f 10f (sec 1)))
    [2.799999952f; 4.599999905f; 6.400000095f; 8.199999809f; 10.0f]
    "Animation.rangeF"

Test.float32List
    (Animation.toList (ms 200) (Animation.map float32 (Animation.range 1 10 (sec 1))))
    (Animation.toList (ms 200) (Animation.rangeF 1f 10f (sec 1)))
    "Animation.range with map to float32 is the same as Animation.rangeF"


type Vector2 = {X:float; Y:float}
let vec2 x y = {X = x;   Y = y}

Test.is
    (Animation.toList
        (ms 100)
        (Animation.map2 vec2 (Animation.range 1 100 (sec 1)) (Animation.range 1 50 (sec 1))))
    [
        vec2 10.9 5.9;  vec2 20.8 10.8; vec2 30.7 15.7; vec2 40.6 20.6; vec2 50.5 25.5;
        vec2 60.4 30.4; vec2 70.3 35.3; vec2 80.2 40.2; vec2 90.1 45.1; vec2 100 50
    ]
    "Test a Vector2 animation"

Test.floatList
    (Animation.toList
        (ms 100)
        (Animation.rangeWith Ease.inSine 1 10 (sec 1)))
    [
        1.110804935; 1.440491353; 1.980941282;
        2.718847051; 3.636038969; 4.709932729;
        5.914085502; 7.218847051; 8.592089815;
        10.0
    ]
    "Animation.rangeWith Ease.inSine"

let test_overshoot =
    let anim =
        (Animation.append
            (Animation.range 1  10 (sec 1))
            (Animation.range 10 1  (sec 1)))

    Test.floatList
        (Animation.toList (ms 270) anim)
        [3.43; 5.86; 8.29; 9.28; 6.85; 4.42; 1.99; 1.0]
        "correct values when there was time left on previous animation"

    Test.is
        (Animation.execute (ms 2160) anim)
        (Anim.finished 1.0 (ms 160))
        "correct end value and remaining time on appended animation"

Test.is
    (Animation.toArray (ms 16) (Animation.range 0 1 (sec 1)))
    (List.toArray (Animation.toList (ms 16) (Animation.range 0 1 (sec 1))))
    "toList and toArray yields the same results when running"

Test.is
    (Animation.toList (ms 100)
        (Animation.append
            (Animation.range 1 10 (ms 500))
            (Animation.range 10 1 (ms 500))))
    (Animation.toList (ms 100) (Animation.roundTrip 1 10 (sec 1)))
    "roundTrip is the same as appending two animation forwards and backwards"

Test.ok
    (Animation.equal
        (ms 16)
        (Animation.append
            (Animation.range 1 10 (ms 500))
            (Animation.range 10 1 (ms 500)))
        (Animation.roundTrip 1 10 (sec 1)))
    "Animation.equal"

Test.floatList
    (Animation.toList (ms 100)
        (Animation.take (sec 1) (Animation.range 0 10 (sec 2))))
    [0.5; 1.0; 1.5; 2.0; 2.5; 3.0; 3.5; 4.0; 4.5; 5.0]
    "take 1 second of a 2 seconds animation"

Test.floatList
    (Animation.toList (ms 200)
        (Animation.take (sec 4) (Animation.loop (Animation.roundTrip 0 10 (sec 2)))))
    [
        2.0; 4.0; 6.0; 8.0; 10.0; 8.0; 6.0; 4.0; 2.0; 0.0
        2.0; 4.0; 6.0; 8.0; 10.0; 8.0; 6.0; 4.0; 2.0; 0.0
    ]
    "Take 4 seconds of an endless roundTrip animation"

Test.is
    (Animation.toList (ms 100) (Animation.take (sec 1) (Animation.wrap 5)))
    [5]
    "Taking more from a finished animation"

Test.is
    (Animation.execute (ms 100) (Animation.take (sec 1) (Animation.wrap 5)))
    (Anim.finished 5 (ms 100))
    "take 1 second of a wrapped value"

Test.floatList
    (Animation.toList (ms 200)
        (Animation.take (sec 10)
            (Animation.roundTrip 0 10 (sec 2))))
    [2; 4; 6; 8; 10; 8; 6; 4; 2; 0]
    "Taking 10 seconds from a 2 seconds animation yields the whole animation"

Test.floatList
    (Animation.toList (ms 200)
        (Animation.take (sec 1)
            (Animation.roundTrip 0 10 (sec 2))))
    [2; 4; 6; 8; 10]
    "Taking 1 seconds from a 2 seconds animation yields half of the animation"

Test.floatList
    (Animation.roundTrip 0 10 (sec 2)
    |> Animation.skip   (ms 500)
    |> Animation.take   (sec 1)
    |> Animation.toList (ms 100))
    [6; 7; 8; 9; 10; 9; 8; 7; 6; 5]
    "skip and take from a longer animation"

// Todo:
// * Easing functions

Test.doneTesting ()