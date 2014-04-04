// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Reading service replies``

open System.Threading.Tasks
open ThriftSharp
open ThriftSharp.Internals

[<ThriftStruct("MyException")>]
type MyException() =
    inherit System.Exception()
    [<ThriftField(1s, true, "stuff")>]
    member val Stuff = "" with get, set

[<ThriftService("Service")>]
type Service =
    [<ThriftMethod("noReply")>]
    abstract NoReply: unit -> Task

    [<ThriftMethod("noReplyWithException")>]
    [<ThriftThrows(1s, "exn", typeof<MyException>)>]
    abstract NoReplyWithException: unit -> Task

    [<ThriftMethod("noException")>]
    abstract NoException: unit -> Task<int>

    [<ThriftMethod("withException")>]
    [<ThriftThrows(1s, "exn", typeof<MyException>)>]
    abstract WithException: unit -> Task<int>

    [<ThriftMethod("withUnixDateReturnValue")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    abstract WithUnixDateReturnValue: unit -> Task<System.DateTime>


let (--) a b = a,b

let (==>) (data, methodName) (checker: 'a -> unit) = run <| async {
    let m = MemoryProtocol(data)
    let! reply = readMsgAsync<Service> m methodName
    m.IsEmpty <=> true
    do checker (reply :?> 'a)
}

let (=//=>) (data, methodName) (checker: 'a -> unit) = run <| async {
    let m = MemoryProtocol(data)
    return! throwsAsync<'a> (fun () -> readMsgAsync<Service> m methodName)
}


[<TestContainer>]
type __() =
    [<Test>]
    member __.``No reply expected, none received``() =
        [MessageHeader (0, "noReply", ThriftMessageType.Reply)
         StructHeader ""
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoReply"
        ==>
        fun (o: obj) ->
            o <=> null

    [<Test>]
    member __.``No reply expected, but one was received``() =
        [MessageHeader (0, "noReply", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (0s, "", tid 8uy)
         Int32 69
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoReply"
        ==>
        fun (o: obj) ->
            o <=> null

    [<Test>]
    member __.``No reply expected, but a server error was received``() =
        [MessageHeader (0, "noReply", ThriftMessageType.Exception)
         StructHeader "TApplicationException"
         FieldHeader (1s, "message", tid 11uy)
         String "An error occured."
         FieldEnd
         FieldHeader (2s, "type", tid 8uy)
         Int32 6
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoReply"
        =//=>
        fun (ex: ThriftProtocolException) ->
            ex.Message <=> "An error occured."
            ex.ExceptionType <=> ThriftProtocolExceptionType.InternalError

    [<Test>]
    member __.``No reply with exception declared, nothing received``() =
        [MessageHeader (0, "noReplyWithException", ThriftMessageType.Reply)
         StructHeader ""
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoReplyWithException"
        ==>
        fun (o: obj) ->
            o <=> null

    [<Test>]
    member __.``No reply with exception declared, but another one was received``() = 
        [MessageHeader (0, "noReplyWithException", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (10s, "exn", tid 13uy)
         StructHeader "MyException"
         FieldHeader (1s, "message", tid 12uy)
         String "Error."
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoReplyWithException"
        ==>
        fun (o: obj) ->
            o <=> null

    [<Test>]
    member __.``No reply with exception declared and received``() =
        [MessageHeader (0, "noReplyWithException", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (1s, "exn", tid 12uy)
         StructHeader "MyException"
         FieldHeader (1s, "stuff", tid 11uy)
         String "Everything went wrong."
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoReplyWithException"
        =//=>
        fun (ex: MyException) ->
            ex.Stuff <=> "Everything went wrong."

    [<Test>]
    member __.``Reply expected, but none was received``() =
        [MessageHeader (0, "noException", ThriftMessageType.Reply)
         StructHeader ""
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoException"
        =//=>
        fun (ex: ThriftProtocolException) ->
            ex.ExceptionType <=> ThriftProtocolExceptionType.MissingResult

    [<Test>]
    member __.``Reply expected and received``() =
        [MessageHeader (0, "noException", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (0s, "", tid 8uy)
         Int32 101
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoException"
        ==>
        fun (n: int) ->
            n <=> 101

    [<Test>]
    member __.``Reply expected, but an undeclared exception was received``() =
        [MessageHeader (0, "noException", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (1s, "exn", tid 12uy)
         StructHeader "MyException"
         FieldHeader (1s, "stuff", tid 11uy)
         String "Everything went wrong."
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "NoException"
        =//=>
        fun (ex: ThriftProtocolException) ->
            ex.ExceptionType <=> ThriftProtocolExceptionType.MissingResult

    [<Test>]
    member __.``Reply or exception expected, reply received``() =
        [MessageHeader (0, "withException", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (0s, "", tid 8uy)
         Int32 007
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "WithException"
        ==>
        fun (n: int) ->
            n <=> 7

    [<Test>]
    member __.``Reply or exception expected, exception received``() =
        [MessageHeader (0, "withException", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (1s, "exn", tid 12uy)
         StructHeader "MyException"
         FieldHeader (1s, "stuff", tid 11uy)
         String "Everything went wrong."
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "WithException" 
        =//=>
        fun (ex: MyException) ->
            ex.Stuff <=> "Everything went wrong."

    [<Test>]
    member __.``UnixDate return type``() =
        [MessageHeader (0, "withUnixDateReturnValue", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (0s, "", tid 8uy)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        "WithUnixDateReturnValue"
        ==>
        fun (date: System.DateTime) ->
            date.ToUniversalTime() <=> utcDate(18, 12, 1994)