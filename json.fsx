#load "Combinators.fsx"
open Combinators
open FSExtensions

type Json =
    | JsonNull
    | JsonBool   of bool
    | JsonString of string
    | JsonFloat  of float
    | JsonArray  of Json list
    | JsonObject of Map<string,Json>

module Json =
    let jNull =
        Parser.str "null" |> Parser.map (fun _ -> JsonNull)
        |> Parser.setLabel "null"

    let bool =
        Parser.orElse (Parser.str "true") (Parser.str "false")
        |> Parser.map (fun x -> if x = "true" then JsonBool(true) else JsonBool(false))
        |> Parser.setLabel "bool"

    let hexDigit = Parser.anyChar (List.concat [
            ['0' .. '9']
            ['A' .. 'F']
            ['a' .. 'f']
        ])

    let escape =
        Parser.andThenRight (Parser.str "\\") (Parser.anyOf [
            Parser.str "\""
            Parser.str "\\"
            Parser.str "/"
            Parser.str "b"
            Parser.str "f"
            Parser.str "n"
            Parser.str "r"
            Parser.str "t"
            (Parser.toStr (Parser.andThenRight (Parser.str "u") (Parser.repeat 4 hexDigit)))
        ])
        |> Parser.map (function
            | "\"" -> JsonString "\""
            | "\\" -> JsonString "\\"
            | "/"  -> JsonString "/"
            | "b"  -> JsonString "\b"
            | "f"  -> JsonString "\f"
            | "n"  -> JsonString "\n"
            | "r"  -> JsonString "\r"
            | "t"  -> JsonString "\t"
            | hex  -> JsonString (string (char (System.Int32.parseHexExn hex)))
        )

    // let JsonString =


