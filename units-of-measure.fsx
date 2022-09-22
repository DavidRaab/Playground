#!/usr/bin/env -S dotnet fsi

// See: https://fsharpforfunandprofit.com/posts/units-of-measure/
// For more information

[<Measure>] type km
[<Measure>] type m
[<Measure>] type sec
[<Measure>] type h
[<Measure>] type kg
[<Measure>] type inch
[<Measure>] type foot

let distance     = 1.0<m>
let time         = 2.0<sec>
let speed        = distance/time
let acceleration = speed/time
let mass         = 5.0<kg>
let force        = mass * speed/time

let d2 = distance * 2.0
let m2 = distance * distance
let m3 = m2 * distance


//conversion factor
let inchesPerFoot = 12.0<inch/foot>

// test
let distanceInInches = 3.0<foot> * inchesPerFoot  // foot * (inch/foot)

// toMeterPerSecond (kmh:float<km/h>) : float<m/sec>
let toMeterPerSecond (kmh:float<km/h>) =
    kmh * 1000.0<m/km> / 3600.0<sec/h>

printfn "%A" (toMeterPerSecond 3.0<km/h>)

[<Measure>] type rad
[<Measure>] type deg

let deg2rad (degree:float<deg>) =
    degree * System.Math.PI / 180.0 * 1.0<rad/deg>

let rad2deg (radiant:float<rad>) =
    radiant * 180.0 / System.Math.PI * 1.0<deg/rad>

let begindegs = [15.0; 30.0; 45.0; 60.0; 75.0; 360.0]
let rads = List.map deg2rad (List.map (fun x -> x * 1.0<deg>) begindegs)
printfn "Rads: %A" rads

let degs = List.map rad2deg rads
printfn "Degs: %A" degs
