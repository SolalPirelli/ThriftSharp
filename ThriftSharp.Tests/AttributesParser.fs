﻿// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Tests.``Parsing services``

open System
open System.Collections
open System.Collections.Generic
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
      MemberData("Parameters")>]
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
      MemberData("Parameters")>]
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


    let Parameters() =
         [ [ typeof<int> ]
           [ typeof<string> ]
           [ typeof<CustomStruct> ]
           [ typeof<int>; typeof<int> ]
           [ typeof<int>; typeof<string> ]
           [ typeof<int>; typeof<int>; typeof<int> ]
           [ typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int>; typeof<int> ]
           [ typeof<int>; typeof<string>; typeof<CustomStruct> ] ] |> List.map Array.singleton

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

    let shouldFail meth =
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

module ``Parsing structs: Normal fields`` =
    let makeStruct fieldType isReq =
        makeClass [{ typ = typeof<ThriftStructAttribute>; args = ["Struct"]; namedArgs = [] }] 
                  [fieldType, [{typ = typeof<ThriftFieldAttribute>; args = [0s; isReq; "Field"]; namedArgs = [] }]]
    
    let test fieldType isReq =
        let genType = makeStruct fieldType isReq
        let expected = ThriftStruct(ThriftStructHeader("Struct"), [| ThriftField(0s, "Field", isReq, null, null, genType.GetProperty("0")) |], genType)
        
        ThriftAttributesParser.ParseStruct(genType.GetTypeInfo()) <=> expected

    let throws fieldType isReq expected =
        let genType = makeStruct fieldType isReq
        let exn = Assert.Throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseStruct genType |> ignore)
        Assert.Contains(expected, exn.Message)

    [<ThriftEnum>]
    type CustomEnum = A = 1 | B = 2

    let ValuePrimitives() =
        [typeof<bool>
         typeof<sbyte>
         typeof<int16>
         typeof<int32>
         typeof<int64>
         typeof<double>
         typeof<CustomEnum>] |> List.map (box >> Array.singleton)

    let ClassPrimitives() =
        [typeof<string>
         typeof<sbyte[]>] |> List.map (box >> Array.singleton)
         
    type CustomList<'T>() = inherit List<'T>()
    type CustomSet<'T>() = inherit HashSet<'T>()
    type CustomDictionary<'K,'V when 'K: equality>() = inherit Dictionary<'K,'V>()

    let Collections() =
        [typeof<int[]>
         typeof<IList<int>>
         typeof<List<int>>
         typeof<CustomList<int>>
         typeof<ISet<int>>
         typeof<HashSet<int>>
         typeof<CustomSet<int>>
         typeof<IDictionary<int,int>>
         typeof<Dictionary<int,int>>
         typeof<CustomDictionary<int,int>>] |> List.map (box >> Array.singleton)
        
    [<Theory;
      MemberData("ValuePrimitives")>]
    let ``Required value primitive fields``(typ) =
        test typ true
        
    [<Theory;
      MemberData("ClassPrimitives")>]
    let ``Required class primitive fields``(typ) =
        test typ true
        
    [<Theory;
      MemberData("Collections")>]
    let ``Required collection fields``(typ) =
        test typ true

    [<Theory;
      MemberData("ValuePrimitives")>]
    let ``Optional nullable value primitive fields``(typ) =
        test (typedefof<Nullable<_>>.MakeGenericType([| typ |])) false

    [<Theory;
      MemberData("ClassPrimitives")>]
    let ``Optional class primitive fields``(typ) =
        test typ false

    [<Theory;
      MemberData("Collections")>]
    let ``Optional collection fields``(typ) =
        test typ false

    [<Theory;
      MemberData("ValuePrimitives")>]
    let ``Optional value primitive fields are not allowed``(typ) =
        throws typ false "The Thrift field '0' (in type 'GeneratedType') is optional, but its type is a value type"

    [<Theory;
      MemberData("ValuePrimitives")>]
    let ``Required nullable fields are not allowed``(typ) =
        throws (typedefof<Nullable<_>>.MakeGenericType([| typ |])) true 
               "The Thrift field '0' (in type 'GeneratedType') is required, but its type is nullable"
        
module ``Parsing structs: Bad field types`` =
    let makeStruct fieldType =
        makeClass [{ typ = typeof<ThriftStructAttribute>; args = ["Struct"]; namedArgs = [] }]
                  [fieldType, [{ typ = typeof<ThriftFieldAttribute>; args = [0s; true; "Field"]; namedArgs = [] } ]]

    let throws fieldType expected =
        let typ = makeStruct fieldType
        let exn = Assert.Throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseStruct typ |> box)
        Assert.Contains(expected, exn.Message)


    type CustomEnum = A = 1 | B = 2

    [<Fact>]
    let ``Enum without an attribute is not allowed``() =
        throws typeof<CustomEnum> "The enum type 'CustomEnum' is not part of a Thrift interface definition"


    [<ThriftEnum>]
    type ByteEnum = A = 1y | B = 2y

    [<Fact>]
    let ``Non-Int32-based enum is not allowed``() =
        throws typeof<ByteEnum> "The enum type 'ByteEnum' has an underlying type different from Int32"


    [<ThriftStruct("CustomValueType")>]
    type CustomValueType(unused: int) = struct end

    [<Theory;
      InlineData(typeof<byte>);
      InlineData(typeof<uint16>);
      InlineData(typeof<uint32>);
      InlineData(typeof<uint64>);
      InlineData(typeof<float32>);
      InlineData(typeof<decimal>);
      InlineData(typeof<CustomValueType>)>]
    let ``Unknown value types are not allowed``(typ) =
        throws typ ("The type '" + typ.Name + "' is an unknown value type.")


    type BadList<'T>( thisIsNotAParameterlessConstructor: obj ) =
        inherit List<'T>()
    
    type BadSet<'T>( thisIsNotAParameterlessConstructor: obj ) =
        inherit HashSet<'T>()
    
    type BadDictionary<'K,'V when 'K: equality>( thisIsNotAParameterlessConstructor: obj ) =
        inherit Dictionary<'K,'V>()

    [<Theory;
      InlineData(typeof<BadList<int>>, "list");
      InlineData(typeof<BadSet<int>>, "set");
      InlineData(typeof<BadDictionary<int,int>>, "map")>]
    let ``Collections without a parameterless constructor are not allowed``(typ: Type, name) =
        throws typ ("The " + name + " type '" + typ.Name + "' is not supported.")
        
    type ListAndSet<'T>() =
        inherit List<'T>()
        interface ISet<'T> with
            member x.Add(_: 'T) = false
            member x.Contains(_) = false
            member x.UnionWith(_) = ()
            member x.ExceptWith(_) = ()
            member x.IntersectWith(_) = ()
            member x.SymmetricExceptWith(_) = ()
            member x.SetEquals(_) = false
            member x.Overlaps(_) = false
            member x.IsSupersetOf(_) = false
            member x.IsSubsetOf(_) = false
            member x.IsProperSupersetOf(_) = false
            member x.IsProperSubsetOf(_) = false

    [<Fact>]
    let ``Implementing both IList and ISet is not supported``() =
        throws typeof<ListAndSet<int>> "The collection type 'ListAndSet`1' implements more than one of IDictionary<TKey, TValue>, ISet<T> and IList<T>."


    type WeirdList<'T>() =
        inherit List<string>()

    [<Fact>]
    let ``IList<T> implementation with a different generic arg than the interface``() =
        let typ = makeStruct typeof<WeirdList<int>>
        let thriftStruct = ThriftAttributesParser.ParseStruct typ

        thriftStruct.Fields.Count <=> 1
        thriftStruct.Fields.[0].BackingProperty.PropertyType <=> typeof<WeirdList<int>>

    type SetOfInt() =
        interface IEnumerable with
            member x.GetEnumerator() = Unchecked.defaultof<IEnumerator>

        interface IEnumerable<int> with
            member x.GetEnumerator() = Unchecked.defaultof<IEnumerator<int>>

        interface ICollection<int> with
            member x.Count with get() = 0
            member x.IsReadOnly with get() = false
            member x.Add(_: int) = ()
            member x.Contains(_: int) = false
            member x.Remove(_) = false
            member x.Clear() = ()
            member x.CopyTo(_, _) = ()

        interface ISet<int> with
            member x.Add(_: int) = false
            member x.UnionWith(_) = ()
            member x.ExceptWith(_) = ()
            member x.IntersectWith(_) = ()
            member x.SymmetricExceptWith(_) = ()
            member x.SetEquals(_) = false
            member x.Overlaps(_) = false
            member x.IsSupersetOf(_) = false
            member x.IsSubsetOf(_) = false
            member x.IsProperSupersetOf(_) = false
            member x.IsProperSubsetOf(_) = false

    type SetOfIntAndOfStrings() =
        inherit SetOfInt()
        interface IEnumerable<string> with
            member x.GetEnumerator() = Unchecked.defaultof<IEnumerator<string>>

        interface ICollection<string> with
            member x.Count with get() = 0
            member x.IsReadOnly with get() = false
            member x.Add(_: string) = ()
            member x.Contains(_: string) = false
            member x.Remove(_) = false
            member x.Clear() = ()
            member x.CopyTo(_, _) = ()

        interface ISet<string> with
            member x.Add(_: string) = false
            member x.UnionWith(_) = ()
            member x.ExceptWith(_) = ()
            member x.IntersectWith(_) = ()
            member x.SymmetricExceptWith(_) = ()
            member x.SetEquals(_) = false
            member x.Overlaps(_) = false
            member x.IsSupersetOf(_) = false
            member x.IsSubsetOf(_) = false
            member x.IsProperSupersetOf(_) = false
            member x.IsProperSubsetOf(_) = false

    [<Fact>]
    let ``ISet implementing two ISet<T> versions is not allowed``() =
        throws typeof<SetOfIntAndOfStrings> "The collection type 'SetOfIntAndOfStrings' implements more than one version of the same generic interface"

module ``Parsing structs: Normal structs`` =
    let parse (typ: Type) expectedCount =
        ThriftAttributesParser.ParseStruct(typ.GetTypeInfo()).Fields.Count <=> expectedCount

    
    [<ThriftStruct("NoFields")>]
    type StructWithoutFields() = class end

    [<Fact>]
    let ``Struct without fields``() =
        parse typeof<StructWithoutFields> 0
    
    
    [<ThriftStruct("OnlyUnmarkedFields")>]
    type StructWithOnlyUnmarkedFields() =   
        member val Property = 1 with get, set
    
    [<Fact>]
    let ``Struct with only unmarked fields``() =
        parse typeof<StructWithOnlyUnmarkedFields> 0


    [<ThriftStruct("OnlyMarkedFields")>]
    type StructWithOnlyMarkedFields() =
        [<ThriftField(1s, true, "Marked")>]
        member val Property = 1 with get, set
    
    [<Fact>]
    let ``Struct with only marked fields``() =
        parse typeof<StructWithOnlyMarkedFields> 1


    [<ThriftStruct("UnmarkedFields")>]
    type StructWithUnmarkedFields() =   
        member val Unmarked = 1 with get, set
    
        [<ThriftField(1s, true, "Marked")>]
        member val Marked = 1 with get, set
    
    [<Fact>]
    let ``Struct with marked and unmarked fields``() =
        parse typeof<StructWithUnmarkedFields>1


    
    [<ThriftStruct("SelfReferencing")>]
    type StructWithSelfReference() =
        [<ThriftField(1s, true, "Field")>]
        member val Field = Unchecked.defaultof<StructWithSelfReference> with get, set
    
    [<ThriftStruct("SelfReferencingList")>]
    type StructWithSelfReferenceList() =
        [<ThriftField(1s, true, "Field")>]
        member val Field = null :> List<StructWithSelfReference> with get, set
    
    [<ThriftStruct("SelfReferencingMap")>]
    type StructWithSelfReferenceMap() =
        [<ThriftField(1s, true, "Field")>]
        member val Field = null :> Dictionary<StructWithSelfReference,StructWithSelfReference> with get, set

    [<Theory;
      InlineData(typeof<StructWithSelfReference>);
      InlineData(typeof<StructWithSelfReferenceList>);
      InlineData(typeof<StructWithSelfReferenceMap>)>]
    let ``Self-referencing struct``(typ) =
        parse typ 1

module ``Parsing structs: Bad structs`` =
    let throws (typ: Type) expected =
        let exn = Assert.Throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseStruct(typ.GetTypeInfo()) |> box)
        Assert.Contains(expected, exn.Message)


    type StructWithoutAttribute = class end

    [<Fact>]
    let ``Struct without attribute is not allowed``() =
        throws typeof<StructWithoutAttribute> "The type 'StructWithoutAttribute' is not part of a Thrift interface definition"


    [<ThriftStruct("Interface")>]
    type Interface = interface end

    [<Fact>]
    let ``Interface is not allowed``() =
        throws typeof<Interface> "The type 'Interface' is not a concrete type"


    [<ThriftStruct("Abstract")>]
    [<AbstractClass>]
    type Abstract  = class end

    [<Fact>]
    let ``Abstract class is not allowed``() =
        throws typeof<Abstract> "The type 'Abstract' is not a concrete type"