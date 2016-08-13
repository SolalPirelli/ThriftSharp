// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

module ThriftSharp.Tests.``Service calls``

open System.Linq.Expressions
open System.Threading
open System.Threading.Tasks
open Xunit
open ThriftSharp
open ThriftSharp.Models
open ThriftSharp.Protocols
open ThriftSharp.Transport

[<ThriftStruct("CustomException")>]
type CustomException() =
    inherit exn()

    [<ThriftField(1s, true, "Value")>]
    member val Value = "" with get, set

[<ThriftService("Service")>]
type IService =
    [<ThriftMethod("NoReturn0")>]
    abstract NoReturn0: unit -> Task

    [<ThriftMethod("NoReturn1")>]
    abstract NoReturn1: [<ThriftParameter(1s, "arg1")>] arg1: bool -> Task

    [<ThriftMethod("NoReturn2")>]
    abstract NoReturn2: [<ThriftParameter(1s, "arg1")>] arg1: bool
                      * [<ThriftParameter(2s, "arg2")>] arg2: int -> Task

    [<ThriftMethod("NoReturn3")>]
    abstract NoReturn3: [<ThriftParameter(1s, "arg1")>] arg1: bool
                      * [<ThriftParameter(2s, "arg2")>] arg2: int
                      * [<ThriftParameter(3s, "arg3")>] arg3: double -> Task
    [<ThriftMethod("NoReturn4")>]
    abstract NoReturn4: [<ThriftParameter(1s, "arg1")>] arg1: bool 
                      * [<ThriftParameter(2s, "arg2")>] arg2: int
                      * [<ThriftParameter(3s, "arg3")>] arg3: double
                      * [<ThriftParameter(4s, "arg4")>] arg4: string -> Task

    [<ThriftMethod("NoReturn5")>]
    abstract NoReturn5: arg0: CancellationToken
                      * [<ThriftParameter(1s, "arg1")>] arg1: bool 
                      * [<ThriftParameter(2s, "arg2")>] arg2: int
                      * [<ThriftParameter(3s, "arg3")>] arg3: double
                      * [<ThriftParameter(4s, "arg4")>] arg4: string -> Task
                      
    [<ThriftMethod("NoReturnException")>]
    [<ThriftThrows(1s, "exn", typeof<CustomException>)>]
    abstract NoReturnException: unit -> Task

    [<ThriftMethod("Return0")>]
    abstract Return0: unit -> Task<int>

    [<ThriftMethod("Return1")>]
    abstract Return1: [<ThriftParameter(1s, "arg1")>] arg1: bool -> Task<int>

    [<ThriftMethod("Return2")>]
    abstract Return2: [<ThriftParameter(1s, "arg1")>] arg1: bool
                      * [<ThriftParameter(2s, "arg2")>] arg2: int -> Task<int>

    [<ThriftMethod("Return3")>]
    abstract Return3: [<ThriftParameter(1s, "arg1")>] arg1: bool
                      * [<ThriftParameter(2s, "arg2")>] arg2: int
                      * [<ThriftParameter(3s, "arg3")>] arg3: double -> Task<int>
    [<ThriftMethod("Return4")>]
    abstract Return4: [<ThriftParameter(1s, "arg1")>] arg1: bool 
                      * [<ThriftParameter(2s, "arg2")>] arg2: int
                      * [<ThriftParameter(3s, "arg3")>] arg3: double
                      * [<ThriftParameter(4s, "arg4")>] arg4: string -> Task<int>

    [<ThriftMethod("Return5")>]
    abstract Return5: arg0: CancellationToken
                      * [<ThriftParameter(1s, "arg1")>] arg1: bool 
                      * [<ThriftParameter(2s, "arg2")>] arg2: int
                      * [<ThriftParameter(3s, "arg3")>] arg3: double
                      * [<ThriftParameter(4s, "arg4")>] arg4: string -> Task<int>

    [<ThriftMethod("ReturnException")>]
    [<ThriftThrows(1s, "exn", typeof<CustomException>)>]
    abstract ReturnException: unit -> Task<int>

    [<ThriftMethod("ConvertedReturn", Converter = typeof<ThriftUnixDateConverter>)>]
    abstract ConvertedReturn: unit -> Task<System.DateTime>

    [<ThriftMethod("OneWay", IsOneWay = true)>]
    abstract OneWay: unit -> Task


[<AbstractClass>]
type Tests() =
    abstract GetService: (IThriftTransport -> IThriftProtocol) -> IService

    member x.Test call expectedWrite expectedRead expectedResult = asTask <| async {
        let mutable prot: IThriftProtocol = null
        let svc = x.GetService(fun t -> prot <- MemoryProtocol(expectedRead, t); prot)
        let! res = call(svc) |> Async.AwaitTask
        (prot :?> MemoryProtocol).WrittenValues <=> expectedWrite
        res <=> expectedResult
    }
    
    member x.TestException<'e when 'e :> exn> call expectedWrite expectedRead checker = asTask <| async {
        let mutable prot: IThriftProtocol = null
        let svc = x.GetService(fun t -> prot <- MemoryProtocol(expectedRead, t); prot)
        let! ex = Assert.ThrowsAnyAsync<'e>(fun() -> call(svc) :> Task) |> Async.AwaitTask
        (prot :?> MemoryProtocol).WrittenValues <=> expectedWrite
        checker(ex)
    }

    [<Fact>] 
    member x.``No return value, no args``() =
        x.Test (fun s -> s.NoReturn0() |> asUnit)
              [MessageHeader ("NoReturn0", ThriftMessageType.Call)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              [MessageHeader ("", ThriftMessageType.Reply)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              ()

    [<Fact>] 
    member x.``No return value, 1 arg``() =
        x.Test (fun s -> s.NoReturn1(true) |> asUnit)
               [MessageHeader ("NoReturn1", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               ()

    [<Fact>] 
    member x.``No return value, 2 args``() =
        x.Test (fun s -> s.NoReturn2(true, 1) |> asUnit)
               [MessageHeader ("NoReturn2", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               ()

    [<Fact>] 
    member x.``No return value, 3 args``() =
        x.Test (fun s -> s.NoReturn3(true, 1, 1.0) |> asUnit)
               [MessageHeader ("NoReturn3", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               ()

    [<Fact>] 
    member x.``No return value, 4 args``() =
        x.Test (fun s -> s.NoReturn4(true, 1, 1.0, "abc") |> asUnit)
               [MessageHeader ("NoReturn4", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldHeader (4s, "arg4", ThriftTypeId.Binary)
                String "abc"
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               ()

    [<Fact>] 
    member x.``No return value, 4 args + cancellation token (not cancelled)``() =
        x.Test (fun s -> s.NoReturn5(CancellationToken.None, true, 1, 1.0, "abc") |> asUnit)
               [MessageHeader ("NoReturn5", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldHeader (4s, "arg4", ThriftTypeId.Binary)
                String "abc"
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               ()

    [<Fact>] 
    member x.``No return value, 4 args + cancellation token (cancelled)``() =
        x.TestException<System.OperationCanceledException>
               (fun s -> s.NoReturn5(CancellationToken(true), true, 1, 1.0, "abc") |> asUnit)
               [MessageHeader ("NoReturn5", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldHeader (4s, "arg4", ThriftTypeId.Binary)
                String "abc"
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               []
               (fun _ -> ())
               
    [<Fact>]
    member x.``No return value, declared exception not thrown``() =
        x.Test (fun s -> s.NoReturnException() |> asUnit)
              [MessageHeader ("NoReturnException", ThriftMessageType.Call)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              [MessageHeader ("", ThriftMessageType.Reply)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              ()

    [<Fact>]
    member x.``No return value, declared exception thrown``() =
        x.TestException<CustomException> 
              (fun s -> s.NoReturnException() |> asUnit)
              [MessageHeader ("NoReturnException", ThriftMessageType.Call)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              [MessageHeader ("", ThriftMessageType.Reply)
               StructHeader ""
               FieldHeader (1s, "exn", ThriftTypeId.Struct)
               StructHeader ("CustomException")
               FieldHeader (1s, "Value", ThriftTypeId.Binary)
               String "Oops"
               FieldEnd
               FieldStop
               StructEnd
               FieldEnd
               FieldStop
               StructEnd
               MessageEnd]
              (fun e -> e.Value <=> "Oops")

    [<Fact>]
    member x.``No return value, undeclared exception thrown``() =
        x.TestException<ThriftProtocolException> 
               (fun s -> s.NoReturn0() |> asUnit)
               [MessageHeader ("NoReturn0", ThriftMessageType.Call)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Exception)
                StructHeader "TApplicationException"
                FieldHeader (1s, "message", ThriftTypeId.Binary)
                String "Error"
                FieldEnd
                FieldHeader (2s, "type", ThriftTypeId.Int32)
                Int32 6
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               (fun e -> e.Message <=> "Error"; e.ExceptionType <=> nullable ThriftProtocolExceptionType.InternalError)

    [<Fact>] 
    member x.``Return value, no args``() =
        x.Test (fun s -> s.Return0())
              [MessageHeader ("Return0", ThriftMessageType.Call)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              [MessageHeader ("", ThriftMessageType.Reply)
               StructHeader ""
               FieldHeader (0s, null, ThriftTypeId.Int32)
               Int32 42
               FieldEnd
               FieldStop
               StructEnd
               MessageEnd]
              42

    [<Fact>] 
    member x.``Return value, 1 arg``() =
        x.Test (fun s -> s.Return1(true))
               [MessageHeader ("Return1", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldHeader (0s, null, ThriftTypeId.Int32)
                Int32 42
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               42

    [<Fact>] 
    member x.``Return value, 2 args``() =
        x.Test (fun s -> s.Return2(true, 1))
               [MessageHeader ("Return2", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldHeader (0s, null, ThriftTypeId.Int32)
                Int32 42
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               42

    [<Fact>] 
    member x.``Return value, 3 args``() =
        x.Test (fun s -> s.Return3(true, 1, 1.0))
               [MessageHeader ("Return3", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldHeader (0s, null, ThriftTypeId.Int32)
                Int32 42
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               42

    [<Fact>] 
    member x.``Return value, 4 args``() =
        x.Test (fun s -> s.Return4(true, 1, 1.0, "abc"))
               [MessageHeader ("Return4", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldHeader (4s, "arg4", ThriftTypeId.Binary)
                String "abc"
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldHeader (0s, null, ThriftTypeId.Int32)
                Int32 42
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               42

    [<Fact>] 
    member x.``Return value, 4 args + cancellation token (not cancelled)``() =
        x.Test (fun s -> s.Return5(CancellationToken.None, true, 1, 1.0, "abc"))
               [MessageHeader ("Return5", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldHeader (4s, "arg4", ThriftTypeId.Binary)
                String "abc"
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldHeader (0s, null, ThriftTypeId.Int32)
                Int32 42
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               42

    [<Fact>] 
    member x.``Return value, 4 args + cancellation token (cancelled)``() =
        x.TestException<System.OperationCanceledException>
               (fun s -> s.Return5(CancellationToken(true), true, 1, 1.0, "abc") |> asUnit)
               [MessageHeader ("Return5", ThriftMessageType.Call)
                StructHeader ""
                FieldHeader (1s, "arg1", ThriftTypeId.Boolean)
                Bool true
                FieldEnd
                FieldHeader (2s, "arg2", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldHeader (3s, "arg3", ThriftTypeId.Double)
                Double 1.0
                FieldEnd
                FieldHeader (4s, "arg4", ThriftTypeId.Binary)
                String "abc"
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               []
               (fun _ -> ())

    [<Fact>]
    member x.``Return value, undeclared exception thrown``() =
        x.TestException<ThriftProtocolException> 
               (fun s -> s.Return0() |> asUnit)
               [MessageHeader ("Return0", ThriftMessageType.Call)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Exception)
                StructHeader "TApplicationException"
                FieldHeader (1s, "message", ThriftTypeId.Binary)
                String "Error"
                FieldEnd
                FieldHeader (2s, "type", ThriftTypeId.Int32)
                Int32 6
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               (fun e -> e.Message <=> "Error"; e.ExceptionType <=> nullable ThriftProtocolExceptionType.InternalError)
               
    [<Fact>]
    member x.``Return value, declared exception not thrown``() =
        x.Test (fun s -> s.ReturnException())
              [MessageHeader ("ReturnException", ThriftMessageType.Call)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              [MessageHeader ("", ThriftMessageType.Reply)
               StructHeader ""
               FieldHeader (0s, null, ThriftTypeId.Int32)
               Int32 1
               FieldEnd
               FieldStop
               StructEnd
               MessageEnd]
              1

    [<Fact>]
    member x.``Return value, declared exception thrown``() =
        x.TestException<CustomException> 
              (fun s -> s.ReturnException() |> asUnit)
              [MessageHeader ("ReturnException", ThriftMessageType.Call)
               StructHeader ""
               FieldStop
               StructEnd
               MessageEnd]
              [MessageHeader ("", ThriftMessageType.Reply)
               StructHeader ""
               FieldHeader (1s, "exn", ThriftTypeId.Struct)
               StructHeader ("CustomException")
               FieldHeader (1s, "Value", ThriftTypeId.Binary)
               String "Oops"
               FieldEnd
               FieldStop
               StructEnd
               FieldEnd
               FieldStop
               StructEnd
               MessageEnd]
              (fun e -> e.Value <=> "Oops")
    [<Fact>]
    member x.``Converted return value``() =
        x.Test (fun s -> s.ConvertedReturn())
               [MessageHeader ("ConvertedReturn", ThriftMessageType.Call)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               [MessageHeader ("", ThriftMessageType.Reply)
                StructHeader ""
                FieldHeader (0s, "", ThriftTypeId.Int32)
                Int32 787708800
                FieldEnd
                FieldStop
                StructEnd
                MessageEnd]
               (date(18, 12, 1994))

    [<Fact>]
    member x.``One-way method``() =
        x.Test (fun s -> s.OneWay() |> asUnit)
               [MessageHeader ("OneWay", ThriftMessageType.OneWay)
                StructHeader ""
                FieldStop
                StructEnd
                MessageEnd]
               []
               ()

    [<Fact>]
    member x.``Null parameter``() =
        x.TestException<ThriftSerializationException>
               (fun s -> s.NoReturn4(false, 0, 0.0, null) |> asUnit)
               []
               []
               (fun e -> Assert.Contains("Parameter 'arg4' was null", e.Message))
  
      
open System
type ServiceImpl(prot) =
    inherit ThriftServiceImplementation<IService>(ThriftCommunication.UsingCustomProtocol(Func<IThriftTransport, IThriftProtocol>(prot))
                                                                     .UsingCustomTransport(fun t -> MemoryTransport([], t) :> IThriftTransport))

    // Mimics the C# expression `x => x.Method`
    // Not very pretty, but it works.
    member __.E<'f>(name) =
        let param = Expression.Parameter(typeof<IService>)
        let meth = param.Type.GetMethod(name)
        let call = Expression.Call(Expression.Constant(meth), "CreateDelegate", [| |], 
                                [| Expression.Constant(typeof<'f>) :> Expression; param :> Expression |])
        let conv = Expression.Convert(call, typeof<'f>)
        Expression.Lambda<Func<IService, 'f>>(conv, param)

    interface IService with
        member x.NoReturn0() = base.CallAsync(x.E<Func<Task>>("NoReturn0"))
        member x.NoReturn1(a1) = base.CallAsync<bool>(x.E<Func<bool, Task>>("NoReturn1"), a1)
        member x.NoReturn2(a1,a2) = base.CallAsync<bool, int>(x.E<Func<bool, int, Task>>("NoReturn2"), a1, a2)
        member x.NoReturn3(a1,a2,a3) = base.CallAsync<bool, int, double>(x.E<Func<bool, int, double, Task>>("NoReturn3"), a1, a2, a3)
        member x.NoReturn4(a1,a2,a3,a4) = base.CallAsync<bool, int, double, string>(x.E<Func<bool, int, double, string, Task>>("NoReturn4"), a1, a2, a3, a4)
        member x.NoReturn5(a1,a2,a3,a4,a5) = base.CallAsync("NoReturn5", a1, a2, a3, a4, a5)
        member x.NoReturnException() = base.CallAsync(x.E<Func<Task>>("NoReturnException"))
        member x.Return0() = base.CallAsync<int>(x.E<Func<Task<int>>>("Return0"))
        member x.Return1(a1) = base.CallAsync<bool, int>(x.E<Func<bool, Task<int>>>("Return1"), a1)
        member x.Return2(a1,a2) = base.CallAsync<bool, int, int>(x.E<Func<bool, int, Task<int>>>("Return2"), a1, a2)
        member x.Return3(a1,a2,a3) = base.CallAsync<bool, int, double, int>(x.E<Func<bool, int, double, Task<int>>>("Return3"), a1, a2, a3)
        member x.Return4(a1,a2,a3,a4) = base.CallAsync<bool, int, double, string, int>(x.E<Func<bool, int, double, string, Task<int>>>("Return4"), a1, a2, a3, a4)
        member x.Return5(a1,a2,a3,a4,a5) = base.CallAsync<int>("Return5", a1, a2, a3, a4, a5)
        member x.ReturnException() = base.CallAsync<int>(x.E<Func<Task<int>>>("ReturnException"))
        member x.ConvertedReturn() = base.CallAsync<DateTime>(x.E<Func<Task<DateTime>>>("ConvertedReturn"))
        member x.OneWay() =  base.CallAsync(x.E<Func<Task>>("OneWay"))

type ServiceImplementation() =
    inherit Tests()

    override __.GetService prot =
        ServiceImpl(prot) :> IService

type Proxy() =
    inherit Tests()

    override __.GetService prot =
        ThriftProxy.Create<IService>(ThriftCommunication.UsingCustomProtocol(Func<IThriftTransport, IThriftProtocol>(prot))
                                                        .UsingCustomTransport(fun t -> MemoryTransport([], t) :> IThriftTransport))