#!/usr/bin/env -S dotnet fsi

type Car = {
    Name:  string
    Wheel: decimal
    Motor: decimal
    Body:  decimal
}

module Car =
    let create name wheel motor body =
        { Name = name; Wheel = wheel; Motor = motor; Body = body }
    let price car =
        4m * car.Wheel + car.Motor + car.Body

    let wheel car = car.Wheel
    let motor car = car.Motor
    let body  car = car.Body


module Data =
    let getByName str data =
        List.tryFind (fun car -> str = car.Name) data

let showCar fn name data =
    match fn name data with
    | Some car -> printfn "%s Offer: %.2f" car.Name (Car.price car)
    | None     -> printfn "%s Not found in data" name


let data = [
    Car.create "Trabi" 30m 350m  550m
    Car.create "VW"   100m 500m  850m
    Car.create "BMW"  300m 850m 1250m
    Car.create "Fancy" 30m 500m 1250m
]

// Partial Applied for In-Memory Database
let showCarIM = showCar Data.getByName

showCarIM "Trabi"   data
showCarIM "VW"      data
showCarIM "BMW"     data
showCarIM "Fancy"   data
showCarIM "Porsche" data



// A Generic Reader Monad
type Reader<'data,'a> = Reader of ('data -> option<'a>)
module Reader =
    let run (data:'data) (Reader fn) =
        fn data

    let wrap           x  = Reader (fun _ -> Some x)
    let unwrap (Reader x) = x

    let map f reader =
        Reader(fun data -> Option.map f (run data reader))

    let bind (f : 'a -> Reader<'data,'b>) reader =
        Reader(fun data ->
            run data reader
            |> Option.bind (fun x -> run data (f x))
        )

    let flatten reader =
        bind id reader

    // Reader<'data,('a -> 'b)> -> Reader<'data,'a> -> Reader<'data,'b>
    let apply rf rx : Reader<'data,'b> =
        rf |> bind (fun f ->
        rx |> bind (fun x ->
            wrap (f x)
        ))

    let create f = Reader(fun data -> f data)

// Specific Reader
type CarReader<'a> = CarReader of Reader<list<Car>,'a>
module CarReader =
    let run data (CarReader reader) =
        Reader.run data reader

    let wrap x =
        CarReader(Reader.wrap x)

    let unwrap (CarReader x) = x

    let map f (CarReader cars) =
        CarReader(Reader.map f cars)

    let bind f cars =
        CarReader(Reader.bind (fun cars -> unwrap (f cars)) (unwrap cars))

    let create f =
        CarReader(Reader.create f)



// Transform Data.getByName into Reader-Monad
let getByName name =
    CarReader.create (Data.getByName name)

// Reader Example
let wheel =
    getByName "Trabi"
    |> CarReader.map Car.wheel

match CarReader.run data wheel with
| Some price -> printfn "TrabiWheel %.2f" price
| None       -> printfn "Cannot find Trabi Wheel price"





// same as 'wheel' - just in one line
// let trabiWheel1 = carWheel (Data.getByName "Trabi")
// let trabiWheel2 = Data.getByName "Trabi" |> carWheel |> fmtNum "Trabi Wheel: "

// match trabiWheel2 data with
// | Some str -> printfn "%s" str
// | None     -> printfn "Cannot find Trabi"
