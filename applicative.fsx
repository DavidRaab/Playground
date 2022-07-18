#!/usr/bin/env -S dotnet fsi

let ap fs xs =
    List.foldBack2 (fun f x state ->
        f x :: state
    ) fs xs []

let add x y z = x + y + z

let xs = [1..3]
let ys = [10;20;30]
let zs = [100;200;300]

let res1 = (ap (ap (List.map add xs) ys) zs)
printfn "%A" res1 // [111;222;333]

let (<!>) = List.map
let (<*>) = ap

let res2 = add <!> xs <*> ys <*> zs
printfn "%A" res2 // [111;222;333]