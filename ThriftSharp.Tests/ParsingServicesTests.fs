module ThriftSharp.Tests.``Parsing services``

open System
open System.Reflection
open System.Threading.Tasks
open ThriftSharp
open ThriftSharp.Internals

type NotAnInterface() =
    [<ThriftMethod("test")>]
    member x.Test() = Task.FromResult(0)

type UnmarkedService =
    [<ThriftMethod("test")>]
    abstract Test: unit -> Task<int>

[<ThriftService("NoMethods")>]
type NoMethods =
    interface end

[<ThriftService("UnmarkedMethod")>]
type ServiceWithUnmarkedMethod =
    abstract Test: unit -> Task<int>
    
[<ThriftService("UnmarkedMethodParameter")>]
type ServiceWithUnmarkedMethodParameter =
    [<ThriftMethod("test")>]
    abstract Test: int -> Task<int>


[<ThriftStruct("CustomException")>]
type CustomException() =
    inherit Exception()

[<ThriftService("NormalService")>]
type NormalService =
    [<ThriftMethod("test1")>]
    abstract Test1: unit -> Task<int>

    [<ThriftMethod("test2")>]
    abstract Test2: [<ThriftParameter(1s, "stuff")>] stuff: int -> Task
  
    [<ThriftMethod("test3")>]
    [<ThriftThrows(1s, "exn", typeof<CustomException>)>]
    abstract Test3: unit -> Task<int>


[<ThriftService("ThrowsInteger")>]
type ServiceThrowingInteger =
    [<ThriftMethod("test")>]
    [<ThriftThrows(1s, "not_exn", typeof<int>)>]
    abstract Test: unit -> Task<int>

[<ThriftService("SynchronousService")>]
type SynchronousService =
    [<ThriftMethod("test")>]
    abstract Test: unit -> int


let parseOk<'T> () =
    ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo()) |> ignore

let parseError<'T> () =
    throwsAsync<ThriftParsingException> (fun () -> async { return box (ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())) })
 |> Async.RunSynchronously
 |> ignore


[<TestContainer>]
type __() =
    [<Test>]
    member __.``Error on concrete type``() =
        parseError<NotAnInterface>()

    [<Test>]
    member __.``Error on service without an attribute``() =
        parseError<UnmarkedService>()

    [<Test>]
    member __.``Service without methods``() =
        parseOk<NoMethods>()

    [<Test>]
    member __.``Method without attributes``() =
        parseOk<ServiceWithUnmarkedMethod>()

    [<Test>]
    member __.``Error on method parameter without attribute``() =
        parseError<ServiceWithUnmarkedMethodParameter>()

    [<Test>]
    member __.``Normal service``() =
        parseOk<NormalService>()

    [<Test>]
    member __.``Error on 'throws' clause throwing non-exception``() =
        parseError<ServiceThrowingInteger>()

    [<Test>]
    member __.``Error on synchronous service``() =
        parseError<SynchronousService>()