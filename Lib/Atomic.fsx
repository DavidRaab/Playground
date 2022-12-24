type Atomic<'a> = Atomic of (unit -> 'a ref)

module Atomic =
    let value x =
        let r = ref x
        Atomic(fun () -> r)

    let run (Atomic a) = a ()

    let get (Atomic f) =
        let v = f ()
        v.Value

    let set x a =
        let v = run a
        v.Value <- x

    let map f a =
        let r = run a
        Atomic(fun () ->
            ref (f r.Value)
        )

    let map2 f a b =
        let a = run a
        let b = run b
        Atomic(fun () ->
            ref (f a.Value b.Value)
        )
