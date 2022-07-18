module Iterator =
    type Iter<'a>     = Iter     of (unit -> 'a option)
    type Iterator<'a> = Iterator of (unit -> Iter<'a>)

    module Iter =
        let get  (Iterator iter) = iter ()
        let next (Iter iter)     = iter ()

    let unfold f (acc:'State) =
        Iterator(fun () ->
            let mutable acc = Some acc
            Iter(fun () ->
                if Option.isSome acc then
                    let x = acc.Value
                    match f x with
                    | Some (value,nextAcc) ->
                        acc <- Some nextAcc
                        Some value
                    | None ->
                        acc <- None
                        None
                else
                    None
            )
        )

    let fold f (acc:'State) iter =
        let iter = Iter.get iter
        let rec loop acc =
            match Iter.next iter with
            | Some x -> loop (f acc x)
            | None   -> acc
        loop acc

    let iter f iter =
        let iter = Iter.get iter
        let rec loop () =
            match Iter.next iter with
            | Some x -> f x; loop ()
            | None   -> ()
        loop ()

    let range start stop =
        let folder acc =
            if   acc <= stop
            then Some (acc, acc+1)
            else None
        unfold folder start

    let take x iter =
        Iterator(fun () ->
            let         iter   = Iter.get iter
            let mutable amount = 0
            Iter(fun () ->
                if amount < x then
                    amount <- amount + 1
                    Iter.next iter
                else
                    None
            )
        )

    let inline sum iter =
        fold (+) LanguagePrimitives.GenericZero iter

    let map f iter =
        Iterator(fun () ->
            let iter = Iter.get iter
            Iter(fun () ->
                Option.map f (Iter.next iter)
            )
        )

    let rev (xs:Iterator<'a>) =
        Iterator(fun () ->
            let stack = System.Collections.Generic.Stack ()
            iter (stack.Push) xs
            Iter(fun () ->
                let mutable result = Unchecked.defaultof<'a>
                match stack.TryPop(&result) with
                | true  -> Some result
                | false -> None
            )
        )

let r  = Iterator.range 0 100_000_000
let xs = Iterator.take 10 r
let ys = Iterator.take 5 r

xs |> Iterator.iter (fun x ->
    printfn "%d" x
)

Iterator.range 100 2000
|> Iterator.take 10
|> Iterator.map  (fun x      -> x, sqrt (float x))
|> Iterator.iter (fun (x,sq) -> printfn "%d -> %f" x sq)

Iterator.range 100 2000
|> Iterator.rev
|> Iterator.take 10
|> Iterator.sum
|> printfn "Sum: %d"


Iterator.range 100 2000
|> Iterator.rev
|> Iterator.take 10
|> Iterator.iter (printfn "%d")

ys |> Iterator.iter (fun x ->
    printfn "%d" x
)
