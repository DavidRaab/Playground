#!/usr/bin/env -S dotnet fsi
#r "nuget: Expecto"
#load "Combinators.fsx"
#load "../FSExtensions.fsx"
open FSExtensions
open Combinators
open Expecto
open Expecto.Flip

module P = Parser

let matched m rest result =
    match result with
    | Parser.Matched (r,src)      -> r = m && src.Source = rest
    | Parser.NotMatched (pos,err) -> false

let isMatch m rest msg result =
    matched m rest result
    |> Expect.isTrue msg

let isNotMatch msg result =
    match result with
    | P.Matched _            -> Expect.isTrue msg false
    | P.NotMatched (pos,msg) -> Expect.isTrue msg true

let testPchar = test "pchar" {
    let parseA = P.char 'a'
    let parseB = P.char 'b'

    Parser.parse parseA "abc"
    |> isMatch 'a' "bc"  @"parseA"

    Parser.parse parseB "bac"
    |> isMatch 'b' "ac"  @"parseB"

    Parser.parse parseA "bac"
    |> isNotMatch "parseA fails"

    Parser.parse parseB "abc"
    |> isNotMatch "parseB fails"
}

let testOrElse = test "orElse|Anyof|anyChar" {
    let parseA    = Parser.char 'a'
    let parseB    = Parser.char 'b'
    let parseC    = Parser.char 'c'
    let pAorB     = Parser.orElse parseA parseB
    let AorBorC   = Parser.orElse pAorB parseC
    let anyABC    = Parser.anyOf [parseA; parseB; parseC]
    let anyFGH1   = Parser.anyChar ['f';'g';'h']
    let anyFGH2   = Parser.anyChar "fgh"
    let FGHthenBC = Parser.andThen anyFGH2 (Parser.orElse parseB parseC)

    Parser.parse parseA ""
    |> isNotMatch "Error against empty string"

    Parser.parse pAorB "abc"
    |> isMatch 'a' "bc"  @"AorB against abc"

    Parser.parse pAorB "bac"
    |> isMatch 'b' "ac"  @"AorB against bac"

    Parser.parse pAorB "cab"
    |> isNotMatch "Not A or B"

    Parser.parse AorBorC "abc"
    |> isMatch 'a' "bc"  @"A from ABC"

    Parser.parse AorBorC "bac"
    |> isMatch 'b' "ac"  @"B from ABC"

    Parser.parse AorBorC "cab"
    |> isMatch 'c' "ab"  @"C from ABC"

    Parser.parse anyABC "abc"
    |> isMatch 'a' "bc"  @"A from ABC"

    Parser.parse anyABC "bac"
    |> isMatch 'b' "ac"  @"B from ABC"

    Parser.parse anyABC "cab"
    |> isMatch 'c' "ab"  @"C from ABC"

    Parser.parse anyFGH1 "fuck"
    |> isMatch 'f' "uck"  @"[FGH]"

    Parser.parse anyFGH1 "guck"
    |> isMatch 'g' "uck"  @"[FGH]"

    Parser.parse anyFGH1 "huck"
    |> isMatch 'h' "uck"  @"[FGH]"

    Parser.parse anyFGH2 "fuck"
    |> isMatch 'f' "uck"  @"[FGH]"

    Parser.parse anyFGH2 "guck"
    |> isMatch 'g' "uck"  @"[FGH]"

    Parser.parse anyFGH2 "huck"
    |> isMatch 'h' "uck"  @"[FGH]"

    Parser.parse FGHthenBC "hca"
    |> isMatch ('h','c') "a"  @"[fgh][bc]"

    Parser.parse FGHthenBC "fca"
    |> isMatch ('f','c') "a"  @"[fgh][bc]"

    Parser.parse FGHthenBC "hba"
    |> isMatch ('h','b') "a"  @"[fgh][bc]"

    Parser.parse FGHthenBC "gba"
    |> isMatch ('g','b') "a"  @"[fgh][bc]"

    Parser.parse FGHthenBC "hda"
    |> isNotMatch "[fgh][bc]"
}

let testAndThen = test "andThen" {
    let parseA  = Parser.char 'a'
    let parseB  = Parser.char 'b'
    let parseC  = Parser.char 'c'

    let ab = Parser.andThen parseA parseB
    let ac = Parser.andThen parseA parseC

    let aThenBorC = (P.andThen parseA (P.orElse parseB parseC))

    Parser.parse ab "abc"
    |> isMatch ('a','b') "c"  @"AB"

    Parser.parse ac "acb"
    |> isMatch ('a','c') "b"  @"AB"

    Parser.parse ab "acb"
    |> isNotMatch "AB on ACB"

    Parser.parse aThenBorC "acd"
    |> isMatch ('a','c') "d"  @"A[BC]"
}

let testMany = test "Sequence|Repeat|Many" {
    let pABC    = P.toStr (P.sequence [P.char 'a'; P.char 'b'; P.char 'c'])
    let pABC2   = P.str "abc"
    let num     = P.toInt_exn (P.toStr (P.sequence [P.digit; P.digit; P.digit]))
    let num2    = P.toInt_exn (P.toStr (P.repeat 3 P.digit))
    let num3    = P.toInt_exn (P.asString [P.digit; P.digit; P.digit])
    let number' = P.toInt_exn (P.toStr (P.many1 P.digit))
    let word'   = P.toStr (P.many1 (P.anyChar ['a' .. 'z']))

    Parser.parse pABC "abcdefgh"
    |> isMatch "abc" "defgh"  @"abc as string"

    Parser.parse pABC2 "abcdefgh"
    |> isMatch "abc" "defgh"  @"abc as string"

    Parser.parse pABC "abcdefgh"
    |> Expect.equal "pABC = pABC2" (P.parse pABC2 "abcdefgh")

    Parser.parse num "123abcde"
    |> isMatch 123 "abcde"  @"Extract 123 num"

    Parser.parse num2 "123abcde"
    |> isMatch 123 "abcde"  @"Extract 123 num2"

    Parser.parse num "123abcde"
    |> Expect.equal "num = num2" (P.parse num2 "123abcde")

    Parser.parse num2 "123abcde"
    |> Expect.equal "num2 = num3" (P.parse num3 "123abcde")

    Parser.parse P.integer "1234abcde"
    |> isMatch "1234" "abcde"  @"number"

    Parser.parse P.integer "hallo123"
    |> isNotMatch "number on hallo123"

    Parser.parse (P.sequence [P.word; P.integer]) "hallo123abc"
    |> isMatch ["hallo";"123"] "abc"  @"word and number"

    Parser.parse P.word "hallo123"
    |> isMatch "hallo" "123"  @"word"

    Parser.parse word' "Hallo123"
    |> isNotMatch "Hallo"

    Parser.parse P.word "Hallo123"
    |> isMatch "Hallo" "123"  @"word"
}

let testMany2 = test "Many2" {
    let pAB = P.many (P.str "AB")

    Parser.parse pAB "ABABABC"
    |> isMatch ["AB";"AB";"AB"] "C"  @"many AB"

    Parser.parse (P.toStr pAB) "ABABABC"
    |> isMatch "ABABAB" "C"  @"many AB Str"

    let wordNumber =
        P.sequence [
            P.toStr (P.many1 P.whitespace)
            P.word
            P.toStr (P.many1 P.whitespace)
            P.integer
        ]
    Parser.parse wordNumber " Hallo 123 ABC"
    |> isMatch [" ";"Hallo";" ";"123"] " ABC"  @"WS Word number"
}

let lineCounting = test "Line/Column Counting" {
    let abcd  = Parser.str "abcd"
    let mabcd = Parser.many (Parser.sequence [abcd;Parser.newline])

    let got = Parser.parse mabcd "abcd\nabcd\r\nabcd\nfoo"
    let expected =
        Parser.Matched (
            [["abcd";"\n"];["abcd";"\r\n"];["abcd";"\n"]],
            Parser.Source.create "foo" 17
        )

    Expect.equal "3 Matches" expected got
}

let testMaybe = test "Maybe" {
    P.parse P.integer "1234"
    |> isMatch "1234" ""  @"num 1234"

    P.parse P.integer "-1234"
    |> isMatch "-1234" ""  @"num -1234"

    P.parse (P.maybe P.integer) "hallo"
    |> isMatch None "hallo"  @"maybe int"

    P.parse (P.maybe P.digit) "123hallo"
    |> isMatch (Some '1') "23hallo"  @"maybe digit"

    let line1 =
        P.seq [
            P.seq [
                P.asString [
                    P.digit
                    P.digit
                ]
                P.str "."
                P.integer
                P.str "."
                P.asString [
                    P.digit
                    P.digit
                    P.digit
                    P.digit
                ]
                P.ws
            ]
            P.seq [ P.integer; P.ws ]
            P.seq [ P.toStr (P.many P.any)]
        ]

    Parser.parse line1 "21.05.2021 1234 Hallo"
    |> isMatch [["21";".";"05";".";"2021";" "];["1234";" "];["Hallo"]] ""  @"Complex Line"

    Parser.parse Parser.any ""
    |> isNotMatch "any on Empty String"

    let line2 =
        P.seq [
            P.extractMany [0;2;4] (P.seq [
                P.asString [ P.digit; P.digit ]
                P.str "."
                P.integer
                P.str "."
                P.toStr (P.repeat 4 P.digit)
                P.ws
            ])
            P.extractMany [0] (P.seq [ P.integer; P.ws ])
            P.seq [ P.toStr (P.many P.any)]
        ]

    Parser.parse line2 "21.05.2021 1234 Hallo"
    |> isMatch [["21";"05";"2021"];["1234"];["Hallo"]] ""  @"Complex Line 2"
}

let testAndThen2 = test "AndThen2" {
    let parens      = P.anyChar ['('; ')']
    let surrounded1 = P.andThenRight parens (P.andThenLeft P.word parens)
    let surrounded2 = P.andThenLeft (P.andThenRight parens P.word) parens

    Parser.parse surrounded1 "(hallo)"
    |> isMatch "hallo" ""  @"hallo in parens"

    let m1 = Parser.parse surrounded1 "(hallo)"
    let m2 = Parser.parse surrounded2 "(hallo)"

    Expect.equal "m1 = m2" m1 m2
}

let testBind = test "bind" {
    let line = parser {
        let! day   = P.repeat 2 P.digit |> P.toStr
        let! _     = P.char '.'
        let! month = P.repeat 2 P.digit |> P.toStr
        let! _     = P.char '.'
        let! year  = P.repeat 4 P.digit |> P.toStr
        let! _     = P.ws

        let! num   = P.toInt_exn P.integer
        let! _     = P.ws

        let! rest  = P.many1 P.any |> P.toStr

        let date = sprintf "%s-%s-%s" year month day
        return (date,num,rest)
    }

    Parser.parse line "21.05.2021 1234 Hallo"
    |> isMatch ("2021-05-21",1234,"Hallo") ""  @"Complex Line 2"
}

let testSep = test "Seperator" {
    let digits =
        P.seperatedBy1 (P.char ',') P.digit
        |> P.map (fun xs -> xs |> List.map (fun x -> System.Int32.Parse(string x)))

    let digits0 =
        P.seperatedBy (P.char ',') P.digit
        |> P.map (fun xs -> xs |> List.map (fun x -> System.Int32.Parse(string x)))

    Parser.parse digits "a"
    |> isNotMatch  @"Not a digit"

    Parser.parse digits0 "asd"
    |> isMatch [] "asd"  @"No digit Found"

    Parser.parse digits "1"
    |> isMatch [1] ""  @"1"

    Parser.parse digits "1,2,3,4"
    |> isMatch [1;2;3;4] ""  @"1,2,3,4"

    Parser.parse digits "1,2,3,4a"
    |> isMatch [1;2;3;4] "a"  @"1,2,3,4 with a"

    Expect.equal "digit = digit0"
        (Parser.parse digits  "1,2,3,4a")
        (Parser.parse digits0 "1,2,3,4a")

    let pList = P.between (P.char '[') (P.char ']') (P.seperatedBy (P.char ',') P.integer)
    Parser.parse pList "[1,2,3,4]"
    |> isMatch ["1";"2";"3";"4"] ""  @"Parse List"

}

let testErrorMessages = test "ErrorMessages" {
    Parser.parse (P.char 'a') "bac"
    |> Expect.equal "" (P.NotMatched (1, "Expected: Char a Got: b"))

    Parser.parse P.any ""
    |> Expect.equal "" (P.NotMatched (1, "Expected: Any Got: Empty String"))

    Parser.parse P.digit "HAB"
    |> Expect.equal "" (P.NotMatched (1, "Expected: Digit Got: H"))

    Parser.parse P.word "123"
    |> Expect.equal "" (P.NotMatched (1, "Expected: Word Got: 1"))

    Parser.parse P.whitespace "foo"
    |> Expect.equal "" (P.NotMatched (1, "Expected: Whitespace Got: f"))

    Parser.parse P.newline "klo"
    |> Expect.equal "" (P.NotMatched (1, "Expected: Newline Got: k"))

    Parser.parse P.integer "abc1234"
    |> Expect.equal "" (P.NotMatched (1, "Expected: Integer Got: a"))

    Parser.parse P.ws "troja"
    |> Expect.equal "" (P.NotMatched (1, "Expected: Many1 (Whitespace) Got: t"))
}

runTestsWithCLIArgs [] Cli.args (testList "main" [
    testPchar
    testOrElse
    testAndThen
    testMany
    testMany2
    lineCounting
    testMaybe
    testAndThen2
    testBind
    testSep
    testErrorMessages
])
