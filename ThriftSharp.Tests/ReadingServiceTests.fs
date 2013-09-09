namespace ThriftSharp.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals

[<ThriftStruct("MyException")>]
type MyException1() =
    inherit Exception()
    [<ThriftField(1s, true, "Stuff")>]
    member val Stuff = "" with get, set

[<ThriftService("Service1")>]
type Service1 =
    [<ThriftMethod("noReply")>]
    abstract NoReply: [<ThriftParameter(1s, "param")>] param: int -> unit

    [<ThriftMethod("withException")>]
    [<ThriftThrows(1s, "exn", typeof<MyException1>)>]
    abstract WithException: unit -> int

    [<ThriftMethod("noReplyWithException")>]
    [<ThriftThrows(1s, "exn", typeof<MyException1>)>]
    abstract NoReplyWithException: unit -> unit


[<TestClass>]
type ``Reading service replies``() =
    member x.ReadMsg<'T>(prot, name, args) = Thrift.CallMethod<'T>(prot, name, args)

    [<Test>]
    member x.``No reply, none received``() =
        let m = MemoryProtocol([MessageHeader (0, "noReply", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldStop
                                StructEnd
                                MessageEnd])
        let reply = x.ReadMsg<Service1>(m, "NoReply", [| 0 |])
        reply <=> null