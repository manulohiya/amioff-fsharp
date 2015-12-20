// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

printfn "OK"

#load "Request.fs"
#I @"../../packages/FSharp.Data/lib/net40/"
#r "FSharp.Data.dll"

open FSharp.Data
open System.Net
open System
open AmIOff.HttpApi

let aminoResponse = 
    Request.tryCreate December 2015 "UCSFEM"
    |> Option.map (Async.RunSynchronously << Request.fetchRaw)
    |> Option.bind Timesheet.tryMapAmionResponseToCsv

aminoResponse
|> Option.iter (fun aminoResponse -> 
    for row in aminoResponse.Rows do printfn "%A" row.``Assignment name (in quotes)``

)