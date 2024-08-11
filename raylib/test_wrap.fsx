#!/usr/bin/env -S dotnet fsi

#load "Lib_RaylibHelper.fsx"
#load "../Lib/Test.fsx"

open Lib_RaylibHelper
open Test

Test.is
    (List.map (wrap 0f 10f) [2f .. 7f])
    [2f .. 7f]
    "No Wrapping"

Test.is
    (List.map (wrap 0f 10f) [5f .. 15f])
    [5f; 6f; 7f; 8f; 9f; 0f; 1f; 2f; 3f; 4f; 5f]
    "Wrapping if greater"

Test.is
    (List.map (wrap 0f 10f) [-5f .. 5f])
    [5f; 6f; 7f; 8f; 9f; 0f; 1f; 2f; 3f; 4f; 5f]
    "Wrapping if smaller"

Test.is
    (List.map (wrap -16f -8f) [-20f .. -15f])
    [-12f; -11f; -10f; -9f; -16f; -15f]
    "negatives"

Test.is
    (List.map (wrap -16f -8f) [-16f; -16.1f])
    [-16f; -8.1f]
    "small step"

let t1 =
    let xs     = [1..10]
    let length = List.length xs
    Test.is
        (List.map (fun idx -> xs.[int (wrap 0f (float32 length) (float32 idx))]) [-5 .. 5])
        [6;7;8;9;10;1;2;3;4;5;6]
        "negative indexing in array"

    Test.is
        (List.map (fun idx -> xs.[int (wrap 0f (float32 length) (float32 idx))]) [8 .. 12])
        [9;10;1;2;3]
        "indexing above maximum"

let t2 =
    let xs     = [1..10]
    let length = List.length xs
    Test.is
        (List.map (fun idx -> xs.[wrapi 0 length idx]) [-5 .. 5])
        [6;7;8;9;10;1;2;3;4;5;6]
        "negative indexing in array"

    Test.is
        (List.map (fun idx -> xs.[wrapi 0 length idx]) [8 .. 12])
        [9;10;1;2;3]
        "indexing above maximum"

Test.doneTesting ()
