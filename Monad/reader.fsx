#!/usr/bin/env -S dotnet fsi

type Comment = {
    Id:       int
    Name:     string
    Date:     System.DateTime
    Message:  string
    Comments: Comment list
}

module Comment =
    let create id name date msg comments =
        { Id = id; Name = name; Date = date; Message = msg; Comments = comments }

    let name comment     = comment.Name
    let date comment     = comment.Date
    let message comment  = comment.Message
    let comments comment = comment.Comments

    let fmtComment indent comment =
        sprintf "%s %s: %s\n"
            (String.replicate indent "+")
            (name comment)
            (message comment)

    let rec showSubComments indent comment =
        String.concat "" [|
            for comment in comment.Comments do
                fmtComment indent comment + (showSubComments (indent+1) comment)
        |]

    let show comment =
        sprintf "%s -- %O:\n%s\n\n%s" (name comment) (date comment) (message comment) (showSubComments 1 comment)

let now = System.DateTime.UtcNow
let comment =
    Comment.create 1 "David" now "Hello, World!" [
        Comment.create 2 "Richard" now "Fuck, Off!" [
            Comment.create 3 "Dude" now "Richard, du Sau!" []
        ]
        Comment.create 4 "Alice" now "I'm fucking someone else" [
            Comment.create 5 "David" now "Have Fun!" []
        ]
        Comment.create 6 "Vivi"  now "Fuck Me!" [
            Comment.create 7 "David" now "Sure!" []
        ]
    ]

type Reader<'In,'a> = Reader of ('In -> 'a)
module Reader =
    let create f             = Reader(f)
    let wrap x               = Reader(fun _ -> x)
    let unwrap (Reader f)    = f
    let run input (Reader f) =
        f input
    let map f reader =
        Reader(fun input ->
            f (run input reader)
        )
    let bind f reader =
        Reader(fun input ->
            run input (f (run input reader))
        )


// In-Memory, just return database entry from in-memory
// in practice could make any side-effect to a database
// Network, and so on
let fetchCommentById =
    Reader(fun id ->
        if id = 1 then Some comment else None
    )

let fetchByDatabase dsn user pw =
    Reader(fun id ->
        if id = 1 then Some comment else None
    )

let commentAsString reader =
    reader
    |> Reader.map (Option.map Comment.show)
    |> Reader.map (Option.defaultValue "Cannot find comment")

printfn "%s" (Reader.run 1 (commentAsString fetchCommentById))
printfn "%s" (Reader.run 1 (commentAsString (fetchByDatabase "dsn" "User" "PW")))
