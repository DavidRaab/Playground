#!/usr/bin/env -S dotnet fsi
#load "Lib/Extensions.fs"
#r "nuget: Plotly.NET, 2.0.0-preview.6"
open Extensions

open System
open System.Text.RegularExpressions
open Plotly.NET

type StromEntry = {
    Date:    DateTime
    Kwh:     int
    Comment: string
}

module StromEntry =
    let create date kwh comment =
        {Date=date; Kwh=kwh; Comment=comment}

    let date    entry = entry.Date
    let kwh     entry = entry.Kwh
    let comment entry = entry.Comment

    let withKwh     kwh entry = {entry with Kwh=kwh}
    let addKwh      kwh entry = withKwh (entry.Kwh + kwh) entry
    let subtractKwh kwh entry = withKwh (entry.Kwh - kwh) entry

    let daysDelta entry1 entry2 =
        let days = (entry2.Date - entry1.Date).TotalDays
        int days

    let average entry1 entry2 =
        match daysDelta entry1 entry2 with
        | 0    -> 0.0
        | days -> float (entry2.Kwh - entry1.Kwh) / float days

type Strom = {
    StartDate: DateTime
    Entries:   StromEntry list
}

module Strom =
    let create date entries =
        if List.length entries > 0 then
            let mutable before = List.head entries
            if before.Date <= date then
                failwithf "Datum %O muss größer sein als start datum" before.Date
            for entry in List.tail entries do
                if entry.Date <= before.Date then
                    failwithf "Datum %O muss größer sein als vorheriger Eintrag" entry.Date
                if entry.Kwh  <= before.Kwh then
                    failwithf "Kwh %d für Datum %O muss größer sein als vorheriger Eintrag" entry.Kwh entry.Date
                before <- entry

        {StartDate=date; Entries=entries}

    let startDate strom = strom.StartDate
    let entries   strom = strom.Entries

    let start strom =
        StromEntry.create strom.StartDate 0 ""

    let addEntry entry strom =
        let newEntries = List.append strom.Entries [entry]
        create strom.StartDate newEntries

    let before entry strom =
        match List.tryFindIndex (fun x -> x = entry) strom.Entries with
        | None
        | Some 0   -> start strom
        | Some idx -> strom.Entries.[idx-1]

    let daysSinceStart strom entry =
        StromEntry.daysDelta (start strom) entry

    let averageSinceStart strom entry =
        StromEntry.average (start strom) entry

    let averageBefore strom entry =
        StromEntry.average (before entry strom) entry

let main argv =
    // Parsing
    let strom =
        let data = [
            let rget (x:int) (m:Match) =
                m.Groups.[x].Value

            for line in IO.File.ReadLines("strom.txt") do
                let m = Regex.Match(line, @"(?x)\A \s* (\d\d)\.(\d\d)\.(\d\d\d\d) \s+ (\d+) \s* ([^\r\n]*) \Z")
                if m.Success then
                    let dt      = DateTime(int (rget 3 m), int (rget 2 m), int (rget 1 m))
                    let kwh     = rget 4 m |> int
                    let comment = rget 5 m
                    StromEntry.create dt kwh comment
        ]

        let (first,data) = List.head data, List.tail data
        let init_kwh     = StromEntry.kwh first
        let entries      = List.map (StromEntry.subtractKwh init_kwh) data
        Strom.create first.Date entries

    // Prepends a string only if its not empty
    let prepend pre str =
        if   String.IsNullOrWhiteSpace str
        then ""
        else pre + str

    printfn "Insgesamt:"
    for entry in strom.Entries do
        let days    = Strom.daysSinceStart strom entry
        let average = Strom.averageSinceStart strom entry
        let comment = prepend "-- " entry.Comment

        printfn "Datum: %s | Days: %3d | Kwh/Day: %.2f | 365-Total %4d %s"
            (entry.Date.ToString "yyyy-MM-dd")
            days
            average
            (int (average * 365.0))
            comment

    printfn "\nDelta:"
    for entry in strom.Entries do
        let before  = Strom.before entry strom
        let days    = StromEntry.daysDelta before entry
        let average = Strom.averageBefore strom entry
        let change  = average - (Strom.averageBefore strom before)
        let comment = prepend "-- " entry.Comment

        printfn "Datum: %s | Days: %3d | Kwh/Day: %.2f | Change: %+.2f kwh %s"
            (entry.Date.ToString "yyyy-MM-dd")
            days
            average
            change
            comment

    let chart =
        let days      = strom.Entries |> List.map (Strom.daysSinceStart    strom)
        let avgStart  = strom.Entries |> List.map (Strom.averageSinceStart strom)
        let avgbefore = strom.Entries |> List.map (Strom.averageBefore     strom)

        Chart.Combine [
            Chart.Spline(
                days,avgStart,"Total",
                ShowMarkers  = true
            )
            Chart.Spline(
                days,avgbefore,"Delta",
                ShowMarkers  = true
            )
        ]
        |> Chart.withTitle "Stromverbrauch"
        |> Chart.withX_AxisStyle "Days"
        |> Chart.withY_AxisStyle("Kwh/Day", MinMax=(2.0,5.0))
        |> Chart.withSize(1000.0,800.0)
        |> Chart.withMarkerStyle(Symbol = StyleParam.Symbol.Square)

    Chart.Show chart

main Cli.args