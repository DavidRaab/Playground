#!/usr/bin/env dotnet fsi

open System.Text.RegularExpressions

let (|Match|_|) (regex:Regex) str =
    let m = regex.Match(str)
    match m.Success with
    | false -> None
    | true  -> Some [ for g in (Seq.skip 1 m.Groups) -> g.Value ]

let (|Matches|_|) (regex:Regex) str =
    let  ms = regex.Matches(str)
    if   ms.Count = 0
    then None
    else Some [ for m in ms -> [ for g in (Seq.skip 1 m.Groups) -> g.Value ]]

type FastaEntry = {
    Description: string
    Sequence:    string
}

let parseFasta lines =
    let rxHeader   = Regex @"^>(.*)$"
    let rxSequence = Regex @"([A-Z]+)\*?$"
    let rec loop header sequence fastas lines =
        match lines with
        | [] ->
            {Description=header; Sequence=sequence} :: fastas
        | (Match rxHeader [head]) :: lines ->
            if   sequence = ""
            then loop head "" fastas lines
            else loop head "" ({Description=header; Sequence=sequence} :: fastas) lines
        | (Match rxSequence [sequ]) :: lines ->
            loop header (sequence+sequ) fastas lines
        | line :: lines ->
            // Skip unrecognized lines
            loop header sequence fastas lines
    List.rev (loop "" "" [] lines)

let fasta = parseFasta (Seq.toList (System.IO.File.ReadLines "fasta.fasta"))


// Second Solution -- Pure Regex

let fastaRegexStr = 
    @"
    ^>                  # Line Starting with >
      (.*)                 # Capture into $1
    \r?\n               # End-of-Line
    (                   # Capturing in $2
        (?:                   
            ^           # A Line ...
              [A-Z]+       # .. containing A-Z
            \*? \r?\n   # Optional(*) followed by End-of-Line
        )+              # ^ Multiple of those lines
    )
    (?:
        (?: ^ [ \t\v\f]* \r?\n )  # Match an empty (whitespace) line ..
        |                         # or
        \z                        # End-of-String
    )
    "

// Regex for matching one Fasta Block
let fasta' = Regex(fastaRegexStr, RegexOptions.IgnorePatternWhitespace ||| RegexOptions.Multiline)

// Whole file as a string
let content = System.IO.File.ReadAllText "fasta.fasta"

let entries = [
    for m in fasta'.Matches(content) do
        let desc = m.Groups.[1].Value
        // Remove *, \r and \n from string
        let sequ = Regex.Replace(m.Groups.[2].Value, @"\*|\r|\n", "")
        {Description=desc; Sequence=sequ}
]
