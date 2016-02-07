namespace AmIOff.HttpApi

open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open Suave.Json
open Suave.Types
open Suave.Http.Files

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

    let routes = 
        let returnJson = Writers.setMimeType "application/json; charset=utf-8"
        [
          pathScan "/api/%s/%d" findFreeResidentsAsJson >>= returnJson
          path "/" >>= Suave.Http.Files.file "content/index.html"
          pathScan "/%s" (Suave.Http.Files.file  << sprintf "content/%s")
          RequestErrors.NOT_FOUND "Found no handlers" 
        ]

    let residentsApp = GET >>= choose routes 