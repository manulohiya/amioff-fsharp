// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "Library.fs"
#I @"../../packages/FSharp.Data/lib/portable-net40+sl5+wp8+win8/"
#r "FSharp.Data.dll"

open AmIOff.HttpApi

let num = Library.hello 42
printfn "%i" num
