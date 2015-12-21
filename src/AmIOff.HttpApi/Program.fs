open AmIOff.HttpApi
open System
open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open Suave.Json
open Suave.Types


[<EntryPoint>]
let main args =
    printfn "Arguments passed to function : %A" args
    // Return 0. This indicates success.

    let serverConfig = 
      let port = 
        try
            int (Environment.GetEnvironmentVariable("PORT"))
        with
            | exn -> 3000
      { Web.defaultConfig with
          homeFolder = Some __SOURCE_DIRECTORY__ 
          logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Warn
          bindings = [ Types.HttpBinding.mk' Types.HTTP "0.0.0.0" port ] }

    Web.startWebServer serverConfig Service.residentsApp

    0