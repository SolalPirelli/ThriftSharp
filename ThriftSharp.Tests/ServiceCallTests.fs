// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Service calls``

open System.Threading.Tasks
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals

[<ThriftService("Service")>]
type IService =
    [<ThriftMethod("AsyncMethod")>]
    abstract Async: [<ThriftParameter(1s, "arg")>] arg: int -> Task<string>

[<TestContainer>]
type __() =
    [<Test>]
    member __.``Asynchronous call``() = run <| async {
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.String)
                                String "the result"
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let svc = ThriftAttributesParser.ParseService(typeof<IService>.GetTypeInfo())
        let! res = Thrift.CallMethodAsync(ThriftCommunication(m), svc, "Async", 1)
                         .ContinueWith(fun (x: Task<obj>) -> x.Result :?> string)
                |> Async.AwaitTask
        res <=> "the result"
    }