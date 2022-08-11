#!/usr/bin/env -S dotnet fsi

(*
    Written while watching "Functional Data - Greg Young"
    YT: https://www.youtube.com/watch?v=S2KLFlM_Z4I
*)

// App Events
type Event =
    | Created
    | CheckedIn
    | CheckedOut
    | Deactivated

// State / Model
type State = { State: Event }
let createState state = { State = state }

let initial_state = createState Deactivated


// Update
let update event model =
    match model.State, event with
    | Deactivated,Created -> createState Created
    | Deactivated,_       -> createState Deactivated
    | Created,    _       -> createState event
    | CheckedIn,  _       -> createState event
    | CheckedOut, _       -> createState event

// View
let view model =
    printfn "%A" model


// Main App

// 1. Deactivated -- pipe style
view (
    initial_state
    |> update Created
    |> update CheckedIn
    |> update CheckedIn
    |> update CheckedOut
    |> update Deactivated
)

// 2. Deactivated -- same as 1. without pipes
view (
    (update Deactivated
        (update CheckedOut
            (update CheckedIn
                (update CheckedIn
                    (update Created initial_state)))))
)

// 3. Deactivated -- no Created so it states deactivated
view (
    initial_state
    |> update CheckedIn
    |> update CheckedOut
)

// 4. CheckedIn -- It was created, now can change
view (
    initial_state
    |> update Created
    |> update CheckedIn
)

// 5. same as 1 --Deactivated
view (
    List.fold
        (fun model event -> update event model)
        initial_state
        [Created;CheckedIn;CheckedIn;CheckedOut;Deactivated]
)

// create a Function from the fold
let applyEvents model events =
    List.fold (fun model event -> update event model) model events

// same as 5 -- Deactivated
view (applyEvents initial_state [Created;CheckedIn;CheckedIn;CheckedOut;Deactivated])

// same as 4 -- CheckedIn
view (applyEvents initial_state [Created;CheckedIn])

// when i switch order of arguments in update
let update_r event model = update model event

// i can use List.fold without lambda
view (List.fold update_r initial_state [Created;CheckedOut])

// List.fold turns a function like (update : 'State      'a  -> 'State)
// into a new function             (update : 'State list<'a> -> 'State)

// so instead of one argument, you can pass a list of arguments.
// The state is automatically passed through all function calls


// Object-thinking
[Created;CheckedOut]
|> List.fold update_r initial_state

// reads like, i have [Created;CheckedOut] and i do something with it.
// What is not the case, and makes no sense to me.

// Functional thinking
List.fold update_r initial_state [Created;CheckedOut]

// reads like, i have function "update_r" and and an initial state
// and then pass it one argument of the list in order to get
// to the end-state


// Snapshot | or Time-Traversal Debugging | Whoooaaaahhh!
view (List.scan update_r initial_state [Created;CheckedOut])

// output from above -- Just keep all in-between states in a list
// [{ State = Deactivated }; { State = Created }; { State = CheckedOut }]

