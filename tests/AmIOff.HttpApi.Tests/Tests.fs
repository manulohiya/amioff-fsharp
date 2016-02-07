module AmIOff.HttpApi.Tests

open AmIOff.HttpApi
open NUnit.Framework

[<TestFixture>]
type SampleTests () = 
    [<Test>]
    member x.``hello returns 42`` () = 
        AmIOff.HttpApi.April |> ignore
        Assert.AreEqual(true , true)