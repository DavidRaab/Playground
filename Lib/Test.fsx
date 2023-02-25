module Test =
    let mutable testsSoFar = 0

    /// Defines how many tests should be runned
    let plan num =
        printfn "1..%d" num

    /// If you don't know how many test will be runned, call doneTesting
    /// at the end of all testing
    let doneTesting () =
        plan testsSoFar

    /// Checks if a boolean is true
    let ok bool name =
        testsSoFar <- testsSoFar + 1
        if bool then printfn "ok %d - %s"     testsSoFar name
                else printfn "not ok %d - %s" testsSoFar name

    /// Compares if two values are the same
    let is got expected name =
        testsSoFar <- testsSoFar + 1
        if got = expected then
            printfn "ok %d - %s" testsSoFar name
        else
            printfn "not ok %d - %s" testsSoFar name
            printfn "#   Failed test '%s'" name
            printfn "#          got: %A" got
            printfn "#     expected: %A" expected

    /// Check if a function successfully throws an exception
    let throws f name =
        let mutable throws = false
        try
            f ()
        with
        | _ -> throws <- true

        testsSoFar <- testsSoFar + 1
        if throws
        then printfn "ok %d - %s" testsSoFar name
        else printfn "not ok %d - %s" testsSoFar name

    /// Explicitly pass a test, used in conjunction with `fail`
    let pass name =
        testsSoFar <- testsSoFar + 1
        printfn "ok %d - %s" testsSoFar name

    /// Explicitly fail a test, used in conjunction with `pass`
    let fail name =
        testsSoFar <- testsSoFar + 1
        printfn "not ok %d - %s" testsSoFar name
