#!/usr/bin/env -S dotnet fsi

let col       = Seq.init 8 id
let allBoards = seq {
    for a in col do
    for b in col do
    for c in col do
    for d in col do
    for e in col do
    for f in col do
    for g in col do
    for h in col do
        yield [|a;b;c;d;e;f;g;h|]
}

let isValid board =
    let checkrow = Set.count (Set board) = 8
    let rec checkDiag (low,high) toCheck =
        match toCheck with
        | []    -> true
        | x::xs -> 
            x <> low && x <> high
            && checkDiag (low-1,high+1) xs
            && checkDiag (x-1,x+1) xs

    let board       = Array.toList board
    let (head,tail) = List.head board, List.tail board
    checkrow && checkDiag (head-1,head+1) tail

let boards = Seq.cache (Seq.filter isValid allBoards)

let showBoard board =
    let replicate x str =
        if x <= 0 then "" else String.replicate x str
    let sb = System.Text.StringBuilder ()
    let getRow row = Array.findIndex (fun x -> x = row) board
    for i=0 to 7 do
        let field = getRow i + 1
        ignore <| sb.Append     (replicate (field-1) ". ")
        ignore <| sb.Append     "x "
        ignore <| sb.AppendLine (replicate (8-field) ". ")
    string sb

let mutable i=1
for board in boards do
        printfn "=== %02d ===" i
        i <- i + 1
        printfn "%s" (showBoard board)

printfn "Solutions: %d" (Seq.length boards)