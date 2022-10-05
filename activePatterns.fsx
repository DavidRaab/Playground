#!/usr/bin/env -S dotnet fsi

#load "Lib/Extensions.fs"
open Extensions

type Person = {
    Name: string
    Age:  int
    Sex:  Sex
}
and Sex =
    | Male
    | Female

type Freimaurer =
    Freimaurer of level:Level * person:Person
and Level =
    | Novice
    | Advanced
    | Master

type Error =
    | TooYoung
    | Women

let upgrade (Freimaurer (level,person)) =
    match level with
    | Novice   -> Freimaurer (Advanced,person)
    | Advanced -> Freimaurer (Master,  person)
    | Master   -> Freimaurer (Master,  person)

let (|Integer|_|) str = System.Int32.tryParse  str
let (|Float|_|)   str = System.Double.tryParse str
let (|Float32|_|) str = System.Single.tryParse str

let (|Child|Adult|) person =
    if   person.Age < 18
    then Child
    else Adult

let (|Sex|) person =
    person.Sex

let getLevel (Freimaurer (level,_)) = level

let toFreimaurer person =
    match person with
    | Sex Female -> Error Women
    | Child      -> Error TooYoung
    | p          -> Ok (Freimaurer (Novice, p))

let persons = [
    { Name = "David";  Age = 400; Sex = Male }
    { Name = "Alice";  Age = 17;  Sex = Female }
    { Name = "Vivi";   Age = 21;  Sex = Female }
    { Name = "Markus"; Age = 38;  Sex = Male }
    { Name = "Boris";  Age = 12;  Sex = Male }
]

let frei       = List.map    (fun p -> toFreimaurer p, p) persons
let freimaurer = List.choose (Result.tryOk << fst) frei

frei |> List.iter (fun (frei,person) ->
    match frei with
    | Ok (Freimaurer (level,p)) -> printfn "Welcome %s to the Freimaurer as %A" p.Name level
    | Error Women               -> printfn "We don't like women like %s" person.Name
    | Error TooYoung            -> printfn "Soory, %s you are too young" person.Name
)

for frei,person in frei do
    match frei with
    | Ok (Freimaurer (level,p)) -> printfn "Welcome %s to the Freimaurer as %A" p.Name level
    | Error Women               -> printfn "We don't like women like %s" person.Name
    | Error TooYoung            -> printfn "Soory, %s you are too young" person.Name


freimaurer
|> List.map upgrade
|> List.iter (fun (Freimaurer (level, {Name = name})) ->
    printfn "Welcome %s to the Freimaurer as %A" name level
)