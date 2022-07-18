open System.Text.RegularExpressions

let rx = Regex(@"\w+", RegexOptions.Compiled)

let splitIntoWords str = [|
    let matches = rx.Matches(str)
    for m in matches -> m.Value
|]

// A lot Faster -- 2x - 4x
let splitIntoWords' (str:string) =
    str.Split(' ');

let wordCount words =
    Seq.fold (fun state word ->
        state |> Map.change word (function
            | None   -> Some 1
            | Some x -> Some (x+1)
        )
        // Map.change word (Some << Option.fold (+) 1) count
        // (Option.fold (fun state x -> Some (state.Value + x)) (Some 1))
        // (Option.map (fun x -> x+1) >> Option.orElse (Some 1)) 
    ) Map.empty words
    

let onlyDuplicates wordCount =
    let folder state key value =
        if   value > 1
        then key :: state
        else state
    Map.fold folder [] wordCount

let onlyDuplicates' wordCount =
    wordCount
    |> Map.filter (fun key value -> value > 1)
    |> Map.toList
    |> List.map fst

let inline addCombine combine (key:'Key) (value:'Value) map =
    Map.change key (function
        | None   -> Some value
        | Some x -> Some (combine x value)
    ) map

let getValue def key (dict:System.Collections.Generic.IDictionary<_,_>) =
    let mutable value = Unchecked.defaultof<_>
    if   dict.TryGetValue(key, &value)
    then value
    else def
