namespace AmIOff.HttpApi

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
module ScheduleItem =

    let private mapToHour n = 
        let hour = n / 100
        let minute = n % 100
        if minute < 0 || minute >= 60 then 
            printfn "Could not create (minute, hour) from %d" n
            None
        else
            Some (hour, minute)

    let private addToDateTime (date : System.DateTime) (hour, minute) = 
        date
            .AddHours(float hour)
            .AddMinutes(float minute)

    let private maybeTime f (scheduleItem : ScheduleItem) = 
        let assignmentDate = scheduleItem.``Date of assignment``
        scheduleItem
        |> f
        |> mapToHour 
        |> Option.map (addToDateTime assignmentDate)

    let internal tryStartTime offset scheduleItem = 
        scheduleItem
        |> maybeTime (fun scheduleItem -> 
            scheduleItem.``Time of assignment Start``)
        |> Option.map (fun date -> date.AddHours (float offset))

    let internal tryEndTime offset scheduleItem = 
        scheduleItem 
        |> tryStartTime offset
        |> Option.bind (fun startTime -> 
            scheduleItem
            |> maybeTime (fun scheduleItem ->
                scheduleItem.``Time of assignment End``)
            |> Option.map (fun endTime -> 
                let endTime = endTime.AddHours (float offset)
                if startTime > endTime then
                    endTime.AddDays(1.)
                else endTime))

    let isBusy (time : System.DateTime) offset (scheduleItem : ScheduleItem) = 
        scheduleItem 
        |> tryStartTime offset
        |> Option.exists (fun startTime ->
            let isAfterStart = startTime <= time
            let isBeforeEnd = 
                scheduleItem
                |> tryEndTime offset
                |> Option.exists (fun endTime -> time < endTime)
            isAfterStart && isBeforeEnd)

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Resident = 

    let tryCreate (name : string) id : Resident option=
        try
            let names = name.Split ','
            {

                Resident.first = names.[1].Trim()
                Resident.last = names.[0].Trim()
                Resident.id = id
            } 
            |> Some
        with
            | exn -> 
                printfn "Could not build resident from (name : %A, id :%A)"
                        name 
                        id
                None

    let toJson (resident : Resident) = 
        let first = resident.first
        let last = resident.last
        sprintf "{\"firstName\":\"%s\",\"lastName\":\"%s\",\"timeFreeUntil\":\"\"}" first last

    let toJsonUntil (resident : Resident) (until : System.DateTime) = 
        let first = resident.first
        let last = resident.last
        let unix = (until - (new System.DateTime(1970,1,1,0,0,0,0, System.DateTimeKind.Local))).TotalSeconds
        sprintf "{\"firstName\":\"%s\",\"lastName\":\"%s\",\"timeFreeUntil\":%d}" first last (int unix)

    let hasParenthesis resident = 
        let hasParens (str : string) = 
            str.Contains("(") || str.Contains(")")
        let first = resident.first
        let last = resident.last
        not (hasParens first || hasParens last)

    let ignoreWithParenthesis = List.filter hasParenthesis

    let isBusy time offset scheduleItem resident =
        ScheduleItem.isBusy time offset scheduleItem && scheduleItem.``Staff name - unique ID`` = resident.id 

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Timesheet =

    let tryMapAmionResponseToCsv (headerLines : int) (rawAmionResp : string) = 
        let csvHeader = "\"Staff name\",\"Staff name - unique ID\",\"Staff name - backup ID\",\"Assignment name (in quotes)\",\"Assignment name (in quotes) - unique ID\",\"Assignment name (in quotes) - backup ID\",\"Date of assignment\",\"Time of assignment Start\",\"Time of assignment End\""
        try 
            let offset =
                (new System.Text.RegularExpressions.Regex("(?<=GMTO=)([^\s]*)"))
                    .Match(rawAmionResp)
                    .Groups
                    .Item(0)
                    .Value
            rawAmionResp.Split '\n'
            |> fun x -> x.[headerLines..]
            |> String.concat "\n"
            |> sprintf "%s\n%s" csvHeader
            |> fun result -> 
                (Timesheet.Parse result, - int offset) // , offset)
            |> Some
        with
            exn -> 
                printfn "Could not map amion string to csv with error: %s [stack trace: %s]" 
                        exn.Message
                        exn.StackTrace
                None

    let toResidents (timesheet : Timesheet) = 
        [ for row in timesheet.Rows -> 
            let name = row.``Staff name``
            let residentId = row.``Staff name - unique ID``
            (name, residentId) ]
        |> List.toSeq
        |> Seq.distinctBy snd
        |> List.ofSeq
        |> List.choose ((<||) Resident.tryCreate)

    let residentIsBusy (resident : Resident) (time : System.DateTime) offset (timesheet : Timesheet) = 
        timesheet.Rows 
        |> Seq.filter (fun scheduleItem -> 
            scheduleItem.``Staff name - unique ID`` = resident.id)
        |> Seq.exists (fun scheduleItem -> 
            Resident.isBusy time offset scheduleItem resident)

    let freeResidents residents time offset timesheet = 
        residents
        |> List.filter (fun (resident : Resident) -> 
            not (residentIsBusy resident time offset timesheet))
        |> List.sortBy (fun resident -> resident.first)

    let residentsFreeUntil resident timeFree offset (timesheet : Timesheet) = 
        let shifts = 
            timesheet.Rows 
            |> Seq.filter (fun scheduleItem -> 
                scheduleItem.``Staff name - unique ID`` = resident.id) //TODO: Make sure timesheet is sorted
        let isBeforeFirst = 
            try 
                timesheet.Rows
                |> Seq.head
                |> ScheduleItem.tryStartTime offset 
                |> Option.exists (fun t -> timeFree < t)
            with
                | _ -> 
                    printfn "Timesheet is empty ?"
                    false
        if isBeforeFirst then
            timesheet.Rows 
            |> Seq.tryHead
        else 
            shifts
            |> Seq.tryFindIndex (fun scheduleItem -> 
                scheduleItem 
                |> ScheduleItem.tryEndTime offset 
                |> Option.exists (fun t -> timeFree > t))
            |> Option.bind (fun i -> Seq.tryItem (i + 1) shifts)
        |> Option.bind (ScheduleItem.tryStartTime offset)
