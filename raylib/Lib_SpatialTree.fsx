#!/usr/bin/env -S dotnet fsi
open System.Numerics

type Dic<'a,'b> = System.Collections.Generic.Dictionary<'a,'b>

module Dic =
    let getOrInit init key (dic:Dic<'a,'b>) =
        match dic.TryGetValue(key) with
        | true , value -> value
        | false, _     ->
            let value = init ()
            dic.Add(key, value)
            value

    /// Gets key from a Dictionary and assumes it is an array. Creates
    /// array if key was not populated before.
    let getArray key dic =
        getOrInit (fun () -> ResizeArray() ) key dic

    let add key value (dic:Dic<_,_>) =
        match dic.ContainsKey key with
        | true  -> dic.[key] <- value
        | false -> dic.Add(key,value)

    let remove key (dic:Dic<_,_>) =
        dic.Remove(key) |> ignore

    /// Assumes that `key` contains a ResizeArray and pushes a value onto it.
    /// Creates an empty ResizeArray when the key was not populated before.
    let push (key:'Key) (value:'Value) (dic:Dic<_,_>) : unit =
        let ra = getArray key dic
        ra.Add(value)

    let get (key:'Key) (dic:Dic<_,_>) =
        match dic.TryGetValue(key) with
        | false,_ -> ValueNone
        | true, x -> ValueSome x


type STree<'a> = {
    ChunkSize: int
    Chunks:    Dic<struct (int * int), ResizeArray<Vector2 * 'a>>
}

module STree =
    let create size = {
        ChunkSize = size
        Chunks    = Dic<_,_>()
    }

    let length tree =
        let mutable count = 0
        for KeyValue(_,ra) in tree.Chunks do
            count <- count + ra.Count
        count

    /// Calculates the position in the Stree for the Vector2
    let calcPos (vec:Vector2) stree =
        let x = int vec.X / stree.ChunkSize
        let y = int vec.Y / stree.ChunkSize
        struct (x,y)

    /// Adds an object with position to a spatial tree
    let add (position:Vector2) x stree =
        Dic.push (calcPos position stree) (position,x) stree.Chunks

    /// returns a chunk for a position
    let get (position:Vector2) stree =
        Dic.get (calcPos position stree) stree.Chunks


