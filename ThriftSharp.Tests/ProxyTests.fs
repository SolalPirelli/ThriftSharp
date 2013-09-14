﻿namespace ThriftSharp.Tests

open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals

[<ThriftService("Service")>]
type IService10 =
    [<ThriftMethod("Sync")>]
    abstract Sync: [<ThriftParameter(1s, "arg")>] arg: int -> string
    [<ThriftMethod("SyncReturnsValueType")>]
    abstract SyncReturnsValueType: unit -> int
    [<ThriftMethod("SyncNoReturn")>]
    abstract SyncNoReturn: unit -> unit
    [<ThriftMethod("Async")>]
    abstract Async: [<ThriftParameter(1s, "arg")>] arg: int -> Task<int>
    [<ThriftMethod("AsyncNoReturn")>]
    abstract AsyncNoReturn: unit -> Task
    [<ThriftMethod("Complex")>]
    abstract Complex: [<ThriftParameter(1s, "arg1")>] arg1: string
                    * [<ThriftParameter(2s, "arg2")>] arg2: double
                    * [<ThriftParameter(3s, "arg3")>] arg3: int[]
                    -> Task<List<string>>

[<TestClass>]
type ``Proxy generator``() =
    [<Test>]
    member x.``Synchronous call``() =
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.String)
                                String "the result"
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let comm = ThriftCommunication(m)
        let impl = ThriftProxy.Create<IService10>(comm)

        impl.Sync(1) <=> "the result"

        m.WrittenValues <===> [MessageHeader (0, "Sync", ThriftMessageType.Call)
                               StructHeader ""
                               FieldHeader (1s, "arg", ThriftType.Int32)
                               Int32 1
                               FieldEnd
                               FieldStop
                               StructEnd
                               MessageEnd]

    [<Test>]
    member x.``Synchronous call, returns a value type``() =
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.Int32)
                                Int32 34
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let comm = ThriftCommunication(m)
        let impl = ThriftProxy.Create<IService10>(comm)

        impl.SyncReturnsValueType() <=> 34

        m.WrittenValues <===> [MessageHeader (0, "SyncReturnsValueType", ThriftMessageType.Call)
                               StructHeader ""
                               FieldStop
                               StructEnd
                               MessageEnd]

    [<Test>]
    member x.``Synchronous call, no return value``() =
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldStop
                                StructEnd
                                MessageEnd])
        let comm = ThriftCommunication(m)
        let impl = ThriftProxy.Create<IService10>(comm)

        impl.SyncNoReturn() <=> ()

        m.WrittenValues <===> [MessageHeader (0, "SyncNoReturn", ThriftMessageType.Call)
                               StructHeader ""
                               FieldStop
                               StructEnd
                               MessageEnd]

    [<Test>]
    member x.``Asynchronous call``() =
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.Int32)
                                Int32 123
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let comm = ThriftCommunication(m)
        let impl = ThriftProxy.Create<IService10>(comm) 

        (impl.Async(2) |> Async.AwaitTask |> Async.RunSynchronously) <=> 123

        m.WrittenValues <===> [MessageHeader (0, "Async", ThriftMessageType.Call)
                               StructHeader ""
                               FieldHeader (1s, "arg", ThriftType.Int32)
                               Int32 2
                               FieldEnd
                               FieldStop
                               StructEnd
                               MessageEnd]

    [<Test>]
    member x.``Asynchronous call, no return value``() =
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldStop
                                StructEnd
                                MessageEnd])
        let comm = ThriftCommunication(m)
        let impl = ThriftProxy.Create<IService10>(comm) 

        (impl.AsyncNoReturn() |> Async.AwaitIAsyncResult |> Async.Ignore |> Async.RunSynchronously) <=> ()

        m.WrittenValues <===> [MessageHeader (0, "AsyncNoReturn", ThriftMessageType.Call)
                               StructHeader ""
                               FieldStop
                               StructEnd
                               MessageEnd]

    [<Test>]
    member x.``Complex call``() =
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.List)
                                ListHeader (3, ThriftType.String)
                                String "the cake"
                                String "is"
                                String "a lie"
                                ListEnd
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let comm = ThriftCommunication(m)
        let impl = ThriftProxy.Create<IService10>(comm)

        (impl.Complex("abc", 123.4, [| 1; 2 |]) |> Async.AwaitTask |> Async.RunSynchronously)
        <===>
        ["the cake";"is";"a lie"]

        m.WrittenValues <===> [MessageHeader (0, "Complex", ThriftMessageType.Call)
                               StructHeader ""
                               FieldHeader (1s, "arg1", ThriftType.String)
                               String "abc"
                               FieldEnd
                               FieldHeader (2s, "arg2", ThriftType.Double)
                               Double 123.4
                               FieldEnd
                               FieldHeader (3s, "arg3", ThriftType.List)
                               ListHeader (2, ThriftType.Int32)
                               Int32 1
                               Int32 2
                               ListEnd
                               FieldEnd
                               FieldStop
                               StructEnd
                               MessageEnd]