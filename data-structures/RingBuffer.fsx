#!/usr/bin/env -S dotnet fsi

type RingBuffer<'a>(capacity) =
    let buffer        = Array.zeroCreate<'a> capacity
    let maxIdx        = buffer.Length - 1
    let mutable start = 0
    let mutable stop  = 0
    let mutable count = 0

    member this.Count    = count
    member this.Capacity = capacity

    /// Add an element to the Ring Buffer. If capacity is reached oldest entry is over-written. O(1)
    member this.Push x =
        if count = buffer.Length then
            buffer.[stop] <- x
            start <- (start + 1) % buffer.Length
            stop  <- (stop  + 1) % buffer.Length
        else
            buffer.[stop] <- x
            stop <- (stop + 1) % buffer.Length
            count <- count + 1

    /// Removes Last element of the Ring Buffer. O(1)
    member this.Pop () =
        match count with
        | 0 -> ValueNone
        | _ ->
            let indexToRemove = if stop = 0 then maxIdx else stop - 1
            let value         = ValueSome buffer.[indexToRemove]

            buffer.[indexToRemove] <- Unchecked.defaultof<_>
            stop                   <- indexToRemove
            count                  <- count - 1

            value

    /// Add Element to the Start of the Ring Buffer. If capacity is reached over-writes last value added (newest). O(1)
    member this.Unshift x =
            let indexToAdd = if start = 0 then maxIdx else start - 1
            buffer.[indexToAdd] <- x

            if start = stop then
                start <- indexToAdd
                stop  <- indexToAdd
            else
                start <- indexToAdd
                count <- count + 1

    /// Removes First element of the Ring Buffer. O(1)
    member this.Shift () =
        match count with
        | 0 -> ValueNone
        | _ ->
            if start = maxIdx then
                let current = ValueSome buffer.[0]
                buffer.[maxIdx] <- Unchecked.defaultof<_>
                start           <- 0
                count           <- count - 1
                current
            else
                let current = ValueSome buffer.[start]
                buffer.[start] <- Unchecked.defaultof<_>
                start          <- start + 1
                count          <- count - 1
                current

    member this.PushMany xs =
        for x in xs do
            this.Push x

    member this.UnshiftMany xs =
        for x in xs do
            this.Unshift x

    member this.Get idx =
        if idx < 0 then
            buffer.[(start + buffer.Length + (idx % buffer.Length)) % buffer.Length]
        else
            buffer.[(start + idx) % buffer.Length]

    member this.Item idx =
        this.Get idx

    member this.Foldi f (state:'State) =
        let rec loop state countSoFar idx =
            if countSoFar = count then
                state
            elif idx > maxIdx then
                loop state countSoFar 0
            else
                loop (f state buffer.[idx] countSoFar) (countSoFar+1) (idx+1)
        loop state 0 start

    member this.Fold f (state:'State) =
        let rec loop state countSoFar idx =
            if countSoFar = count then
                state
            elif idx > maxIdx then
                loop state countSoFar 0
            else
                loop (f state buffer.[idx]) (countSoFar+1) (idx+1)
        loop state 0 start

    member this.FoldBack f (state:'State) =
        let rec loop state countSoFar idx =
            if countSoFar = count then
                state
            elif idx < 0 then
                loop state countSoFar maxIdx
            else
                loop (f buffer.[idx] state) (countSoFar+1) (idx-1)
        loop state 0 (stop-1)

    member this.Iteri f =
        this.Foldi (fun () x i -> f i x) ()

    /// Creates a shallow copy of the Ring Buffer. O(N)
    member this.Copy () =
        RingBuffer(capacity) |> this.Fold (fun buf x ->
            buf.Push x; buf
        )

    member private this.getEnumerator () =
        let mutable countSoFar = 0
        let mutable idx        = start
        let mutable current    = Unchecked.defaultof<_>
        { new System.Collections.Generic.IEnumerator<'a> with
            member _.Current with get () :   'a = current
            member _.Current with get () :  obj = box current
            member _.MoveNext ()         : bool =
                if countSoFar = count then
                    current <- Unchecked.defaultof<_>
                    false
                else
                    if idx > maxIdx then idx <- 0
                    current    <- buffer.[idx]
                    idx        <- idx + 1
                    countSoFar <- countSoFar + 1
                    true
            member _.Reset() =
                countSoFar <- 0
                idx        <- start
                current    <- Unchecked.defaultof<_>
            member _.Dispose () = ()
        }

    interface System.Collections.Generic.IEnumerable<'a> with
        override this.GetEnumerator(): System.Collections.Generic.IEnumerator<'a> =
            this.getEnumerator ()

        override this.GetEnumerator(): System.Collections.IEnumerator =
            this.getEnumerator () :> System.Collections.IEnumerator

    member this.ToArray () =
        let array = Array.zeroCreate count
        if count > 0 && start < stop then
            Array.blit buffer start array 0 count
        else
            let countToStop = buffer.Length - start
            Array.blit buffer start array 0            countToStop
            Array.blit buffer 0     array countToStop (count - countToStop)
        array

    override _.ToString () =
        sprintf "RingBuffer(Start=%d;Stop=%d;Count=%d;%A)" start stop count buffer


let buf = RingBuffer(5)
buf.PushMany [5;10;15;20;25]
printfn "%A" (buf.ToArray ())
buf.PushMany [30]
printfn "%A" (buf.ToArray ())
buf.PushMany [35]
printfn "%A" (buf.ToArray ())
buf.PushMany [40;45;50]
printfn "%A" (buf.ToArray ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "%A" (buf.ToArray())
buf.PushMany [55;60;65]
printfn "%A" (buf.ToArray())
printfn "Pop: %A" (buf.Pop ())
printfn "%A" (buf.ToArray())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
printfn "Pop: %A" (buf.Pop ())
buf.PushMany [70;75;80]
printfn "%A" (buf.ToArray())
printfn "Shift: %A" (buf.Shift ())
printfn "Shift: %A" (buf.Shift ())
printfn "%A" (buf.ToArray())
buf.PushMany [85;90]
printfn "%A" (buf.ToArray())
printfn "Pop: %A" (buf.Pop ())
printfn "Shift: %A" (buf.Shift ())
buf.PushMany [95;100]
printfn "%A" (buf.ToArray())
buf.Unshift 105
printfn "%A" (buf.ToArray())
buf.Unshift 110
printfn "%A" (buf.ToArray())
buf.Unshift 115
printfn "%A" (buf.ToArray())
printfn "%O" buf
buf.UnshiftMany [120;125]
printfn "%A" (buf.ToArray())

let copy = buf.Copy()
copy.PushMany [130;135]

printfn "BUF:  %A" (buf.ToArray())
printfn "BUF:  %O"  buf
printfn "COPY: %A" (copy.ToArray())
printfn "COPY: %O"  copy

printf "FOR BUF:   "
for x in buf do
    printf "%d " x
printfn ""

printf "FOR COPY:  "
for x in copy do
    printf "%d " x
printfn ""

let bufL  = buf.FoldBack (fun x xs -> x :: xs) []
printfn "List Buf:  %A" bufL
let copyL = copy.FoldBack (fun x xs -> x :: xs) []
printfn "List Copy: %A" copyL

for i= -10 to 10 do
    printfn "Buf Get(%d): %A" i (buf.[i])
