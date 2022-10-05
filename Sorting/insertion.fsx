#!/usr/bin/env -S dotnet fsi

let swap idx1 idx2 (array:array<'a>) =
    let tmp = array.[idx1]
    array.[idx1] <- array.[idx2]
    array.[idx2] <- tmp

let swapR (v1:byref<_>) (v2:byref<_>) =
    let tmp = v1
    v1 <- v2
    v2 <- tmp

let sort (array:array<'a>) =
    for idx=1 to array.Length-1 do
        let rec loop i =
            if i>0 then
                if array.[i-1] > array.[i] then
                    // swap (i-1) i array
                    swapR (&array.[i-1]) (&array.[i])
                    loop (i-1)
        loop idx

let data = [|2;12;33;456;12;11;2;54;7;8;0;4;2|]
sort data

printfn "%A" data