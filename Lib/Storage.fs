namespace Storage

type Dictionary<'a,'b> = System.Collections.Generic.Dictionary<'a,'b>

type Storage<'a,'b> = {
    KeyToIndex: Dictionary<'a,int>
    IndexToKey: Dictionary<int,'a>
    Data:       ResizeArray<'b>
}

module Storage =
    let create () = {
        KeyToIndex = Dictionary<_,_>()
        IndexToKey = Dictionary<_,_>()
        Data       = ResizeArray<_>()
    }

    let length storage =
        storage.Data.Count

    /// returns 'Value for 'Key
    let get (key:'Key) storage : voption<'Value> =
        match storage.KeyToIndex.TryGetValue(key) with
        | true, idx -> ValueSome (storage.Data.[idx])
        | false, _  -> ValueNone

    /// returns internal index for 'Key. Should not be cached as it can become invalid
    let getIndex (key:'Key) storage : voption<int> =
        match storage.KeyToIndex.TryGetValue(key) with
        | true, idx -> ValueSome idx
        | false, _  -> ValueNone

    /// When passed an index returns the key for it.
    let getKey (index:int) storage =
        match storage.IndexToKey.TryGetValue(index) with
        | true, key -> ValueSome key
        | false, _  -> ValueNone

    /// Overwrites value at position that was returned by `getIndex`.
    let saveAt (index:int) (value:'Value) storage : unit =
        storage.Data.[index] <- value

    /// adds a key,value mapping. Either newly inserting or overwriting existing data
    let add (key:'Key) (value:'Value) storage =
        match getIndex key storage with
        | ValueSome idx -> storage.Data.[idx] <- value
        | ValueNone     ->
            storage.Data.Add(value)
            let last = storage.Data.Count - 1
            storage.KeyToIndex.Add(key,last)
            storage.IndexToKey.Add(last,key)

    let remove (key:'Key) storage : unit =
        match getIndex key storage with
        | ValueSome idx ->
            let lastIdx = storage.Data.Count - 1
            if idx = lastIdx then
                storage.Data.RemoveAt(lastIdx)
                storage.KeyToIndex.Remove(key) |> ignore
                storage.IndexToKey.Remove(idx) |> ignore
            else
                // overwrite key to delete with last element
                storage.Data.[idx] <- storage.Data.[lastIdx]
                // remove last element
                storage.Data.RemoveAt(lastIdx)
                // delete entries for 'Key in hashes
                storage.KeyToIndex.Remove(key) |> ignore
                // update entries for moved element
                let updatedKey = storage.IndexToKey.[lastIdx]
                storage.IndexToKey.Remove(lastIdx) |> ignore
                storage.KeyToIndex.[updatedKey] <- idx
                storage.IndexToKey.[idx] <- updatedKey
        | ValueNone ->
            ()

    let inline iter ([<InlineIfLambda>] f) storage =
        for value in storage.Data do
            f value