#load "FSExtensions.fsx"
open FSExtensions

let tryParseInt = System.Int32.tryParse

let (|Integer|_|) (str:string) =
    System.Int32.tryParse str

let (|Match|_|) (regex:string) str =
    let m = System.Text.RegularExpressions.Regex(regex).Match(str)
    match m.Success with
    | false -> None
    | true  -> Some [ for g in (Seq.skip 1 m.Groups) -> g.Value ]

let (|Matches|_|) (regex:string) str =
    let ms = System.Text.RegularExpressions.Regex(regex).Matches(str)
    if   ms.Count = 0
    then None
    else Some [ for m in ms -> [ for g in (Seq.skip 1 m.Groups) -> g.Value ]]

let (|IntString|) lol =
    List.map (fun ([Integer x;str]) -> x,str) lol

let (|Default|) noValue opt =
    match opt with
    | None   -> noValue
    | Some x -> x


match "1a2b3c4d5e6f" with
| Matches @"(\d+)(\D+)" (IntString x) -> printfn "%A" x
| Match @"(\d+)" [Integer num]        -> printfn "%d" num
| _                                   -> printfn "Nothing"

let userInput input =
    let input = defaultArg (System.Int32.tryParse input) 0
    printfn "User Input + 10: %d" (input + 10)

userInput "10"
userInput "1"
userInput "12foo"

let printOpt opt =
    printfn "Opt: %A" opt

let incrOpt (Default 0 x) =
    Some (x+1)

printOpt (incrOpt None)
printOpt (incrOpt (Some 1))
printOpt (incrOpt (Some 2))
printOpt (incrOpt (incrOpt (incrOpt (incrOpt (tryParseInt "")))))