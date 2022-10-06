#!/usr/bin/env -S dotnet fsi
#r "nuget: Expecto"
open Expecto
open Expecto.Flip

#load "json.fsx"
open Extensions
open Json
open Combinators

module P = Parser

// Helper Functions
let isMatch m rest msg result =
    match result with
    | ParserResult.Matched (r,src)      ->
        Expect.equal msg m r
        Expect.equal msg rest src.Source
    | ParserResult.NotMatched _ ->
        Expect.isTrue msg false

let isNotMatch msg result =
    match result with
    | ParserResult.Matched _            -> Expect.isTrue msg false
    | ParserResult.NotMatched (pos,msg) -> Expect.isTrue msg true



let single = test "Single" {
    Parser.parse Json.jNull "null"
    |> isMatch JsonNull ""  @"Null"

    Parser.parse Json.jNull "nullt"
    |> isMatch JsonNull "t"  @"Null"

    Parser.parse Json.jNull "mull"
    |> isNotMatch  @"Null"

    Parser.parse Json.bool "true"
    |> isMatch (JsonBool true) ""  @"true"

    Parser.parse Json.bool "trues"
    |> isMatch (JsonBool true) "s"  @"true"

    Parser.parse Json.bool "false"
    |> isMatch (JsonBool false) ""  @"false"

    Parser.parse Json.bool "falses"
    |> isMatch (JsonBool false) "s"  @"false"

    Parser.parse Json.escape @"\"""
    |> isMatch (JsonString "\"") ""  @"quote"

    Parser.parse Json.escape @"\\"
    |> isMatch (JsonString @"\") ""  @"backslash"

    Parser.parse Json.escape @"\/"
    |> isMatch (JsonString @"/") ""  @"slash"

    Parser.parse Json.escape @"\b"
    |> isMatch (JsonString "\b") ""  @"backspace"

    Parser.parse Json.escape @"\f"
    |> isMatch (JsonString "\f") ""  @"formfeed"

    Parser.parse Json.escape @"\n"
    |> isMatch (JsonString "\n") ""  @"newline"

    Parser.parse Json.escape @"\r"
    |> isMatch (JsonString "\r") ""  @"carriage return"

    Parser.parse Json.escape @"\t"
    |> isMatch (JsonString "\t") ""  @"tab"

    Parser.parse Json.escape @"\u16a7\u16a4\u16e4"
    |> isMatch (JsonString "ᚧ") @"\u16a4\u16e4"  @"hex1"

    Parser.parse Json.escape @"\u16a4\u16a7\u16e4"
    |> isMatch (JsonString "ᚤ") @"\u16a7\u16e4"  @"hex2"

    Parser.parse Json.escape @"\u16e4\u16a7\u16a4"
    |> isMatch (JsonString "ᛤ") @"\u16a7\u16a4"  @"hex3"
}


let complex = test "Complex" {
    let input  = """{
        "Name":      "Raab",
        "FirstName": "David",
        "Greatness": 9999.99,
        "Likes":     ["Fucking","Eating",42],
        "Lives":     "true"
        "Deads":     null
        "WantWomen": {
            "Name":      "Insane",
            "HairColor": "Brunette",
            "Breasts":   "Big"
        }
    }"""


    let output = JsonObject(Map [
        "Name",      JsonString("Raab")
        "FirstName", JsonString("David")
        "Greatness", JsonFloat(9999.99)
        "Likes",     JsonArray([
            JsonString("Fucking");
            JsonString("Eating");
            JsonFloat(42.0)
        ])
        "Lives",     JsonBool(true)
        "Deads",     JsonNull
        "WantWomen", JsonObject(Map [
            "Name",      JsonString("Insane")
            "HairColor", JsonString("Brunette")
            "Breasts",   JsonString("Big")
        ])
    ])

    Expect.isTrue "joo" true
}

runTestsWithCLIArgs [] Cli.args (testList "main" [
    single
    complex
])