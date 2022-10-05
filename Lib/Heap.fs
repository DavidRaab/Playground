namespace Heap

type Heap<'a>(comparer, ?depth) =
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

    /// Amount of elements currently stored in the Heap
    member _.Count    = count

    /// The internal Capacity of this Heap. Must be a power of two.
    member _.Capacity = heap.Length - 1

    /// A string representation of the internal Array. Prints a line for every tree depth.
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

    /// Adds an element to the Heap using the comparer function for determining the minimum element. O(log n)
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
                if comparer c p < 0 then
                    heap.[idx]        <- p
                    heap.[parent idx] <- c
                    loop (parent idx)
        loop count

    /// Adds a sequence to the Heap. Internally it just calls `Add` for every element. sequence(m) * O(log n)
    member this.AddMany xs =
        for x in xs do
            this.Add x

    /// Returns the minimum of the Heap specified by the comparer function without removing it. O(1)
    member _.Peek () =
        heap.[1]

    /// Returns and removes the minimum of the Heap specified by the comparer function. O(log n)
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

                    if comparer l r < 0 && comparer l current < 0 then
                        heap.[idx]      <- l
                        heap.[left idx] <- current
                        loop (left idx)
                    elif comparer r current < 0 then
                        heap.[idx]       <- r
                        heap.[right idx] <- current
                        loop (right idx)
                // if index only has left child
                elif idx*2 <= count then
                    let l = heap.[left idx]
                    if comparer l current < 0 then
                        heap.[idx]      <- l
                        heap.[left idx] <- current
                        loop (left idx)
            loop 1

            // Return minimum
            min

    /// Turn the current Heap into an ordered array removing all items in the Heap
    member this.ToArray() =
        let array = Array.zeroCreate count
        let rec loop idx =
            this.RemoveMin() |> ValueOption.iter (fun x ->
                array.[idx] <- x
                loop (idx+1)
            )
        loop 0
        array

    /// Prints a GraphViz Dot file. Output can be turned into PNG,SVG,PS,PDF,... and so on.
    /// Use `dot input.dot -Tps -o output.ps`
    member _.Dot (show: 'a -> string) =
        let sb = System.Text.StringBuilder()
        let rec loop idx =
            // Escaping not 100% right; but it's okay for the moment
            sb.Append (sprintf "  %d [label=\"%s\"];" idx ((show heap.[idx]).Replace("\"", "\\\""))) |> ignore
            if left idx  <= count then sb.Append (sprintf "%d -> %d;\n" idx (left idx))  |> ignore; loop (left idx)
            if right idx <= count then sb.Append (sprintf "%d -> %d;\n" idx (right idx)) |> ignore; loop (right idx)
        loop 1
        sprintf "digraph TREE {\n%s\n}\n" (sb.ToString())
