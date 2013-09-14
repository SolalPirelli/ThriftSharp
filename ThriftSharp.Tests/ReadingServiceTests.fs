// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Protocols
open ThriftSharp.Internals

[<ThriftStruct("MyException")>]
type MyException5() =
    inherit System.Exception()
    [<ThriftField(1s, true, "stuff")>]
    member val Stuff = "" with get, set

[<ThriftService("Service")>]
type Service5 =
    [<ThriftMethod("noReply")>]
    abstract NoReply: unit -> unit

    [<ThriftMethod("noReplyWithException")>]
    [<ThriftThrows(1s, "exn", typeof<MyException5>)>]
    abstract NoReplyWithException: unit -> unit

    [<ThriftMethod("noException")>]
    abstract NoException: unit -> int

    [<ThriftMethod("withException")>]
    [<ThriftThrows(1s, "exn", typeof<MyException5>)>]
    abstract WithException: unit -> int

    [<ThriftMethod("withUnixDateReturnValue")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    abstract WithUnixDateReturnValue: unit -> System.DateTime


[<TestClass>]
type ``Reading service replies``() =
    member x.ReadMsg<'T>(prot: IThriftProtocol, name) =
        let svc = ThriftAttributesParser.ParseService(typeof<'T>)
        Thrift.CallMethod(ThriftCommunication(prot), svc, name, [| |])

    [<Test>]
    member x.``No reply expected, none received``() =
        let m = MemoryProtocol([MessageHeader (0, "noReply", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldStop
                                StructEnd
                                MessageEnd])
        let reply = x.ReadMsg<Service5>(m, "NoReply")
        reply <=> null

    [<Test>]
    member x.``No reply expected, but one was received``() =
        let m = MemoryProtocol([MessageHeader (0, "noReply", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.Int32)
                                Int32 69
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let reply = x.ReadMsg<Service5>(m, "NoReply")
        reply <=> null

    [<Test>]
    member x.``No reply expected, but a server error was received``() =
        let m = MemoryProtocol([MessageHeader (0, "noReply", ThriftMessageType.Exception)
                                StructHeader "TApplicationException"
                                FieldHeader (1s, "message", ThriftType.String)
                                String "An error occured."
                                FieldEnd
                                FieldHeader (2s, "type", ThriftType.Int32)
                                Int32 6
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let exn = throws<ThriftProtocolException> (fun () -> x.ReadMsg<Service5>(m, "NoReply"))
        exn.Message <=> "An error occured."
        exn.ExceptionType <=> ThriftProtocolExceptionType.InternalError

    [<Test>]
    member x.``No reply with exception declared, nothing received``() =
        let m = MemoryProtocol([MessageHeader (0, "noReplyWithException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldStop
                                StructEnd
                                MessageEnd])
        let reply = x.ReadMsg<Service5>(m, "NoReplyWithException")
        reply <=> null

    [<Test>]
    member x.``No reply with exception declared, but another one was received``() =
        let m = MemoryProtocol([MessageHeader (0, "noReplyWithException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (10s, "exn", ThriftType.Struct)
                                StructHeader "MyException"
                                FieldHeader (1s, "message", ThriftType.String)
                                String "Error."
                                FieldEnd
                                FieldStop
                                StructEnd
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let reply = x.ReadMsg<Service5>(m, "NoReplyWithException")
        reply <=> null

    [<Test>]
    member x.``No reply with exception declared and received``() =
        let m = MemoryProtocol([MessageHeader (0, "noReplyWithException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (1s, "exn", ThriftType.Struct)
                                StructHeader "MyException"
                                FieldHeader (1s, "stuff", ThriftType.String)
                                String "Everything went wrong."
                                FieldEnd
                                FieldStop
                                StructEnd
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])   
        let exn = throws<MyException5> (fun () -> x.ReadMsg<Service5>(m, "NoReplyWithException"))
        exn.Stuff <=> "Everything went wrong."

    [<Test>]
    member x.``Reply expected, but none was received``() =
        let m = MemoryProtocol([MessageHeader (0, "noException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldStop
                                StructEnd
                                MessageEnd])
        let exn = throws<ThriftProtocolException> (fun () -> x.ReadMsg<Service5>(m, "NoException"))
        exn.ExceptionType <=> ThriftProtocolExceptionType.MissingResult

    [<Test>]
    member x.``Reply expected and received``() =
        let m = MemoryProtocol([MessageHeader (0, "noException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.Int32)
                                Int32 101
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let res = x.ReadMsg<Service5>(m, "NoException")
        (res :?> int) <=> 101

    [<Test>]
    member x.``Reply expected, but an undeclared exception was received``() =
        let m = MemoryProtocol([MessageHeader (0, "noException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (1s, "exn", ThriftType.Struct)
                                StructHeader "MyException"
                                FieldHeader (1s, "stuff", ThriftType.String)
                                String "Everything went wrong."
                                FieldEnd
                                FieldStop
                                StructEnd
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])   
        let exn = throws<ThriftProtocolException> (fun () -> x.ReadMsg<Service5>(m, "NoException"))
        exn.ExceptionType <=> ThriftProtocolExceptionType.MissingResult

    [<Test>]
    member x.``Reply or exception expected, reply received``() =
        let m = MemoryProtocol([MessageHeader (0, "withException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.Int32)
                                Int32 007
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let res = x.ReadMsg<Service5>(m, "WithException")
        (res :?> int) <=> 7

    [<Test>]
    member x.``Reply or exception expected, exception received``() =
        let m = MemoryProtocol([MessageHeader (0, "withException", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (1s, "exn", ThriftType.Struct)
                                StructHeader "MyException"
                                FieldHeader (1s, "stuff", ThriftType.String)
                                String "Everything went wrong."
                                FieldEnd
                                FieldStop
                                StructEnd
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])   
        let exn = throws<MyException5> (fun () -> x.ReadMsg<Service5>(m, "WithException"))
        exn.Stuff <=> "Everything went wrong."

    [<Test>]
    member x.``UnixDate return type``() =
        let m = MemoryProtocol([MessageHeader (0, "withUnixDateReturnValue", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.Int32)
                                Int32 787708800
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        (x.ReadMsg<Service5>(m, "WithUnixDateReturnValue") :?> System.DateTime) <=> System.DateTime(1994, 12, 18)