module ThriftSharp.Tests.``Communication``

open System
open System.Collections.Generic
open System.Threading
open System.Reflection
open Xunit
open ThriftSharp
open ThriftSharp.Protocols
open ThriftSharp.Transport

let getField obj name: 'a =
    obj.GetType().GetField(name, BindingFlags.NonPublic ||| BindingFlags.Instance).GetValue(obj) :?> 'a

let getProp obj name: 'a =
    obj.GetType().GetProperty(name, BindingFlags.Public ||| BindingFlags.Instance).GetValue(obj) :?> 'a

[<Fact>]
let ``Binary().OverHttp() returns a binary transport over HTTP.``() =
    let headers = dict ["a", "b"]
    let timeout = TimeSpan.FromSeconds(2.1)
    let token = CancellationToken(false)
    let comm = ThriftCommunication.Binary().OverHttp("http://example.org", headers, nullable timeout)
    let prot = comm.CreateProtocol(token)
    let trans: IThriftTransport = getField prot "_transport"
    
    getField trans "_url" <=> "http://example.org"
    getField trans "_headers" <=> headers
    getField trans "_token" <=> token
    getProp (getField trans "_client") "Timeout" <=> TimeSpan.FromSeconds(2.1)

[<Fact>]
let ``OverHttp() handles null parameters correctly.``() =
    let comm = ThriftCommunication.Binary().OverHttp("http://example.org")
    let prot = comm.CreateProtocol(CancellationToken(false))
    let trans: IThriftTransport = getField prot "_transport"
    
    getField trans "_headers" <=> Dictionary<string, string>()
    getProp (getField trans "_client") "Timeout" <=> TimeSpan.FromSeconds(5.0)

[<Fact>]
let ``Equals is overriden to throw``() =
    Assert.Throws<InvalidOperationException>(fun () -> ThriftCommunication.Equals(obj(), obj()))

[<Fact>]
let ``ReferenceEquals is overriden to throw``() =
    Assert.Throws<InvalidOperationException>(fun () -> ThriftCommunication.ReferenceEquals(obj(), obj()))