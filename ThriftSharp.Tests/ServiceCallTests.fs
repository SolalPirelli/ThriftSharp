// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Service calls``

open System.Threading
open System.Threading.Tasks
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals
open ThriftSharp.Protocols
open ThriftSharp.Transport

[<ThriftService("Service")>]
type IService =
    [<ThriftMethod("AsyncMethod")>]
    abstract Async: [<ThriftParameter(1s, "arg")>] arg: int -> Task<string>

    [<ThriftMethod("Cancellable")>]
    abstract Cancellable: [<ThriftParameter(1s, "arg")>] arg: int -> tok: CancellationToken -> Task<string>

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

    [<Test>]
    member __.``Async call with CancellationToken``() = run <| async {
        let source = CancellationTokenSource()
        source.CancelAfter(50)

        let trans = { new IThriftTransport with
                          member x.ReadByteAsync() = Task.FromResult(0uy)
                          member x.ReadBytesAsync(_) = Task.FromResult(Array.empty)
                          member x.WriteByte(_) = ()
                          member x.WriteBytes(_) = ()
                          member x.FlushAsync() = Task.Delay(10000, source.Token)
                          member x.Dispose() = () }

        let svc = ThriftAttributesParser.ParseService(typeof<IService>.GetTypeInfo())
        let comm = ThriftCommunication(ThriftBinaryProtocol(trans))

        do! throwsAsync<System.OperationCanceledException> (fun () -> 
            Thrift.CallMethodAsync(comm, svc, "Cancellable", 1, source.Token) |> Async.AwaitTask) |> Async.Ignore
    }

    [<Test>]
    member __.``Async call with CancellationToken, already canceled``() = run <| async {  
        let source = CancellationTokenSource()
        source.Cancel()

        let trans = { new IThriftTransport with
                          member x.ReadByteAsync() = Task.FromResult(0uy)
                          member x.ReadBytesAsync(_) = Task.FromResult(Array.empty)
                          member x.WriteByte(_) = ()
                          member x.WriteBytes(_) = ()
                          member x.FlushAsync() = Task.Delay(10000, source.Token)
                          member x.Dispose() = () }

        let svc = ThriftAttributesParser.ParseService(typeof<IService>.GetTypeInfo())
        let comm = ThriftCommunication(ThriftBinaryProtocol(trans))

        do! throwsAsync<System.OperationCanceledException> (fun () -> 
            Thrift.CallMethodAsync(comm, svc, "Cancellable", 1, source.Token) |> Async.AwaitTask) 
         |> Async.Ignore
    }