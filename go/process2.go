package main

import (
	"bufio"
	"fmt"
	"maps"
	"os"
	"strconv"
	str "strings"
)

func check(e error) {
    if e != nil {
        panic(e)
    }
}

func openFile(file string) (*bufio.Scanner,error) {
    fh, err := os.Open(file)
    if err != nil {
        return nil, err;
    }
    return bufio.NewScanner(fh), nil
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
    scanner, err := openFile("measurements3.txt")
    if err != nil {
        fmt.Println(err)
        os.Exit(1)
    }

    lines := make(chan Line, 10_000)

    go func() {
        for scanner.Scan() {
            line := scanner.Text()
            cols := str.Split(line, ";")
            num, err := strconv.ParseFloat(cols[1], 64)
            if err == nil {
                lines <- Line {City:cols[0], Num:num}
            }
        }
        close(lines)
    }()

    wait   := make(chan int)
    cities := make(map[string]*City)
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
        wait <- 0
    }()

    <- wait
    fmt.Printf("{ ")
    for city := range maps.Keys(cities) {
        data := cities[city]
        mean := data.Sum / data.Count

        fmt.Printf("%s=%f/%f/%f, ", city, data.Min, mean, data.Max)
    }
    fmt.Printf("\b\b }")
}
