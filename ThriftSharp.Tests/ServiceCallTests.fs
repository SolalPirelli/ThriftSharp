// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Service calls``

open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open ThriftSharp
open ThriftSharp.Internals
open ThriftSharp.Protocols

[<ThriftStruct("MyException")>]
type MyException() =
    inherit System.Exception()

    [<ThriftField(1s, true, "text")>]
    member val Text = "" with get, set

[<ThriftService("Service")>]
type IService =
    [<ThriftMethod("NoArgs")>]
    abstract NoArgs: unit -> Task

    [<ThriftMethod("OneArg")>]
    abstract OneArg: [<ThriftParameter(1s, "arg")>] arg: int -> Task

    [<ThriftMethod("WithReturn")>]
    abstract WithReturn: unit -> Task<int>

    [<ThriftMethod("Cancellable")>]
    abstract Cancellable: [<ThriftParameter(1s, "arg")>] arg: int * tok: CancellationToken -> Task

    [<ThriftMethod("Complex")>]
    abstract Complex: [<ThriftParameter(1s, "arg1")>] arg1: string
                    * [<ThriftParameter(2s, "arg2")>] arg2: double
                    * [<ThriftParameter(3s, "arg3")>] arg3: int[]
                    -> Task<List<string>>

    [<ThriftMethod("ConvertedReturn")>]
    abstract ConvertedReturn: unit -> [<return: ThriftConverter(typeof<ThriftUnixDateConverter>)>] Task<System.DateTime>

    [<ThriftMethod("WithException")>]
    [<ThriftThrows(1s, "exn", typeof<MyException>)>]
    abstract WithException: unit -> Task<int>

    [<ThriftMethod("OneWay", true)>]
    abstract OneWay: unit -> Task

[<AbstractClass>]
type Tests() =
    let (-) a b = a, b
    let (--) (a, b) c = a, b, c
    let (---) (a, b, c) d = a, b, c, d

    // using AwaitIAsyncResult produces weird results
    let awaitNonGenTask (t: Task) =
        let awaiter = t.GetAwaiter()
        Async.FromContinuations(fun (cont, econt, ccont) ->
            awaiter.OnCompleted(fun () ->
                if t.IsCanceled then
                    ccont(t.Exception.InnerException :?> System.OperationCanceledException)
                elif t.IsFaulted then
                    econt(t.Exception.InnerException)
                else
                    cont())
        )

    abstract GetService: IThriftProtocol -> IService

    member x.Test(func: IService -> Async<'a>, writtenData: ThriftProtocolValue list, 
                  toRead: ThriftProtocolValue list, expected: 'a) = run <| async {
        let prot = MemoryProtocol(toRead)
        let svc = x.GetService(prot)
        let! res = func(svc)
        prot.WrittenValues <=> writtenData
        res <=> expected
    }

    member x.TestException(func: (IService -> Async<_>), writtenData: ThriftProtocolValue list, 
                           toRead: ThriftProtocolValue list, expected: 'e) = run <| async {
        let prot = MemoryProtocol(toRead)
        let svc = x.GetService(prot)
        let! exn = throwsAsync<'e> (async { let! res = func(svc) in return box res })
        prot.WrittenValues <=> writtenData
        exn <=> expected
    }

    [<Test>]
    member x.``No arguments, no return value``() =
        x.Test (
            fun s -> s.NoArgs() |> awaitNonGenTask
            -
            [MessageHeader (0, "NoArgs", ThriftMessageType.Call)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            ---
            ()
        )

    [<Test>]
    member x.``Undeclared exception thrown``() =
        x.TestException (
            fun s -> s.NoArgs() |> awaitNonGenTask
            -
            [MessageHeader (0, "NoArgs", ThriftMessageType.Call)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Exception)
             StructHeader "TApplicationException"
             FieldHeader (1s, "message", tid 11)
             String "Error"
             FieldEnd
             FieldHeader (2s, "type", tid 8)
             Int32 6
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            ---
            ThriftProtocolException(ThriftProtocolExceptionType.InternalError, Message = "Error")
        )

    [<Test>]
    member x.``One argument, no return value``() =
        x.Test (
            fun s -> s.OneArg(1) |> awaitNonGenTask
            -
            [MessageHeader (0, "OneArg", ThriftMessageType.Call)
             StructHeader ""
             FieldHeader (1s, "arg", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            ---
            ()
        )

    [<Test>]
    member x.``No arguments, with return value``() =
        x.Test (
            fun s -> s.WithReturn() |> Async.AwaitTask
            -
            [MessageHeader (0, "WithReturn", ThriftMessageType.Call)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldHeader (0s, "", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            ---
            1
        )

    [<Test>]
    member x.``One argument, a cancellation token and no return value``() =
        let source = CancellationTokenSource()
        x.Test (
            fun s -> s.Cancellable(1, source.Token) |> awaitNonGenTask
            -
            [MessageHeader (0, "Cancellable", ThriftMessageType.Call)
             StructHeader ""
             FieldHeader (1s, "arg", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            ---
            ()
        )
        source.Cancel()

    [<Test>]
    member x.``One argument, an already-canceled token and no return value``() =
        let source = CancellationTokenSource()
        source.Cancel()
        
        x.Test (
            fun s -> s.Cancellable(1, source.Token) |> awaitNonGenTask
            -
            [MessageHeader (0, "Cancellable", ThriftMessageType.Call)
             StructHeader ""
             FieldHeader (1s, "arg", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            ---
            ()
        )

    [<Test>]
    member x.``Three arguments, with return value``() =
        x.Test (
            fun s -> s.Complex("x", 1.0, [| 1; 2 |]) |> Async.AwaitTask
            -
            [MessageHeader (0, "Complex", ThriftMessageType.Call)
             StructHeader ""
             FieldHeader (1s, "arg1", tid 11)
             String "x"
             FieldEnd
             FieldHeader (2s, "arg2", tid 4)
             Double 1.0
             FieldEnd
             FieldHeader (3s, "arg3", tid 15)
             ListHeader (2, tid 8)
             Int32 1
             Int32 2
             ListEnd
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldHeader (0s, "", tid 15)
             ListHeader (3, tid 11)
             String "a"
             String "b"
             String "c"
             ListEnd
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            ---
            List ["a"; "b"; "c"]
        )

    [<Test>]
    member x.``No args, converted return value``() =
        x.Test (
            fun s -> s.ConvertedReturn() |> Async.AwaitTask
            -
            [MessageHeader (0, "ConvertedReturn", ThriftMessageType.Call)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldHeader (0s, "", tid 8)
             Int32 787708800
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            ---
            date(18, 12, 1994)
        )

    [<Test>]
    member x.``Exception declared, but not thrown``() =
        x.Test (
            fun s -> s.WithException() |> Async.AwaitTask
            -
            [MessageHeader (0, "WithException", ThriftMessageType.Call)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldHeader (0s, "", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            ---
            1
        )

    [<Test>]
    member x.``Exception declared and thrown``() =
        x.TestException (
            fun s -> s.WithException() |> Async.AwaitTask
            -
            [MessageHeader (0, "WithException", ThriftMessageType.Call)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            --
            [MessageHeader (0, "", ThriftMessageType.Reply)
             StructHeader ""
             FieldHeader (1s, "exn", tid 12)
             StructHeader ("MyException")
             FieldHeader (1s, "text", tid 11)
             String "Error"
             FieldEnd
             FieldStop
             StructEnd
             FieldEnd
             FieldStop
             StructEnd
             MessageEnd]
            ---
            MyException(Text = "Error")
        )

    [<Test>]
    member x.``One-way method``() =
        x.Test (
            fun s -> s.OneWay() |> awaitNonGenTask
            -
            [MessageHeader (0, "OneWay", ThriftMessageType.OneWay)
             StructHeader ""
             FieldStop
             StructEnd
             MessageEnd]
            --
            []
            ---
            ()
        )


type ServiceImpl(prot) =
    inherit ThriftServiceImplementation<IService>(ThriftCommunication(prot))

    interface IService with
        member x.NoArgs() = base.CallAsync("NoArgs")     
        member x.OneArg(arg) = base.CallAsync("OneArg", arg)      
        member x.WithReturn() = base.CallAsync<int>("WithReturn")      
        member x.Cancellable(arg, tok) = base.CallAsync("Cancellable", arg, tok)      
        member x.Complex(arg1, arg2, arg3) = base.CallAsync<List<string>>("Complex", arg1, arg2, arg3)  
        member x.ConvertedReturn() = base.CallAsync<System.DateTime>("ConvertedReturn")
        member x.WithException() = base.CallAsync<int>("WithException")
        member x.OneWay() =  base.CallAsync("OneWay")

[<TestClass>]
type ServiceImplementation() =
    inherit Tests()

    override x.GetService prot =
        ServiceImpl(prot) :> IService


[<TestClass>]
type Proxy() =
    inherit Tests()

    override x.GetService prot =
        ThriftProxy.Create<IService>(ThriftCommunication(prot))