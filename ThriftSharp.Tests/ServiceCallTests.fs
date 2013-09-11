namespace ThriftSharp.Tests

open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals

[<ThriftService("Service")>]
type IService8 =
    [<ThriftMethod("SyncMethod")>]
    abstract Sync: [<ThriftParameter(1s, "arg")>] arg: int -> string
    [<ThriftMethod("AsyncMethod")>]
    abstract Async: [<ThriftParameter(1s, "arg")>] arg: int -> Task<string>

[<TestClass>]
type ``Service calls``() =
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
        let svc = ThriftAttributesParser.ParseService(typeof<IService8>)
        let res = Thrift.CallMethod(m, svc, "Sync", 1) :?> string
        res <=> "the result"

    [<Test>]
    member x.``Asynchronous call``() =
        let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                                StructHeader ""
                                FieldHeader (0s, "", ThriftType.String)
                                String "the result"
                                FieldEnd
                                FieldStop
                                StructEnd
                                MessageEnd])
        let svc = ThriftAttributesParser.ParseService(typeof<IService8>)
        let res = (Thrift.CallMethod(m, svc, "Async", 1) :?> Task<obj>).ContinueWith(fun (x: Task<obj>) -> x.Result :?> string)
                |> Async.AwaitTask |> Async.RunSynchronously
        res <=> "the result"