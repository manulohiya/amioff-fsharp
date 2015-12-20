namespace AmIOff.HttpApi

open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open Suave.Json
open Suave.Types

module Service = 

    let private ofUnixTime (unix : int) = 
        let dtDateTime = new System.DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
        dtDateTime.AddSeconds(float unix).ToLocalTime()

    let private findFreeResidentsAsJson  (login, time) (x : HttpContext) = 
        let time = ofUnixTime time
        async {
            let maybeRequest = Request.tryCreate (Month.ofInt time.Month) time.Year login
            match maybeRequest with
            | None -> return! Suave.Http.RequestErrors.BAD_REQUEST "Invalid login or date" x
            | Some request -> 
                let! raw = Request.fetchRaw request
                match Timesheet.tryMapAmionResponseToCsv raw with
                | Some timesheet -> 
                    let residents = Timesheet.toResidents timesheet
                    let freeResidents = Timesheet.freeResidents residents time timesheet
                    let freeResidentsAsJSON = 
                        let joined = 
                            freeResidents 
                            |> List.map Resident.toJson
                            |> String.concat ","
                        sprintf "[%s]" joined
                    return! OK freeResidentsAsJSON x
                | None -> return! Suave.Http.RequestErrors.BAD_REQUEST "Invalid login or date" x
        }                

    let residentsApp =
        let returnJson = Writers.setMimeType "application/json; charset=utf-8"
        GET >>= choose [
          pathScan "/api/%s/%d" findFreeResidentsAsJson >>= returnJson
          RequestErrors.NOT_FOUND "Found no handlers" ]