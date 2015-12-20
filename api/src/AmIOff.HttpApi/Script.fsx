// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

printfn "OK"

#I @"../../packages/FSharp.Data/lib/net40/"
#r "FSharp.Data.dll"

#load "Request.fs"

open FSharp.Data
open System.Net
open System
open AmIOff.HttpApi

let amionResponse = 
    Request.tryCreate December 2015 "UCSFEM"
    |> Option.map (Async.RunSynchronously << Request.fetchRaw)
    |> Option.bind Timesheet.tryMapAmionResponseToCsv
    |> Option.get 

// "Shalen, Evan (IM)",2823,192,"SFGH-EM 6p-2a (Zone 1)",1079,275,12-19-15,1800,0200


let resident = 
    {
        first = "Evan"
        last = "Shalen"
        id = 2823
    }

let dateTime = System.DateTime(2015, 12, 19, 21, 00, 00)
let dateTime' = System.DateTime(2015, 12, 20, 03, 00, 00) 

Timesheet.residentIsBusy resident dateTime amionResponse
|> printfn "Resident should be busy %A" 

Timesheet.residentIsBusy resident dateTime' amionResponse
|> not
|> printfn "Resident should not be busy: %A"

let residents = Timesheet.toResidents amionResponse;;

let freeResidents = 
    let now = System.DateTime.Now.AddHours(10.)
    Timesheet.freeResidents residents now amionResponse;;

freeResidents
|> List.map Resident.toJson
|> List.iter (printfn "Free People: %s")