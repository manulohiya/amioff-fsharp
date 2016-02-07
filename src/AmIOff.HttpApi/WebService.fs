namespace AmIOff.HttpApi

open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open Suave.Json
open Suave.Types
open Suave.Http.Files
open System.Collections.Generic
open Newtonsoft.Json 
open System

type StaffObject = {id : int; firstName : string; lastName : string; staffType : string}
type HangResponseObject = {staffObject : StaffObject; grouping : string; freeUntil : int64}
type HangResponse = {programName : string; inputTime : int64; staff : List<HangResponseObject>}
type SwapResponseObject = {staffObject : StaffObject}
type SwapResponse = {programName : string; start : int64; endTime : int64; swapWindow : int64; staff : List<SwapResponseObject>} 

module Service = 

    let logger = log4net.LogManager.GetLogger("AmIOff.HttpApi.Service")

    let private ofUnixTime (unix : int) = 
        let dtDateTime = new System.DateTime(1970,1,1,0,0,0,0, System.DateTimeKind.Local)
        dtDateTime.AddSeconds(float unix)

    let private findFreeResidentsAsJson (login, time) (x : HttpContext) = 
        let time = ofUnixTime time
        async {
            let maybeRequest = Request.tryCreate (Month.ofInt time.Month) time.Year login
            match maybeRequest with
            | None -> return! Suave.Http.RequestErrors.BAD_REQUEST "Invalid login or date" x
            | Some request -> 
                let! raw = Request.fetchRaw request
                match Timesheet.tryMapAmionResponseToCsv 5 raw with // 5 is the size of header from api
                | Some (timesheet, offset) -> 
                    let residents = Timesheet.toResidents timesheet |> Resident.ignoreWithParenthesis
                    let time = if time.Month <= 6 then time.AddYears 1 else time //HACK: API expects academic year, so we have to convert back to calendar year for processing timesheet
                    let freeResidents = 
                        Timesheet.freeResidents residents time offset timesheet
                        |> List.map (fun resident -> 
                            let freeUntil = Timesheet.residentsFreeUntil resident time offset timesheet
                            (resident, freeUntil))
                    let freeResidentsAsJSON = 
                        let joined = 
                            freeResidents 
                            |> List.map (fun (resident, freeUntil) -> 
                                Resident.toJsonUntil resident freeUntil)
                            |> String.concat ","
                        sprintf "[%s]" joined
                    logger.Debug <| sprintf "Returning JSON: %s" freeResidentsAsJSON
                    return! OK freeResidentsAsJSON x
                | None -> return! Suave.Http.RequestErrors.BAD_REQUEST "Invalid login or date" x
        }                

    let (^^) (r : HttpRequest) (key : string) = 
        match r.queryParam key with
        | Choice1Of2 x -> Some x
        | _ -> 
            logger.Warn <| sprintf "Could not find query param %s" key
            None 

    let (^^!) r k = Option.get (r ^^ k)

    let hangStub (r : HttpRequest) = 
        logger.Info <| sprintf "Hang Query: %s" r.rawQuery
        let staffType = r ^^ "staffType" |> Option.fold (fun _ s -> s) "UCSFEM-R1"
        let grouping = r ^^ "grouping" |> Option.fold (fun _ s -> s) "Vacation"
        let delphine = {id = 1; firstName = "Delphine"; lastName = "Huang"; staffType =  staffType}
        let o = {staffObject = delphine; grouping = grouping; freeUntil = 1L}
        let xs = new List<_>()
        xs.Add(o)
        {programName = (r ^^! ("programName")); inputTime = int64(r ^^! "time"); staff = xs}
        |> JsonConvert.SerializeObject

    let swapStub (r : HttpRequest) = 
        logger.Info <| sprintf "Swap Query: %s" r.rawQuery
        let staffType = r ^^ "staffType" |> Option.fold (fun _ s -> s) "UCSFEM-R1"
        let delphine = {id = 1; firstName = "Delphine"; lastName = "Huang"; staffType = staffType} 
        let programName = r ^^! "programName"
        let start = int64(r ^^! "startTime")
        let endTime = int64(r ^^! "endTime")
        let (o : SwapResponseObject) = {staffObject = delphine}
        let xs = new List<_>()
        xs.Add(o)
        {programName = programName; start = start; endTime = endTime; staff = xs; swapWindow = 0L}
        |> JsonConvert.SerializeObject

    let routes = 
        let returnJson = Writers.setMimeType "application/json; charset=utf-8"
        [
          path "/api/hang" >>= request (OK << hangStub) >>= returnJson
          path "/api/swap" >>= request (OK << swapStub) >>= returnJson
          pathScan "/api/%s/%d" findFreeResidentsAsJson >>= returnJson
          path "/" >>= Suave.Http.Files.file "content/index.html"
          pathScan "/%s" (Suave.Http.Files.file  << sprintf "content/%s")
          RequestErrors.NOT_FOUND "Found no handlers" 
        ]

    let residentsApp = GET >>= choose routes 