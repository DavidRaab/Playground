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

func main() {
    scanner, err := openFile("measurements3.txt")
    // scanner, err := openFile("weather_stations.csv")
    if err != nil {
        fmt.Println(err)
        os.Exit(1)
    }

    cities := make(map[string]*City)
    for scanner.Scan() {
        line := scanner.Text()
        cols := str.Split(line, ";")
        num, err := strconv.ParseFloat(cols[1], 64)
        if err == nil {
            city := cities[cols[0]]
            if city == nil {
                city := City {
                    Min: num,
                    Max: num,
                    Sum: num,
                    Count: 1,
                }
                cities[cols[0]] = &city
            } else {
                city.Min = min(city.Min, num)
                city.Max = max(city.Max, num)
                city.Sum += num
                city.Count++
            }
        }
    }

    fmt.Printf("{ ")
    for city := range maps.Keys(cities) {
        data := cities[city]
        mean := data.Sum / data.Count

        fmt.Printf("%s=%f/%f/%f, ", city, data.Min, mean, data.Max)
    }
    fmt.Printf("\b\b }")
}
