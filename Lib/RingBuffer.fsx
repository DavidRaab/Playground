type RingBuffer<'a>(buffer:'a array, start:int, stop:int, count:int) =
    let buffer        = buffer //Array.zeroCreate<'a> capacity
    let maxIdx        = buffer.Length - 1
    let capacity      = buffer.Length
    let mutable start = start
    let mutable stop  = stop
    let mutable count = count

    /// Creates an empty RingBuffer with given capacity
    new(capacity) =
        if capacity <= 0 then
            invalidArg "capacity" "Capacity must be greater 0."
        RingBuffer(Array.zeroCreate<'a> capacity, 0, 0, 0)

    /// Creates a RingBuffer and uses init for initialization
    new(capacity,init) =
        if capacity <= 0 then
            invalidArg "capacity" "Capacity must be greater 0."

        let init = Seq.toArray init
        if capacity = init.Length then
            RingBuffer(init, 0, 0, capacity)
        elif capacity > init.Length then
            let buffer = Array.zeroCreate capacity
            Array.blit init 0 buffer 0 init.Length
            RingBuffer(buffer, 0, init.Length, init.Length)
        else
            let offset = init.Length - capacity
            let buffer = Array.zeroCreate capacity
            Array.blit init offset buffer 0 capacity
            RingBuffer(buffer, 0, 0, capacity)

    member this.Count    = count
    member this.Capacity = capacity

    /// Add an element to the Ring Buffer. If capacity is reached oldest entry is over-written. O(1)
    member this.Push x =
        if count = capacity then
            buffer.[stop] <- x
            start <- (start + 1) % capacity
            stop  <- (stop  + 1) % capacity
        else
            buffer.[stop] <- x
            stop  <- (stop + 1) % capacity
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

            if start = stop && count > 0 then
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
                let current = ValueSome buffer.[maxIdx]
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
        let idx =
            if idx < 0 then
                let idx = ((abs idx) - 1) % count
                count - (idx + 1)
            else
                idx
        buffer.[(start + (idx % count)) % capacity]

    member this.Set idx x =
        let idx =
            if idx < 0 then
                let idx = ((abs idx) - 1) % count
                count - (idx + 1)
            else
                idx
        buffer.[(start + (idx % count)) % capacity] <- x

    member this.Item
        with get idx   = this.Get idx
        and  set idx x = this.Set idx x

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
        RingBuffer(Array.copy buffer, start, stop, count)

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
        if count = 0 then
            array
        elif start < stop then
            Array.blit buffer start array 0 count
            array
        else
            let countToStop = capacity - start
            Array.blit buffer start array 0            countToStop
            Array.blit buffer 0     array countToStop (count - countToStop)
            array

    override _.ToString () =
        sprintf "RingBuffer(Start=%d;Stop=%d;Count=%d;%A)" start stop count buffer
