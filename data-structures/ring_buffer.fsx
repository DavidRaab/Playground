#!/usr/bin/env -S dotnet fsi

#load "RingBuffer.fs"
open RingBuffer

let buf = RingBuffer(5)
buf.PushMany [5;10;15;20;25]
printfn "%A" (buf.ToArray ())
buf.PushMany [30]
printfn "%A" (buf.ToArray ())
buf.PushMany [35]
printfn "%A" (buf.ToArray ())
buf.PushMany [40;45;50]
printfn "%A" (buf.ToArray ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "%A" (buf.ToArray())
buf.PushMany [55;60;65]
printfn "%A" (buf.ToArray())
printfn "Pop: %A" (buf.Pop ())
printfn "%A" (buf.ToArray())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
buf.PushMany [70;75;80]
printfn "%A" (buf.ToArray())
printfn "Shift: %A" (buf.Shift ())
printfn "Shift: %A" (buf.Shift ())
printfn "%A" (buf.ToArray())
buf.PushMany [85;90]
printfn "%A" (buf.ToArray())
printfn "Pop: %A" (buf.Pop ())
printfn "Shift: %A" (buf.Shift ())
buf.PushMany [95;100]
printfn "%A" (buf.ToArray())
buf.Unshift 105
printfn "%A" (buf.ToArray())
buf.Unshift 110
printfn "%A" (buf.ToArray())
buf.Unshift 115
printfn "%A" (buf.ToArray())
printfn "%O" buf
buf.UnshiftMany [120;125]
printfn "%A" (buf.ToArray())

let copy = buf.Copy()
copy.PushMany [130;135]

printfn "BUF:  %A" (buf.ToArray())
printfn "BUF:  %O"  buf
printfn "COPY: %A" (copy.ToArray())
printfn "COPY: %O"  copy

printf "FOR BUF:   "
for x in buf do
    printf "%d " x
printfn ""

printf "FOR COPY:  "
for x in copy do
    printf "%d " x
printfn ""

let bufL  = buf.FoldBack (fun x xs -> x :: xs) []
printfn "List Buf:  %A" bufL
let copyL = copy.FoldBack (fun x xs -> x :: xs) []
printfn "List Copy: %A" copyL

for i= -10 to 10 do
    printfn "Buf Get(%d): %A" i (buf.[i])
