#!/usr/bin/env -S dotnet fsi

type ClampImpl() =
    member _.Clamp(value:float32,x:float32,y:float32) =
        System.Math.Clamp(value,x,y)
    member _.Clamp(value:float,x:float,y:float) =
        System.Math.Clamp(value,x,y)

let dispatch = ClampImpl()

let inline clampD (dispatch:^T) (x:^Num) y value =
    (^T : (member Clamp : ^Num * ^Num * ^Num -> ^Num ) (dispatch,value,x,y) )

// let inline clamp x y value =
//     clampD dispatch x y value

printfn "%f" (clampD dispatch 1.0f 3.0f 10f)