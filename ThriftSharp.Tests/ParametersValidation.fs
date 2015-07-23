module ThriftSharp.Tests.``Parameters validation``

open System
open System.Linq.Expressions
open System.Threading.Tasks
open Xunit
open ThriftSharp

let fails func =
    Assert.ThrowsAny<ArgumentException>(func >> ignore) |> ignore
    

let StringData =
    [| [| null |]; [| "" |]; [| " \t \n" |] |]

[<Theory;
  MemberData("StringData")>]
let ``ThriftFieldAttribute: name``(value) =
    fails (fun () -> ThriftFieldAttribute(1s, false, value))
    

[<Theory;
  MemberData("StringData")>]
let ``ThriftStructAttribute: name``(value) =
    fails (fun () -> ThriftStructAttribute(value))


[<Theory;
  MemberData("StringData")>]
let ``ThriftParameterAttribute: name``(value) =
    fails (fun () -> ThriftParameterAttribute(1s, value))


[<ThriftStruct("CustomException")>]
type CustomException() = inherit exn()

[<Theory;
  MemberData("StringData")>]
let ``ThriftThrowsAttribute: name``(value) =
    fails (fun () -> ThriftThrowsAttribute(1s, value, typeof<CustomException>))
    

[<Theory;
  MemberData("StringData")>]
let ``ThriftMethodAttribute: name``(value) =
    fails (fun () -> ThriftMethodAttribute(value))


[<Theory;
  MemberData("StringData")>]
let ``ThriftServiceAttribute: name``(value) =
    fails (fun () -> ThriftServiceAttribute(value))


[<Theory;
  MemberData("StringData")>]
let ``ThriftCommunication#OverHttp: url``(value) =
    fails (fun () -> ThriftCommunication.Binary().OverHttp(value))


[<ThriftService("CustomService")>]
type ICustomService =
    [<ThriftMethod("NoReturn")>]
    abstract NoReturn: unit -> Task
    [<ThriftMethod("Return")>]
    abstract Return: unit -> Task<int>

type CustomServiceNullToCtor() =
    inherit ThriftServiceImplementation<ICustomService>(null)

    interface ICustomService with
        member x.NoReturn() = failwith "oops"
        member x.Return() = failwith "oops"

[<Fact>]
let ``ThriftServiceImplementation: communication``() =
    fails CustomServiceNullToCtor


type CustomService(value: string) =
    inherit ThriftServiceImplementation<ICustomService>({ new ThriftCommunication() with member x.CreateProtocol(_) = null })

    interface ICustomService with
        member x.NoReturn() =
            x.CallAsync(value)

        member x.Return() =
            x.CallAsync<int>(value)
          
[<Theory;
  MemberData("StringData")>]
let ``ThriftServiceImplementation#CallAsync: name``(value) =
    fails ((CustomService(value) :> ICustomService).NoReturn)

[<Theory;
  MemberData("StringData")>]
let ``ThriftServiceImplementation#CallAsync<T>: name``(value) =
    fails ((CustomService(value) :> ICustomService).Return)


type CustomServiceNullArgs() =
    inherit ThriftServiceImplementation<ICustomService>({ new ThriftCommunication() with member x.CreateProtocol(_) = null })

    interface ICustomService with
        member x.NoReturn() =
            x.CallAsync("NoReturn", Unchecked.defaultof<obj[]>)

        member x.Return() =
            x.CallAsync<int>("Return", Unchecked.defaultof<obj[]>)
          
[<Fact>]
let ``ThriftServiceImplementation#CallAsync: args``() =
    fails ((CustomServiceNullArgs() :> ICustomService).NoReturn)

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T>: args``() =
    fails ((CustomServiceNullArgs() :> ICustomService).Return)


[<ThriftService("Service2")>]
type ICustomService2 =
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

type CustomService2() =
    inherit ThriftServiceImplementation<ICustomService2>({ new ThriftCommunication() with member x.CreateProtocol(_) = null })

    member x.E<'e>() =
        Unchecked.defaultof<Expression<Func<ICustomService2, 'e>>>

    interface ICustomService2 with
        member x.NoReturn0() = base.CallAsync(x.E<Func<Task>>())
        member x.NoReturn1(a1) = base.CallAsync<bool>(x.E<Func<bool, Task>>(), a1)
        member x.NoReturn2(a1,a2) = base.CallAsync<bool, int>(x.E<Func<bool, int, Task>>(), a1, a2)
        member x.NoReturn3(a1,a2,a3) = base.CallAsync<bool, int, double>(x.E<Func<bool, int, double, Task>>(), a1, a2, a3)
        member x.NoReturn4(a1,a2,a3,a4) = base.CallAsync<bool, int, double, string>(x.E<Func<bool, int, double, string, Task>>(), a1, a2, a3, a4)
        member x.Return0() = base.CallAsync<int>(x.E<Func<Task<int>>>())
        member x.Return1(a1) = base.CallAsync<bool, int>(x.E<Func<bool, Task<int>>>(), a1)
        member x.Return2(a1,a2) = base.CallAsync<bool, int, int>(x.E<Func<bool, int, Task<int>>>(), a1, a2)
        member x.Return3(a1,a2,a3) = base.CallAsync<bool, int, double, int>(x.E<Func<bool, int, double, Task<int>>>(), a1, a2, a3)
        member x.Return4(a1,a2,a3,a4) = base.CallAsync<bool, int, double, string, int>(x.E<Func<bool, int, double, string, Task<int>>>(), a1, a2, a3, a4)

          
[<Fact>]
let ``ThriftServiceImplementation#CallAsync: expr``() =
    fails ((CustomService2() :> ICustomService2).NoReturn0)
    
[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).NoReturn1(false))

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T1, T2>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).NoReturn2(false, 0))

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T1, T2, T3>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).NoReturn3(false, 0, 0.0))

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T1, T2, T3, T4>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).NoReturn4(false, 0, 0.0, ""))

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<TReturn>: expr``() =
    fails ((CustomService2() :> ICustomService2).Return0)
    
[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T, TReturn>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).Return1(false))

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T1, T2, TResult>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).Return2(false, 0))

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T1, T2, T3, TResult>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).Return3(false, 0, 0.0))

[<Fact>]
let ``ThriftServiceImplementation#CallAsync<T1, T2, T3, T4, TResult>: expr``() =
    fails (fun () -> (CustomService2() :> ICustomService2).Return4(false, 0, 0.0, ""))
