package main

import (
	"fmt"
	"strconv"
)

// A Result type with two constructors
type Result[T any] struct {
    Ok  T
    Err error
}

func Ok[T any] (value T) Result[T] {
    return Result[T]{ Ok:value, Err:nil }
}

func Err[T any](value error) Result[T] {
    var zero T
    return Result[T]{ Ok:zero, Err:value }
}

func wrap[T any](x T, error error) Result[T] {
    if error == nil {
        return Ok(x)
    } else {
        return Err[T](error)
    }
}

func rmap[A any, B any](result Result[A], f func(A)B) Result[B] {
    if result.Err == nil {
        return Result[B]{ Ok:f(result.Ok), Err:nil }
    } else {
        var zero B
        return Result[B]{ Ok:zero, Err:result.Err }
    }
}

func rmap2[A any, B any, C any](res1 Result[A], res2 Result[B], f func(A,B)C) Result[C] {
    if res1.Err == nil && res2.Err == nil {
        return Result[C]{ Ok:f(res1.Ok, res2.Ok), Err:nil }
    } else {
        var zero C
        if ( res1.Err != nil ) {
            return Result[C]{ Ok:zero, Err:res1.Err }
        } else {
            return Result[C]{ Ok:zero, Err:res2.Err }
        }
    }
}

func riter[A any](result Result[A], f func(A)) {
    if result.Err == nil {
        f(result.Ok)
    } else {
        return
    }
}

func rselect[A any](result Result[A], fok func(A), ferr func(error)) {
    if result.Err == nil {
        fok(result.Ok)
    } else {
        ferr(result.Err)
    }
}

func ror[A any](res Result[A], or A) A {
    if ( res.Err == nil ) {
        return res.Ok
    }
    return or
}

func add100(num float64) float64 {
    return num + 100
}

func add(x float64, y float64) float64 {
    return x + y
}

// F#: let pfloat str = Ok (strconv.ParseFloat str 64)
func pf64(str string) Result[float64] {
    return wrap(strconv.ParseFloat(str, 64))
}

func printNumber(num Result[float64]) {
    rselect(num,
        func(num float64) { fmt.Println("Result:", num)          },
        func(err error)   { fmt.Println("One string not number", err) },
    )
}

func main() {
    num1 := rmap(pf64("abc"), add100)
    // C#: x => x + 100
    // F#: (fun x -> x + 100.0)
    // lambdas are annoying in Go because you must specify types and
    // they are not inferred through type-inference
    num2 := rmap(pf64("100"), func(x float64) float64 { return x + 100 })
    num3 := rmap(pf64("50"), add100)

    printNumber(rmap2(num1, num2, add))
    printNumber(rmap2(num2, num3, add))

    added := ror(num1, 0) + ror(num2, 0)
    fmt.Println("Num1 + Num2:", added)
}
