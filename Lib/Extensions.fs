namespace Extensions

[<AutoOpen>]
module Ext =
    type System.Int32 with
        static member tryParse (str:string) =
            match System.Int32.TryParse(str) with
            | true, value-> Some value
            | false, _   -> None

        static member tryParseHex (str:string) =
            let mutable value = 0
            let m =
                System.Int32.TryParse(
                    str,
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    &value
                )
            match m with
            | false -> None
            | true  -> Some value

        static member parseHexExn (str:string) =
            let mutable value = 0
            let m =
                System.Int32.TryParse(
                    str,
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    &value
                )
            match m with
            | false -> failwith "Couldn't parse Hex String to number"
            | true  -> value

    type System.Single with
        static member tryParse (str:string) =
            match System.Single.TryParse(str) with
            | true , value -> Some value
            | false, _     -> None

    type System.Double with
        static member tryParse (str:string) =
            match System.Double.TryParse str with
            | true , value -> Some value
            | false, _     -> None

    type System.Collections.Generic.Stack<'a> with
        static member tryPop(stack:System.Collections.Generic.Stack<_>) =
            match stack.TryPop() with
            | true , x -> ValueSome x
            | false, _ -> ValueNone


    module Array =
        let shuffle_random = System.Random()
        let shuffle array =
            if Array.isEmpty array then [||] else
                let array = Array.copy array
                let max = Array.length array
                for i=0 to max-1 do
                    let ni = shuffle_random.Next max
                    let tmp = array.[ni]
                    array.[ni] <- array.[i]
                    array.[i]  <- tmp
                array


    module List =
        let lift = List.map

        let lift2 f (xs:list<'a>) (ys:list<'b>) : list<'c> = [
            for x in xs do
            for y in ys do
                yield f x y
        ]

        let lift3 f (xs:list<'a>) (ys:list<'b>) (zs:list<'c>) : list<'d> = [
            for x in xs do
            for y in ys do
            for z in zs do
                yield f x y z
        ]

        let mapFilter mapper predicate xs =
            List.foldBack (fun x state ->
                let  x = mapper x
                if   predicate x
                then x :: state
                else state
            ) xs []

        let filterMap predicate mapper xs =
            List.foldBack (fun x state ->
                if   predicate x
                then (mapper x) :: state
                else state
            ) xs []


    module Option =
        let ofPredicate predicate x =
            if predicate x then Some x else None

        let toValue opt =
            match opt with
            | None   -> ValueNone
            | Some x -> ValueSome x


    module ValueOption =
        let ofPredicate predicate x =
            if predicate x then ValueSome x else ValueNone

        let toOption opt =
            match opt with
            | ValueNone   -> None
            | ValueSome x -> Some x


    module Async =
        let wrap x = async { return x }

        let bind f x = async {
            let! x = x
            return! f x
        }

        let map f x = async {
            let! x = x
            return f x
        }

        let map2 f x y = async {
            let! x = x
            let! y = y
            return f x y
        }

        let map3 f x y z = async {
            let! x = x
            let! y = y
            let! z = z
            return f x y z
        }


    module Result =
        let isOk result =
            match result with
            | Ok _    -> true
            | Error _ -> false

        let isError result =
            match result with
            | Ok _    -> false
            | Error _ -> true

        let getOk result =
            match result with
            | Ok value -> value
            | Error _  -> failwith "Cannot get Ok because Result is Error"

        let getError result =
            match result with
            | Ok _        -> failwith "Cannot get Error because Result is Ok"
            | Error value -> value

        let tryOk result =
            match result with
            | Ok value -> Some value
            | Error _  -> None

        let tryError result =
            match result with
            | Ok _        -> None
            | Error value -> Some value


    module Cli =
        let args =
            Array.skip 2 (System.Environment.GetCommandLineArgs())