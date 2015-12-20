﻿namespace AmIOff.HttpApi

open FSharp.Data
open System.Net

type Month = 
    January | February | March | April 
    | May | June | July | August | September 
    | October | November | December

type Day = int

type Year = int 

type Date = 
    | MonthDayYear of Month * Year * Day
    | MonthYear of Month * Year

type Request = 
    {
        login : string
        date : Date
    }

type Timesheet = FSharp.Data.CsvProvider<"templates/ocs.csv">

type ScheduleItem = FSharp.Data.CsvProvider<"templates/ocs.csv">.Row

type Resident = 
    {
        first : string
        last : string
        id : int
    }

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Month = 

    let toInt = function
        | January -> 1
        | February -> 2
        | March -> 3
        | April -> 4
        | May -> 5 
        | June -> 6
        | July -> 7
        | August -> 8
        | September -> 9
        | October -> 10
        | November -> 11
        | December -> 12

    let ofInt = function
        | 1 -> January
        | 2 -> February
        | 3 -> March
        | 4 -> April
        | 5 -> May
        | 6 -> June
        | 7 -> July
        | 8 -> August
        | 9 -> September
        | 10 -> October
        | 11 -> November
        | 12 -> December
        | _ -> invalidArg "month" "Not a valid month"

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Date = 

    let tryCreate (day : Day) (month : Month) (year : Year) = 
        let month' = Month.toInt month
        try
            System.DateTime (year, month', day)
            |> ignore
            Some <| MonthDayYear (month, day, year)
        with
            | exn -> 
                printfn "Could not create Date: %s" exn.Message 
                None

    let tryCreateMonthYear (month : Month) (year : Year) = 
        let month' = Month.toInt month
        try 
            System.DateTime (year, month', 1)
            |> ignore
            Some <| MonthYear (month, year)
        with
            | exn -> 
                printfn "Could not create Date: %s" exn.Message
                None

    let tryDay : Date -> Day option = function
        | MonthYear _ -> None
        | MonthDayYear (_, day, _) -> Some day

    let month : Date -> Month = function
        | MonthYear (month, _) -> month
        | MonthDayYear (month, _, _) -> month

    let year : Date -> Year =  function
        | MonthYear (_, year)  -> year
        | MonthDayYear (_, year, _)  -> year

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Request = 

    let create login date = {login = login; date = date}

    let tryCreate month year login =
        Date.tryCreateMonthYear month year
        |> Option.map (create login)

    let constructQuery request = 
        let queries = 
            [
                ("Lo", request.login)
                ("Rpt", string 619)
                ("Month", request.date |> Date.month |> Month.toInt |> string)
                ("Year", request.date |> Date.year |> string)
            ]
        match Date.tryDay request.date with
        | Some date -> 
            ("Day", string date) :: queries
        | None -> 
            printfn "Could not find date"
            queries

    let fetchRaw request = 
        let baseUrl = "http://www.amion.com/cgi-bin/ocs"
        let query = constructQuery request
        let logging = 
            fun (http : HttpWebRequest) -> 
                printfn "Requesting data from: %s" http.Address.OriginalString
                http
        Http.AsyncRequestString(baseUrl, query = query, httpMethod = "GET", customizeHttpRequest = logging)

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Resident = 

    let tryCreate (name : string) id =
        try
            let names = name.Split ','
            {

                first = names.[1]
                last = names.[0]
                id = id
            } 
            |> Some
        with
            | exn -> 
                printfn "Could not build resident from (name : %A, id :%A)"
                        name 
                        id
                None

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Timesheet =

    let tryMapAmionResponseToCsv (rawAmionResp : string) = 
        let csvHeader = "\"Staff name\",\"Staff name - unique ID\",\"Staff name - backup ID\",\"Assignment name (in quotes)\",\"Assignment name (in quotes) - unique ID\",\"Assignment name (in quotes) - backup ID\",\"Date of assignment (GMTO=-8 1)\",\"Time of assignment (GMTO=-8 1) Start\",\"Time of assignment (GMTO=-8 1) End\""
        try 
            rawAmionResp.Split '\n'
            |> Array.skip 5
            |> String.concat "\n"
            |> sprintf "%s\n%s" csvHeader
            |> Timesheet.Parse
            |> Some
        with
            exn -> 
                printfn "Could not map amion string to csv with error: %s" exn.Message
                None