// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Tests.``Parsing services``

open System
open System.Reflection
open System.Threading
open System.Threading.Tasks
open Xunit
open ThriftSharp
open ThriftSharp.Internals
open ThriftSharp.Utilities
open ThriftSharp.Tests

module ``Parsing: Converters`` =
    let throws<'T>(expected) =
        let exn = Assert.Throws<ArgumentException>(fun () -> ThriftParameterAttribute(0s, "param", Converter = typeof<'T>) |> ignore)
        Assert.Contains(expected, exn.Message)

    [<Fact>]
    let ``Converter not implementing IThriftValueConverter<,> is not allowed``() =
        throws<string>("The type must implement IThriftValueConverter")


    type BadConverter(notAParameterlessCtor: obj) =
        interface IThriftValueConverter<int, int> with
            member x.Convert(i) = i
            member x.ConvertBack(i) = i

    [<Fact>]
    let ``Converter without a parameterless ctor is not allowed``() =
        throws<BadConverter>("The type must have a parameterless constructor.")

        
    type GoodConverter() =
        interface IThriftValueConverter<int, int> with
            member x.Convert(i) = i
            member x.ConvertBack(i) = i

    [<Fact>]
    let ``Converter getter works properly``() =
        let attribute = ThriftParameterAttribute(0s, "param", Converter=typeof<GoodConverter>)
        attribute.Converter <=> typeof<GoodConverter>

module ``Parsing services: Normal methods`` =
    [<ThriftStruct("CustomStruct")>]
    type CustomStruct() = class end

    let test(args: (Type * Type) list, retType: Type * Type, isOneWay: bool) =
        let serviceAttrs = [{ typ = typeof<ThriftServiceAttribute>; args = ["Service"]; namedArgs = [] }]
        let methodAttrs = [{ typ = typeof<ThriftMethodAttribute>; args = ["Method"]; namedArgs = ["IsOneWay", box isOneWay; "Converter", box (snd retType)] }]
        let methodArgs = args |> List.mapi (fun n (t, c) -> (t, [{ typ = typeof<ThriftParameterAttribute>; args = [int16 n; n.ToString()]; namedArgs = ["Converter", box c] }]))
        let iface = makeInterface serviceAttrs [(methodArgs, fst retType, methodAttrs)]

        let service = ThriftAttributesParser.ParseService iface
        
        let expectedMethod =  
            new ThriftMethod(
                "Method", 
                isOneWay, 
                new ThriftReturnValue(ReflectionExtensions.UnwrapTask(fst retType).GetTypeInfo(), if snd retType = null then null else Activator.CreateInstance(snd retType)), 
                [| |], 
                args |> List.mapi (fun n (t, c) -> new ThriftParameter(int16 n, 
                                                                       n.ToString(), 
                                                                       t.GetTypeInfo(), 
                                                                       if c = null then null else Activator.CreateInstance(c))) |> List.toArray)

        // makeInterface names methods with a counter
        service <=> new ThriftService("Service", dict [("0", expectedMethod)])

    [<Theory;
      InlineData(typeof<Task>)>]
    let ``One-way without parameters``(retType) =
        test([], (retType, null), true)

    [<Theory;
      MemberData("ParametersData")>]
    let ``One-way with parameters``(types) =
        test(List.map (fun t -> t, null) types, (typeof<Task>, null), true)

    [<Theory;
      InlineData(typeof<Task>);
      InlineData(typeof<Task<int>>);
      InlineData(typeof<Task<string>>);
      InlineData(typeof<Task<CustomStruct>>)>]
    let ``Two-way without parameters``(retType) =
        test([], (retType, null), false)
        
    [<Theory;
      MemberData("ParametersData")>]
    let ``Two-way with parameters``(types) =
        test(List.map (fun t -> t, null) types, (typeof<Task<int>>, null), false)
        
    [<Theory;
      InlineData(1);
      InlineData(2);
      InlineData(10)>]
    let ``Converter on parameters``(count) =
        test(List.init count (fun _ -> (typeof<DateTime>, typeof<ThriftJavaDateConverter>)), (typeof<Task>, null), false)
        
    [<Fact>]
    let ``Converter on the return value``() =
        test([], (typeof<Task<DateTime>>, typeof<ThriftJavaDateConverter>), false)
        
    [<Fact>]
    let ``Converter on a parameter and the return value``() =
        test([(typeof<DateTimeOffset>, typeof<ThriftJavaDateOffsetConverter>)], 
             (typeof<Task<DateTime>>, typeof<ThriftJavaDateConverter>), 
             false)


    let ParametersData() =
         [| [| [ typeof<int> ] |]; 
            [| [ typeof<string> ] |]; 
            [| [ typeof<CustomStruct> ] |]; 
            [| [ typeof<int>; typeof<int> ] |]; 
            [| [ typeof<int>; typeof<string> ] |]; 
            [| [ typeof<int>; typeof<int>; typeof<int> ] |]; 
            [| [ typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int> ] |]; 
            [| [ typeof<int>; typeof<string>; typeof<CustomStruct> ] |] |]

module ``Parsing services: Methods with exceptions`` =
    [<ThriftStruct("CustomStruct")>]
    type CustomStruct() = class end
    
    [<ThriftStruct("CustomException")>]
    type CustomException() = inherit Exception()
    
    type StructToExceptionConverter() =
        interface IThriftValueConverter<CustomStruct, CustomException> with
            member x.Convert(_) = CustomException() 
            member x.ConvertBack(_) = CustomStruct()


    let makeIface (exns: (Type * Type) list) (isOneWay: bool) =
        let serviceAttrs = [{ typ = typeof<ThriftServiceAttribute>; args = ["Service"]; namedArgs = [] }]
        let methodAttrs = 
            { typ = typeof<ThriftMethodAttribute>; args = ["Method"]; namedArgs = ["IsOneWay", box isOneWay] }
         :: (exns |> List.mapi (fun n (e, c) -> { typ = typeof<ThriftThrowsAttribute>; args = [int16 n; n.ToString(); e]; namedArgs = ["Converter", box c]}))
        makeInterface serviceAttrs [([], typeof<Task>, methodAttrs)]


    [<Theory;
      InlineData(typeof<CustomException>, null);
      InlineData(typeof<CustomException>, typeof<StructToExceptionConverter>)>]
    let ``Normal exceptions``(exnType, convType) =
        let iface = makeIface [exnType, convType] false
        let service = ThriftAttributesParser.ParseService iface

        let expectedMethod =  
            new ThriftMethod(
                "Method", 
                false, 
                new ThriftReturnValue(typeof<Void>.GetTypeInfo(), null), 
                [| ThriftThrowsClause(0s, "0", exnType.GetTypeInfo(), if convType = null then null else Activator.CreateInstance(convType)) |],
                [| |])

        // makeInterface names methods with a counter
        service <=> new ThriftService("Service", dict [("0", expectedMethod)])

    [<Theory;
      InlineData(typeof<int>);
      InlineData(typeof<string>);
      InlineData(typeof<CustomStruct>)>]
    let ``Non-exception structs are not allowed``(typ) =
        let iface = makeIface [typeof<CustomStruct>, null] true
        let exn = Assert.Throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseService iface |> ignore)
        Assert.Contains("does not inherit from Exception.", exn.Message)
      
    [<Fact>]  
    let ``Exceptions on one-way method are not allowed``() =
        let iface = makeIface [typeof<CustomException>, null] true
        let exn = Assert.Throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseService iface |> ignore)
        Assert.Contains("One-way methods cannot throw exceptions", exn.Message)

module ``Parsing services: Bad services`` =
    let throws<'T>(expected) =
        let exn = Assert.Throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo()) |> box)
        Assert.Contains(expected, exn.Message)
        

    [<ThriftService("NotAService")>]
    type NotAnInterface() = class end

    [<Fact>]
    let ``Not an interface``() =
        throws<NotAnInterface>("The type 'NotAnInterface' is not an interface")
        

    type UnmarkedService =
        [<ThriftMethod("test")>]
        abstract Test: unit -> Task<int>

    [<Fact>]
    let ``Unmarked service``() =
        throws<UnmarkedService>("The interface 'UnmarkedService' does not have a Thrift interface definition")

        
    [<ThriftService("EmptyService")>]
    type EmptyService = interface end

    [<Fact>]
    let ``Empty service``() =
        throws<EmptyService>("The interface 'EmptyService' does not have any methods")


    [<ThriftService("UnmarkedMethod")>]
    type ServiceWithUnmarkedMethod =
        [<ThriftMethod("test")>]
        abstract Test1: unit -> Task<int>
        abstract Test2: unit -> Task<int>

    [<Fact>]
    let ``Unmarked method``() =
        throws<ServiceWithUnmarkedMethod>("The method 'Test2' (in type 'ServiceWithUnmarkedMethod') is not part of the Thrift interface.")


    [<ThriftService("UnmarkedParameter")>]
    type ServiceWithUnmarkedParameter =
        [<ThriftMethod("test")>]
        abstract Test: param: int -> Task<int>

    [<Fact>]
    let ``Unmarked parameter``() =
        throws<ServiceWithUnmarkedParameter>("Parameter 'param' of method 'Test' (in type 'ServiceWithUnmarkedParameter') does not have a Thrift interface definition")

        
    [<ThriftService("TooManyTokens")>]
    type ServiceWithTooManyTokens =
        [<ThriftMethod("test")>]
        abstract Test: CancellationToken * CancellationToken -> Task<int>

    [<Fact>]
    let ``Too many cancellation tokens``() =
        throws<ServiceWithTooManyTokens>("The method 'Test' (in type 'ServiceWithTooManyTokens') has more than one CancellationToken parameter")

module ``Parsing services: Bad return types`` =
    [<ThriftStruct("CustomStruct")>]
    type CustomStruct() = class end

    let shouldFail(meth) =
        let serviceAttrs = [{ typ = typeof<ThriftServiceAttribute>; args = ["Service"]; namedArgs = [] }]
        let iface = makeInterface serviceAttrs [meth]
    
        Assert.Throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseService iface |> ignore)   
    
    [<Theory;
      InlineData(typeof<Void>);
      InlineData(typeof<int>);
      InlineData(typeof<string>);
      InlineData(typeof<CustomStruct>)>]
    let ``One-way (sync)``(typ) =
        let methodAttrs = [{ typ = typeof<ThriftMethodAttribute>; args = ["Method"]; namedArgs = ["IsOneWay", box true] }]
    
        let exn = shouldFail ([], typ, methodAttrs)
        Assert.Contains("Only asynchronous calls are supported.", exn.Message)
    
    [<Theory;
      InlineData(typeof<Task<int>>);
      InlineData(typeof<Task<string>>);
      InlineData(typeof<Task<CustomStruct>>)>]
    let ``One-way (async)``(typ) =
        let methodAttrs = [{ typ = typeof<ThriftMethodAttribute>; args = ["Method"]; namedArgs = ["IsOneWay", box true] }]
    
        let exn = shouldFail ([], typ, methodAttrs)
        Assert.Contains("One-way methods cannot return a value.", exn.Message)
    
    [<Theory;
      InlineData(typeof<Void>);
      InlineData(typeof<int>);
      InlineData(typeof<string>);
      InlineData(typeof<CustomStruct>)>]
    let ``Two-way``(typ) =
        let methodAttrs = [{ typ = typeof<ThriftMethodAttribute>; args = ["Method"]; namedArgs = [] }]
    
        let exn = shouldFail ([], typ, methodAttrs)
        Assert.Contains("Only asynchronous calls are supported.", exn.Message)