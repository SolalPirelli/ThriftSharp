﻿// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Proxy generator``

open System.Collections.Generic
open System.Threading.Tasks
open ThriftSharp
open ThriftSharp.Internals

[<ThriftService("Service")>]
type IService =
    [<ThriftMethod("Async")>]
    abstract Async: [<ThriftParameter(1s, "arg")>] arg: int -> Task<int>
    [<ThriftMethod("AsyncNoReturn")>]
    abstract AsyncNoReturn: unit -> Task
    [<ThriftMethod("Complex")>]
    abstract Complex: [<ThriftParameter(1s, "arg1")>] arg1: string
                    * [<ThriftParameter(2s, "arg2")>] arg2: double
                    * [<ThriftParameter(3s, "arg3")>] arg3: int[]
                    -> Task<List<string>>

                    
let (--) a b = a,b

let (==>) ((readData, meth), resultCheck) writtenData = run <| async {
    let m = MemoryProtocol(readData)
    let comm = ThriftCommunication(m)
    let impl = ThriftProxy.Create<IService>(comm)

    let! result = meth(impl) |> Async.AwaitTask
    resultCheck result

    m.WrittenValues <===> writtenData
}

let (-->) (readData, meth) writtenData = run <| async {
    let m = MemoryProtocol(readData)
    let comm = ThriftCommunication(m)
    let impl = ThriftProxy.Create<IService>(comm)

    let! result = meth(impl) |> Async.AwaitIAsyncResult

    m.WrittenValues <===> writtenData
}


[<TestContainer>]
type __() =
    [<Test>]
    member __.``Asynchronous call``() =
        [MessageHeader (0, "", ThriftMessageType.Reply)
         StructHeader ""
         FieldHeader (0s, "", ThriftType.Int32)
         Int32 123
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]
        --
        fun s -> s.Async(2)
        --
        fun n -> n <=> 123
        ==>
        [MessageHeader (0, "Async", ThriftMessageType.Call)
         StructHeader ""
         FieldHeader (1s, "arg", ThriftType.Int32)
         Int32 2
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``Asynchronous call, no return value``() =
        [MessageHeader (0, "", ThriftMessageType.Reply)
         StructHeader ""
         FieldStop
         StructEnd
         MessageEnd]
        --
        fun s -> s.AsyncNoReturn()
        -->
        [MessageHeader (0, "AsyncNoReturn", ThriftMessageType.Call)
         StructHeader ""
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``Complex call``() =
        [MessageHeader (0, "", ThriftMessageType.Reply)
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
         MessageEnd]
        --
        fun s -> s.Complex("abc", 123.4, [|1; 2|])
        --
        fun res -> res <===> ["the cake";"is";"a lie"]
        ==>
        [MessageHeader (0, "Complex", ThriftMessageType.Call)
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