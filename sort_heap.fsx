#!/usr/bin/env -S dotnet fsi

#load "FSExtensions.fsx"
open FSExtensions

type Heap<'a when 'a : comparison>(?depth) =
    let mutable depth    = defaultArg depth 4
    let mutable maxCount = int (2.0 ** float depth) - 1
    let mutable count    = 0
    // heap is a tree stored as an Array. The size must be power of 2.
    // First element is stored at index 1, not 0.
    let mutable heap     = Array.zeroCreate<'a> (maxCount + 1)

    // Functions that returns the parent, left- or right-child index from the given index
    let parent x = x / 2
    let left   x = 2 * x
    let right  x = 2 * x + 1

    member _.Count    = count
    member _.Capacity = heap.Length - 1

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
        // Allocate/Copy memory if capacity is not enough
        if count+1 > maxCount then
            depth    <- depth + 1
            maxCount <- int (2.0 ** float depth) - 1
            let newHeap = Array.zeroCreate (maxCount + 1)
            Array.blit heap 1 newHeap 1 count
            heap <- newHeap

        // Add element to the end
        heap.[count+1] <- x
        count <- count + 1

        // then restore heap property by moving element up as needed
        let rec loop idx =
            if idx = 1 then () else
                let c = heap.[idx]
                let p = heap.[parent idx]
                if c < p then
                    heap.[idx]        <- p
                    heap.[parent idx] <- c
                    loop (parent idx)
        loop count

    member this.AddMany xs =
        for x in xs do
            this.Add x

    member _.Peek () =
        heap.[1]

    member _.RemoveMin () =
        if count = 0 then
            ValueNone
        else
            // Save Min value we want to remove
            let min = ValueSome heap.[1]

            // swap last value with the top of the tree
            heap.[1]     <- heap.[count]
            heap.[count] <- Unchecked.defaultof<_>
            count        <- count - 1

            // restore Heap-structure by moving the top down the tree as needed
            // by swaping it with the smaller left/right child.
            let rec loop idx =
                let current = heap.[idx]

                // if index has left+right child
                if idx*2+1 <= count then
                    let l = heap.[left idx]
                    let r = heap.[right idx]

                    if l < r && l < current then
                        heap.[idx]      <- l
                        heap.[left idx] <- current
                        loop (left idx)
                    elif r < current then
                        heap.[idx]       <- r
                        heap.[right idx] <- current
                        loop (right idx)
                // if index only has left child
                elif idx*2 <= count then
                    let l = heap.[left idx]
                    if l < current then
                        heap.[idx]      <- l
                        heap.[left idx] <- current
                        loop (left idx)
            loop 1

            // Return minimum
            min

// Generate a shuffled Array
let array = Array.shuffle [|1..100|]
printfn "Array: %A" array

// Add shuffled Array to Heap
let heap = Heap<int>()
heap.AddMany array
printfn "Heap:\n%O\n" heap

// Remove Minimum until heap is empty and add it to a ResizeArray
let sorted = ResizeArray()
let rec loop () =
    heap.RemoveMin() |> ValueOption.iter (fun x ->
        sorted.Add x
        loop ()
    )
loop ()

// Print Sorted Array
printfn "%A" (sorted.ToArray())

