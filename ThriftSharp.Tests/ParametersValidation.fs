module ThriftSharp.Tests.``Parameters validation``

open System
open System.Linq.Expressions
open System.Threading.Tasks
open Xunit
open ThriftSharp

let fails func =
    Assert.ThrowsAny<ArgumentException>(func >> ignore) |> ignore
    

let StringData() = // xUnit crashes if this has no parameters
    [| [| null |]; [| "" |]; [| " \t \n" |] |]

[<Theory;
  MemberData("StringData")>]
let ``ThriftFieldAttribute: name``(value) =
    fails (fun () -> ThriftFieldAttribute(1s, false, value))
    

[<Theory;
  MemberData("StringData")>]
let ``ThriftStructAttribute: name``(value) =
    fails (fun () -> ThriftStructAttribute(value))


[<Theory;
  MemberData("StringData")>]
let ``ThriftParameterAttribute: name``(value) =
    fails (fun () -> ThriftParameterAttribute(1s, value))


[<ThriftStruct("CustomException")>]
type CustomException() = inherit exn()

[<Theory;
  MemberData("StringData")>]
let ``ThriftThrowsAttribute: name``(value) =
    fails (fun () -> ThriftThrowsAttribute(1s, value, typeof<CustomException>))
    

[<Theory;
  MemberData("StringData")>]
let ``ThriftMethodAttribute: name``(value) =
    fails (fun () -> ThriftMethodAttribute(value))


[<Theory;
  MemberData("StringData")>]
let ``ThriftServiceAttribute: name``(value) =
    fails (fun () -> ThriftServiceAttribute(value))

    
[<Fact>]
let ``ThriftCommunication#UsingCustomProtocol: protocolCreator``() =
    fails (fun () -> ThriftCommunication.UsingCustomProtocol(null))

[<Theory;
  MemberData("StringData")>]
let ``ThriftCommunication#OverHttp: url``(value) =
    fails (fun () -> ThriftCommunication.Binary().OverHttp(value))
    
[<Fact>]
let ``ThriftCommunication#UsingCustomTransport: transportCreator``() =
    fails (fun () -> ThriftCommunication.Binary().UsingCustomTransport(null))