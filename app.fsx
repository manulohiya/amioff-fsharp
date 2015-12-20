// --------------------------------------------------------------------------------------
// Start the 'app' WebPart defined in 'app.fsx' on Heroku using %PORT%
// --------------------------------------------------------------------------------------

#I "packages/Suave/lib/net40"
#r "Suave.dll"

#I @"packages/FSharp.Data/lib/net40/"
#r "FSharp.Data.dll"

#I "src/AmIOff.HttpApi/bin/Release"
#r "AmIOff.HttpApi.dll"

open AmIOff.HttpApi
open System
open Suave
open Suave.Http
open Suave.Http.Applicatives
open Suave.Http.Successful
open Suave.Web
open Suave.Json
open Suave.Types

let serverConfig =
  let port = 
    int (Environment.GetEnvironmentVariable("PORT"))
  { Web.defaultConfig with
      homeFolder = Some __SOURCE_DIRECTORY__
      logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Warn
      bindings = [ Types.HttpBinding.mk' Types.HTTP "0.0.0.0" port ] }

Web.startWebServer serverConfig Service.residentsApp