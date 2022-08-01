#load "../FSExtensions.fsx"

open System
open FSExtensions

module Parser =
    type Source = {
        Source:   string
        Position: int
    }

    type ParserResult<'a> =
        | Matched    of result:  'a  * source:Source
        | NotMatched of position:int * error: string

    type Parser<'a> = {
        Label: string
        Fn:    Source -> ParserResult<'a>
    }

    module Source =
        let create source position =
            let source = if source = null then "" else source
            { Source=source; Position=position }

        let empty =
            create "" 1

        let position source =
            source.Position

        let withSource str source =
            { source with Source=str }

        let withPosition position source =
            { source with Position=position }

        let one source =
            if source.Source.Length > 0 then
                let  left  = source.Source.[0]
                let  right = source.Source.[1..]
                ValueSome (left, create right (source.Position+1))
            else
                ValueNone

        let take x source =
            let rec loop amount result src =
                if amount < x then
                    match one src with
                    | ValueNone ->
                        result, src
                    | ValueSome (char,src) ->
                        loop (amount+1) (result+string char) src
                else
                    result,src
            loop 0 "" source

    let create label fn =
        {Label=label; Fn=fn}

    let getLabel parser =
        parser.Label

    let setLabel label parser =
        { parser with Label=label }

    let run parser source =
        match parser.Fn source with
        | Matched (result,source) -> Matched (result,source)
        | NotMatched (pos,_) ->
            match Source.one source with
            | ValueNone ->
                NotMatched (pos, (sprintf "Expected: %s Got: Empty String" (getLabel parser)))
            | ValueSome (got,_) ->
                NotMatched (pos, (sprintf "Expected: %s Got: %c" (getLabel parser) got))

    let parse parser string =
        run parser (Source.create string 1)

    let value x =
        create "Value" (fun input ->
            Matched (x,input)
        )

    let map f parser =
        create (getLabel parser) (fun input ->
            match run parser input with
            | NotMatched (pos,label) -> NotMatched (pos,label)
            | Matched (res,src)      -> Matched ((f res), src)
        )

    let bind f parser =
        create (getLabel parser) (fun input ->
            match run parser input with
            | NotMatched (pos,label) -> NotMatched (pos,label)
            | Matched (m,input)      -> run (f m) input
        )

    type ParserBuilder() =
        member _.Bind(p,f)     = bind f p
        member _.Return(x)     = value x
        member _.ReturnFrom(p) = p
        member _.Zero()        = value ()

    let parser = new ParserBuilder ()

    let expect predicate =
        create "Expect" (fun input ->
            let position = Source.position input
            match Source.one input with
            | ValueNone ->
                NotMatched (position, "Empty String")
            | ValueSome (got,nextInput) ->
                if   predicate got
                then Matched    (got,nextInput)
                else NotMatched (position, "Not Expected")
        )

    let char expected =
        expect (fun c -> c = expected)
        |> setLabel (sprintf "Char %c" expected)

    let any =
        create "Any" (fun input ->
            let position = Source.position input
            match Source.one input with
            | ValueNone              -> NotMatched (position, "Empty String")
            | ValueSome (got,source) -> Matched (got,source)
        )

    let orElse p1 p2 =
        let label = sprintf "%s orElse %s" (getLabel p1) (getLabel p2)
        create label (fun input ->
            match run p1 input with
            | Matched (r,s) -> Matched (r,s)
            | NotMatched _  ->
                match run p2 input with
                | Matched (r,s)        -> Matched (r,s)
                | NotMatched (pos,err) -> NotMatched (pos,err)
        )

    let anyOf listOfParsers =
        Seq.reduce orElse listOfParsers
        |> setLabel (sprintf "AnyOf [%s]" (String.Join(",", Seq.map getLabel listOfParsers)))

    let anyChar charList =
        anyOf (Seq.map char charList)

    let apply pf px =
        pf |> bind (fun f ->
        px |> bind (fun x ->
            value (f x)
        ))
        |> setLabel (getLabel px)

    let andThen p1 p2 =
        p1 |> bind (fun x1 ->
        p2 |> bind (fun x2 ->
            value (x1,x2)
        ))
        |> setLabel (sprintf "(%s) andThen (%s)" (getLabel p1) (getLabel p2))

    let map2 f p1 p2 =
        (apply (map f p1) p2)

    let map3 f p1 p2 p3 =
        (apply (apply (map f p1) p2) p3)

    let map4 f p1 p2 p3 p4 =
        (apply (apply (apply (map f p1) p2) p3) p4)

    let digit =
        expect Char.IsDigit
        |> setLabel "Digit"

    let startsWith start parser =
        let startsWith' (start:string) (str:string) =
            str.StartsWith(start)

        map2 startsWith' start parser
        |> setLabel (sprintf "StartsWith (%s)" (getLabel start))

    let cons p pList =
        map2 (fun h t -> h :: t) p pList

    let traverse f ps =
        let folder x xs =
            apply (apply (value (fun h t -> (f h) :: t)) x) xs
        Seq.foldBack folder ps (value [])

    let sequence ps = traverse id ps

    let seq = sequence

    let toStr parser =
        parser |> map (fun (sl:#seq<'a>) ->
            String.Join("",sl)
        )

    let asString xs =
        toStr (sequence xs)

    let str (str:string) =
        toStr (sequence [for x in str -> char x])

    let repeat x p =
        sequence [for i=1 to x do yield p]
        |> setLabel (sprintf "Repeat %d of (%s)" x (getLabel p))

    let toInt_exn parser =
        parser |> map (fun str ->
            match Int32.tryParse str with
            | None   -> failwithf "Cannot convert %s to Int32" str
            | Some x -> x
        )

    let many parser =
        let rec loop input =
            match run parser input with
            | NotMatched _     -> Matched ([], input)
            | Matched (x,rest) ->
                match loop rest with
                | NotMatched _     -> Matched ([x],  rest)
                | Matched (y,rest) -> Matched (x::y, rest)
        create (sprintf "Many (%s)" (getLabel parser)) loop

    let many1 parser =
        parser      |> bind (fun x1 ->
        many parser |> bind (fun rest ->
            value (x1 :: rest)
        ))
        |> setLabel (sprintf "Many1 (%s)" (getLabel parser))

    let word =
        toStr (many1 (expect Char.IsLetter))
        |> setLabel "Word"

    let whitespace =
        expect Char.IsWhiteSpace
        |> setLabel "Whitespace"

    let newline =
        orElse (str "\r\n") (str "\n")
        |> setLabel "Newline"

    let maybe parser =
        orElse (map Some parser) (value None)

    let defaultValue value parser =
        map (fun p -> Option.defaultValue value p ) parser

    let integer =
        asString [
            defaultValue "" (maybe (str "-"))
            toStr (many1 digit)
        ]
        |> setLabel "Integer"

    let ws =
        toStr (many1 whitespace)

    let extract x parser =
        map (List.item x) parser
        |> setLabel (sprintf "Extract %d of (%s)" x (getLabel parser))

    let extractMany xs parser =
        parser |> map (fun ls ->
            [for x in xs -> List.item x ls]
        )
        |> setLabel (
            sprintf "ExtractMany [%s] of (%s)"
                (String.Join(",", xs))
                (getLabel parser)
            )

    let andThenLeft p1 p2 =
        map fst (andThen p1 p2)
        |> setLabel (sprintf "Return (%s) followed by (%s)" (getLabel p1) (getLabel p2))

    let andThenRight p1 p2 =
        map snd (andThen p1 p2)
        |> setLabel (sprintf "(%s) and then return (%s)" (getLabel p1) (getLabel p2))

    let seperatedBy1 sep p = parser {
        let! head = p
        let! tail = many (andThenRight sep p)
        return head :: tail
    }

    let seperatedBy sep p =
        orElse (seperatedBy1 sep p) (value [])

    let between opener closer x =
        andThenRight opener (andThenLeft x closer)

let parser = new Parser.ParserBuilder ()
