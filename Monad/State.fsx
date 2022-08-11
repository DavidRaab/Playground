#!/usr/bin/env -S dotnet fsi

type GameState = {
    Frame:           int
    Name:            string
    CharacterHealth: int
}

module GameState =
    let create frame name health =
        { Frame = frame; Name = name; CharacterHealth = health }

    let frame           state = state.Frame
    let characterHealth state = state.CharacterHealth
    let show            state = sprintf "Frame %d: Name = %s: Health = %d" state.Frame state.Name state.CharacterHealth


let nextFrame state =
    let newState =
        GameState.create
            (GameState.frame state + 1)
            state.Name
            (GameState.characterHealth state - 1)
    let str = GameState.show newState
    str,newState

let startState = GameState.create 0 "David" 100
let _,s1   = nextFrame startState
let _,s2   = nextFrame s1
let _,s3   = nextFrame s2
let _,s4   = nextFrame s3
let str,s5 = nextFrame s4

printfn "%s" str

type State<'a> = State of (GameState -> 'a * GameState)
module State =
    let wrap x = State(fun state -> x,state)

    let run gs (State f) = f gs
    let map f state =
        State(fun gs ->
            let x,newGameState = run gs state
            f x,newGameState
        )
    let bind f state =
        State(fun gs ->
            let x,ns = run gs state
            run ns (f x)
        )

    let get =
        State(fun state -> state,state)

    let set state =
        State(fun _ -> (),state)

let nextFrame' =
    State(fun gs ->
        let ns  = GameState.create (gs.Frame+1) gs.Name (gs.CharacterHealth-1)
        let str = GameState.show ns
        str,ns
    )

let gBind =
    nextFrame' |> State.bind (fun str1 ->  // let! str1 = nextFrame'
    nextFrame' |> State.bind (fun str2 ->
    nextFrame' |> State.bind (fun str3 ->
    nextFrame' |> State.bind (fun str4 ->
    nextFrame' |> State.bind (fun str5 ->
        State.wrap str5  // return str5
    )))))

// The lines are basically switched compared to a let binding

// nextFrame' |> State.bind (fun str1 ->
// let str1          =       nextFrame'

let str1,gs = State.run startState gBind
printfn "%s, %A" str1 gs

type StateComputation() =
    member this.Bind(m,f) = State.bind f m
    member this.Return(x) = State.wrap x

let state = StateComputation()

let gComp = state {
    let! str1 = nextFrame'
    let! str2 = nextFrame'
    let! str3 = nextFrame'
    let! str4 = nextFrame'
    let! gs   = State.get
    printfn "GS in CE: %s" (GameState.show gs)
    let! str5 = nextFrame'
    do! State.set (GameState.create 40 gs.Name 20)
    return [str1;str2;str3;str4;str5]
}

let glist,gstate = State.run startState gComp
printfn "gComp: %A : %A" glist gstate

let gameLength = State.map List.length gComp
let gl,_ = State.run startState gameLength
printfn "Game Length: %d" gl


let setName newName = state {
    let! gs = State.get
    do! State.set { gs with Name = newName }
}

let doubleFrame = state {
    let! state   = State.get
    let newState = GameState.create (state.Frame+2) state.Name (state.CharacterHealth-2)
    do! State.set newState
    return GameState.show newState
}

let doubleGame = state {
    let! s1 = doubleFrame
    let! s2 = doubleFrame
    do! setName "Vivi"
    let! s3 = doubleFrame
    return s3
}

let ds,dgs = State.run startState doubleGame
printfn "Double: String = %s | State = %A" ds dgs
