

let rec toBinary x =
    if x = 0 then "0"
    else
        let y = x % 2
        (toBinary ((x-y)/2)) + (string y)

[0..20] |> List.map toBinary

let rec toOctal x =
    if x = 0 then "0"
    else
        let y = x % 8
        (toOctal ((x-y)/8)) + (string y)

[0..20] |> List.map toOctal

let rec toBase nbase x =
    if x = 0 then "0"
    else
        let y = x % nbase
        (toBase nbase ((x-y)/nbase)) + (string y)

[0..20] |> List.map (toBase 4)

let rec toBase' nbase x =
    let bse = List.length nbase

    if x = 0 then
        nbase.[0]
    else
        let y = x % bse
        (toBase' nbase ((x-y)/bse)) + (nbase.[y])

[0..20] |> List.map (toBase' ["A";"B";"C";"D"])

let rec toInt nbase x =
    let bse = List.length nbase
    let mut = pown bse (String.length x - 1)

    if x = "" then
        0
    else
        let fc    = string (x.[0])
        let index = List.findIndex (fun e -> e = fc) nbase
        (toInt nbase (x.[1..])) + (index * mut)

let toBin   = toBase' ["0";"1"]
let fromBin = toInt   ["0";"1"]

[0..20]
|> List.map toBin
|> List.map fromBin

let toABCD   = toBase' ["A";"B";"C";"D"]
let fromABCD = toInt   ["A";"B";"C";"D"]

toBinary 20

List.map toABCD [0..20]
|> List.map fromABCD


let toHex   = toBase' ["0";"1";"2";"3";"4";"5";"6";"7";"8";"9";"A";"B";"C";"D";"E";"F"]
let fromHex = toInt   ["0";"1";"2";"3";"4";"5";"6";"7";"8";"9";"A";"B";"C";"D";"E";"F"]

List.map toHex [0..20]
List.map fromHex (List.map toHex [0..20])

let toOct   = toBase' ["0";"1";"2";"3";"4";"5";"6";"7"]
let fromOct = toInt   ["0";"1";"2";"3";"4";"5";"6";"7"]

List.map toOct [0..20]

let binaryStream = Seq.initInfinite toBin
for x in Seq.take 10 binaryStream do
    printfn "%s" x

let addBin x y =
    toBin ((fromBin x) + (fromBin y))

addBin "1000" "1010"



let toB62 =
    toBase' (
        List.map string [0..9]
        @ List.map string ['a' .. 'z']
        @ List.map string ['A' .. 'Z'])

let fromB62 =
    toInt (
        List.map string [0..9]
        @ List.map string ['a' .. 'z']
        @ List.map string ['A' .. 'Z'])

let b62stream = Seq.initInfinite toB62

for x in Seq.take 200 b62stream do
    printfn "%s" x
