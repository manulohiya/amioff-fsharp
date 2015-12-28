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

    let private ofUnixTime (unix : int) = 
        let dtDateTime = new System.DateTime(1970,1,1,0,0,0,0, System.DateTimeKind.Local)
        printfn "Unix DateTime: %A" dtDateTime
        printfn "UNIX: %d" unix
        dtDateTime.AddSeconds(float unix)
        |> fun x -> printfn "DateTime: %A" x; x

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
                    let freeResidents = Timesheet.freeResidents residents time offset timesheet
                    let freeResidentsAsJSON = 
                        let joined = 
                            freeResidents 
                            |> List.map Resident.toJson
                            |> String.concat ","
                        sprintf "[%s]" joined

                    printfn "Returning JSON: %s" freeResidentsAsJSON
                    return! OK freeResidentsAsJSON x
                | None -> return! Suave.Http.RequestErrors.BAD_REQUEST "Invalid login or date" x
        }                

    let stylesAndScripts = 
        [ for file in System.IO.Directory.EnumerateFiles ("content") ->
            printfn "File: %A" file
            path ("/" + file) >>= Suave.Http.Files.file file
        ]

    let routes = 
        let returnJson = Writers.setMimeType "application/json; charset=utf-8"
        [
          //for f in stylesAndScripts -> f
          pathScan "/api/%s/%d" findFreeResidentsAsJson >>= returnJson
          path "/" >>= Suave.Http.Files.file "content/index.html"
          RequestErrors.NOT_FOUND "Found no handlers" 
        ]

    let getRoutes = stylesAndScripts @ routes

    let residentsApp =
        let homePath = __SOURCE_DIRECTORY__ + "/content/"
        let fullPath = Suave.Http.Files.resolvePath homePath "index.html"
        printfn "FullPath :%s" fullPath 
        printfn "HomePath : %s" homePath 
        GET >>= choose getRoutes 