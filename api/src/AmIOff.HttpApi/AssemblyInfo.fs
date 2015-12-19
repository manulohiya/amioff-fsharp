namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("AmIOff.HttpApi")>]
[<assembly: AssemblyProductAttribute("AmIOff.HttpApi")>]
[<assembly: AssemblyDescriptionAttribute("Http api for medical residents to find fellow residents to hang out with.")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
