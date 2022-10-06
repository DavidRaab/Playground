module Test =
    let mutable testsSoFar = 0

    let plan num =
        printfn "1..%d" num

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

    let doneTesting () =
        plan testsSoFar
