module Test =
    let mutable testsSoFar = 0

    let plan num =
        printfn "1..%d" num

    let doneTesting () =
        plan testsSoFar

    let ok bool name =
        testsSoFar <- testsSoFar + 1
        if bool then printfn "ok %d - %s"     testsSoFar name
                else printfn "not ok %d - %s" testsSoFar name

    let is got expected name =
        testsSoFar <- testsSoFar + 1
        if got = expected then
            printfn "ok %d - %s" testsSoFar name
        else
            printfn "not ok %d - %s" testsSoFar name
            printfn "#   Failed test '%s'" name
            printfn "#          got: %A" got
            printfn "#     expected: %A" expected

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

    let pass name =
        testsSoFar <- testsSoFar + 1
        printfn "ok %d - %s" testsSoFar name

    let fail name =
        testsSoFar <- testsSoFar + 1
        printfn "not ok %d - %s" testsSoFar name
