package main

import (
	"bufio"
	"fmt"
	"maps"
	"os"
	"strconv"
	str "strings"
	"sync"
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

func rok[T any](x T, error error, f func(T)) {
    if error == nil {
        f(x)
    } else {
        return
    }
}


// func rmatch[A any, B any](result Result[A], fok func(A)B, ferr func(error)error) {
// }

func openFile(file string) Result[*bufio.Scanner] {
    return rmap(wrap(os.Open(file)), func(fh *os.File)*bufio.Scanner {
        return bufio.NewScanner(fh)
    })
}

type City struct {
    Min   float64
    Max   float64
    Sum   float64
    Count float64
}

type Line struct {
    City string
    Num float64
}

func main() {
    // var scanner *bufio.Scanner = openFile("measurements3.txt")
    // rselect(openFile("measurements3.txt"),
    rselect(openFile("weather_stations.csv"),
        func(scanner *bufio.Scanner) {
            // scanner.Buffer(make([]byte, 4096*10_000), 4096)
            var wg sync.WaitGroup

            // start worker to parse file, that puts result onto lines
            lines := make(chan Line, 10_000)
            wg.Add(1)
            go func() {
                for scanner.Scan() {
                    line := scanner.Text()
                    cols := str.Split(line, ";")
                    riter(wrap(strconv.ParseFloat(cols[1], 64)), func(num float64) {
                        lines <- Line {City:cols[0], Num:num}
                    })
                }

                close(lines)
                wg.Done()
            }()

            // worker that reads from lines
            cities := make(map[string]*City)
            wg.Add(1)
            go func() {
                for line := range lines {
                    city := cities[line.City]
                    if city == nil {
                        city := City {
                            Min: line.Num,
                            Max: line.Num,
                            Sum: line.Num,
                            Count: 1,
                        }
                        cities[line.City] = &city
                    } else {
                        city.Min = min(city.Min, line.Num)
                        city.Max = max(city.Max, line.Num)
                        city.Sum += line.Num
                        city.Count++
                    }
                }
                wg.Done()
            }()

            wg.Wait()
            fmt.Printf("{ ")
            for city := range maps.Keys(cities) {
                data := cities[city]
                mean := data.Sum / data.Count

                fmt.Printf("%s=%f/%f/%f, ", city, data.Min, mean, data.Max)
            }
            fmt.Printf("\b\b }")
        },
        func(err error) {
            fmt.Println("Cannot open file",err)
        },
    )
}
