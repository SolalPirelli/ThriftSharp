// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Parsing services``

open System
open System.Linq
open System.Reflection
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Quotations
open ThriftSharp
open ThriftSharp.Internals
open ThriftSharp.Utilities

// Special types

[<ThriftStruct("CustomException")>]
type CustomException() = inherit Exception()

[<ThriftStruct("CustomException2")>]
type CustomException2() = inherit Exception()

[<ThriftStruct("NotException")>]
type NotException() = class end


// Special services
[<ThriftService("NotAService")>]
type NotAnInterface() = class end

type UnmarkedService =
    [<ThriftMethod("test")>]
    abstract Test: unit -> Task<int>

[<ThriftService("EmptyService")>]
type EmptyService = interface end

[<ThriftService("UnmarkedMethod")>]
type ServiceWithUnmarkedMethod =
    [<ThriftMethod("test")>]
    abstract Test1: unit -> Task<int>

    abstract Test2: unit -> Task<int>
    
[<ThriftService("UnmarkedMethodParameter")>]
type ServiceWithUnmarkedMethodParameter =
    [<ThriftMethod("test")>]
    abstract Test: int -> Task<int>

[<ThriftService("ConvertedReturnValue")>]
type ServiceWithConvertedReturnValue =
    [<ThriftMethod("test")>]
    abstract Test: unit -> [<ThriftConverter(typeof<ThriftUnixDateConverter>)>] Task<int>

[<ThriftService("TooManyTokens")>]
type ServiceWithTooManyTokens =
    [<ThriftMethod("test")>]
    abstract Test: CancellationToken * CancellationToken -> Task<int>


let parse<'T> =
    ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())

let ok argTypes retType isOneWay thrownExns =
    let name = "Service"
    let methodName = "Method"
    let throwsClauses = thrownExns 
                     |> List.mapi (fun j exn -> <@ ThriftThrowsAttribute(int16 j, string j, exn) @> )
                     |> List.map (fun o -> o :> Expr)

    let methodArgs = argTypes |> Array.ofList
                              |> Array.mapi (fun i typ -> typ, [ <@ ThriftParameterAttribute(int16 i, string i) @> ])
    
    let iface = makeInterface [ <@ ThriftServiceAttribute(name) @> ] [methodArgs, retType, <@ ThriftMethodAttribute(methodName, isOneWay) @> :> Expr :: throwsClauses] 
    let thriftService = ThriftAttributesParser.ParseService(iface.GetTypeInfo())

    thriftService.Name <=> name
    
    thriftService.Methods.Count <=> 1
    let meth = thriftService.Methods.First()
    meth.Key <=> iface.GetMethods().[0].Name
    meth.Value.Name <=> methodName
    meth.Value.Parameters |> List.ofSeq |> List.map (fun p -> p.UnderlyingTypeInfo.AsType()) <=> argTypes
    meth.Value.ReturnValue.UnderlyingTypeInfo.AsType() <=> ReflectionExtensions.UnwrapTask( retType )
    meth.Value.IsOneWay <=> isOneWay
    meth.Value.Exceptions |> List.ofSeq |> List.map (fun e -> e.Type.TypeInfo.AsType()) <=> thrownExns

let fails argTypes retType isOneWay thrownExns =
    let throwsClauses = thrownExns 
                     |> List.mapi (fun j exn -> <@ ThriftThrowsAttribute(int16 j, string j, exn) @> )
                     |> List.map (fun o -> o :> Expr)

    let methodArgs = argTypes |> Array.ofList
                              |> Array.mapi (fun i typ -> typ, [ <@ ThriftParameterAttribute(int16 i, string i) @> ])
    
    let iface = makeInterface [ <@ ThriftServiceAttribute("Service") @> ] [methodArgs, retType, <@ ThriftMethodAttribute("Method", isOneWay) @> :> Expr :: throwsClauses] 
    throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseService(iface.GetTypeInfo()) |> box) |> ignore

let failsOn<'T> =
    throws<ThriftParsingException> (fun () -> ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo()) |> box) |> ignore



[<TestClass>]
type __() =
    // Errors should be thrown when parsing services that are not interfaces
    [<Test>] member __.``Error when service isn't an interface``() = failsOn<NotAnInterface>

    // Errors should be thrown when parsing interfaces that are not Thrift services``() 
    [<Test>] member __.``Error when interface isn't a service``() = failsOn<UnmarkedService>

    // Errors should be thrown when parsing services without methods
    [<Test>] member __.``Error on no methods``() = failsOn<EmptyService>

    // Errors should be thrown when parsing services with non-Thrift methods
    [<Test>] member __.``Error on non-Thrift methods``() = failsOn<ServiceWithUnmarkedMethod>
    
    // Errors should be thrown when parsing services with methods with unmarked parameters
    [<Test>] member __.``Error on unmarked method parameter``() = failsOn<ServiceWithUnmarkedMethodParameter>
    
    // Errors should be thrown when parsing methods with more than one cancellation token
    [<Test>] member __.``Error on >1 cancellation tokens``() = failsOn<ServiceWithTooManyTokens>

    // Services with two-way methods should be parsed correctly
    [<Test>] member __.``Method with no return value and no parameters``() = ok [] typeof<Task> false []
    [<Test>] member __.``Method with int return value and no parameters``() = ok [] typeof<Task<int>> false []
    [<Test>] member __.``Method with binary return value and no parameters``() = ok [] typeof<Task<sbyte[]>> false []
    [<Test>] member __.``Method with struct return value and no parameters``() = ok [] typeof<Task<CustomException>> false []
    [<Test>] member __.``Method with no return value and 1 int parameter``() = ok [typeof<int>] typeof<Task> false []
    [<Test>] member __.``Method with no return value and 1 string parameter``() = ok [typeof<string>] typeof<Task> false []
    [<Test>] member __.``Method with no return value and 1 struct parameter``() = ok [typeof<CustomException>] typeof<Task> false []
    [<Test>] member __.``Method with no return value and 3 parameters``() = ok [typeof<bool>; typeof<CustomException>; typeof<int[]>] typeof<Task> false []

    // Errors should be thrown when parsing services with non-Task-returning methods
    [<Test>] member __.``Error on method with no return type``() =     fails [] typeof<System.Void> false []
    [<Test>] member __.``Error on method with int return type``() =    fails [] typeof<int> false []
    [<Test>] member __.``Error on method with string return type``() = fails [] typeof<string> false []
    [<Test>] member __.``Error on method with struct return type``() = fails [] typeof<CustomException> false []

    // Services with one-way methods should be parsed correctly
    [<Test>] member __.``One-way method with Task return type``() = ok [] typeof<Task> true []

    // Errors should be thrown when parsing services with one-way methods that do not return void
    [<Test>] member __.``Error on one-way method with Task<T> return type``() = fails [] typeof<Task<int>> true []

    // Services with methods that throw should be parsed correctly
    [<Test>] member __.``Method with one thrown exception``() = ok [] typeof<Task> false [typeof<CustomException>]
    [<Test>] member __.``Method with two thrown exceptions``() = ok [] typeof<Task> false [typeof<CustomException>; typeof<CustomException2>]

    // Errors should be thrown when parsing services with methods that throw non-Exceptions
    [<Test>] member __.``Error on non-Exception thrown class``() = fails [] typeof<Task> false [typeof<NotException>]

    // Errors should be thrown when parsing services with one-way methods that throw exceptions
    [<Test>] member __.``Error on one-way method with thrown exceptions``() = fails [] typeof<Task> true [typeof<CustomException>]

    // Services with methods whose return value has a converter should be parsed correctly
    [<Test>] 
    member __.``Method with converted return value``() =
        let thriftService = parse<ServiceWithConvertedReturnValue>

        thriftService.Methods.Count <=> 1
        let meth = thriftService.Methods.First()
        meth.Value.ReturnValue.Converter.GetType() <=> typeof<ThriftUnixDateConverter>