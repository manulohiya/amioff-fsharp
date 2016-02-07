// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

printfn "OK"

#I "../../packages/Suave/lib/net40"
#r "Suave.dll"
#I @"../../packages/FSharp.Data/lib/net40/"
#r "FSharp.Data.dll"
#I @"../../packages/log4net/lib/net45-full/"
#r "log4net.dll"

#I "bin/Debug"
#r "AmIOff.HttpApi.exe"

open AmIOff.HttpApi
open System
open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open Suave.Json
open Suave.Types
open log4net

type Log4Net () = 
    let fileInfo = new System.IO.FileInfo (__SOURCE_DIRECTORY__ + "/app.config")
    let _ = log4net.Config.XmlConfigurator.Configure (fileInfo)
    let logger = LogManager.GetLogger "Suave" 
    interface Logging.Logger with
        member x.Log level fn = 
            match level with
            | Logging.LogLevel.Debug
            | Logging.LogLevel.Verbose ->
                let msg = (fn ()).message
                logger.Debug msg
            | Logging.LogLevel.Error ->
                let msg = (fn ()).message
                logger.Warn msg
            | Logging.LogLevel.Warn -> 
                let msg = (fn ()).message
                logger.Debug msg
            | Logging.LogLevel.Fatal ->
                let msg = (fn ()).message
                logger.Debug msg
            | Logging.LogLevel.Info ->
                let msg = (fn ()).message
                logger.Debug msg



let main args =
    printfn "Arguments passed to function : %A" args
    let logger = LogManager.GetLogger "main"
    let log4net = new Log4Net ()
    let serverConfig = 
      let port = 
        try
            int (Environment.GetEnvironmentVariable("PORT"))
        with
            | exn -> 3000
      { Web.defaultConfig with
          homeFolder = Some __SOURCE_DIRECTORY__ 
          logger = log4net
          bindings = [ Types.HttpBinding.mk' Types.HTTP "0.0.0.0" port ] }

    Web.startWebServer serverConfig Service.residentsApp

    0

main [||]