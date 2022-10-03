#!/usr/bin/env -S dotnet fsi

type Heap<'a when 'a : comparison>(?depth) =
    let mutable depth    = defaultArg depth 4
    let mutable maxCount = int (2.0 ** float depth) - 1
    let mutable count    = 0
    let mutable heap     = Array.zeroCreate<'a> (maxCount + 1)

    let parent x = x / 2
    let left   x = 2 * x
    let right  x = 2 * x + 1

    member _.Count     = count
    member _.Allocated = heap.Length - 1

    override _.ToString () =
        let rec show (d:int) =
            if d <= depth then
                let start   = int (2.0 ** (float (d-1)))
                let stop    = int (2.0 ** d) - 1
                let current = sprintf "%A\n" (heap.[start..stop])
                current + (show (d+1))
            else
                ""
        show 1

    member _.Add x =
        if count+1 > maxCount then
            depth    <- depth + 1
            maxCount <- int (2.0 ** float depth) - 1
            let newHeap = Array.zeroCreate (maxCount + 1)
            Array.blit heap 1 newHeap 1 count
            heap <- newHeap

        heap.[count+1] <- x
        count <- count + 1

        // Restore heap-structure
        let rec loop idx =
            if idx = 1 then () else
                let c = heap.[idx]
                let p = heap.[parent idx]
                if c > p then
                    heap.[idx]        <- p
                    heap.[parent idx] <- c
                    loop (parent idx)
        loop count

    member this.AddMany xs =
        for x in xs do
            this.Add x

    member _.Peek () =
        heap.[1]

    member _.Remove () =
        if count = 0 then
            ValueNone
        else
            let max = ValueSome heap.[1]

            heap.[1]     <- heap.[count]
            heap.[count] <- Unchecked.defaultof<_>
            count        <- count - 1

            // restore Heap-structure
            let rec loop idx =
                let current = heap.[idx]

                // if index has left+right child
                if idx*2+1 <= count then
                    let l = heap.[left idx]
                    let r = heap.[right idx]

                    if l > r && l > current then
                        heap.[idx]      <- l
                        heap.[left idx] <- current
                        loop (left idx)
                    elif r > current then
                        heap.[idx]       <- r
                        heap.[right idx] <- current
                        loop (right idx)
                // if index only has left child
                elif idx*2 <= count then
                    let l = heap.[left idx]
                    if l > current then
                        heap.[idx]      <- l
                        heap.[left idx] <- current
                        loop (left idx)
            loop 1
            max



let heap = Heap<int>()
heap.AddMany [10]
heap.AddMany [20;5;3]
heap.AddMany [12;16;9;100]
heap.AddMany [33;42;11;4;1;88;66]
printfn "%O" heap

for i=0 to 20 do
    printfn "%O" heap
    printfn "Max: %A" (heap.Remove ())

