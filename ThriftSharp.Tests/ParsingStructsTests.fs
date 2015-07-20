// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Parsing structs and fields``

open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals

type EnumWithoutAttribute = A = 1 | B = 2

[<ThriftEnum>]
type MyEnum = A = 1 | B = 2

[<ThriftEnum>]
type ByteEnum = A = 1uy | B = 2uy

type CustomList<'T>() =
    inherit List<'T>()

type CustomSet<'T>() =
    inherit HashSet<'T>()

type CustomDictionary<'K,'V when 'K: equality>() =
    inherit Dictionary<'K,'V>()

type BadList<'T>( thisIsNotAParameterlessConstructor: obj ) =
    inherit List<'T>()

type BadSet<'T>( thisIsNotAParameterlessConstructor: obj ) =
    inherit HashSet<'T>()

type BadDictionary<'K,'V when 'K: equality>( thisIsNotAParameterlessConstructor: obj ) =
    inherit Dictionary<'K,'V>()

type EvilList<'T>() =
    inherit List<string>()
    
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

[<ThriftStruct("Interface")>]
type Interface = interface end

[<ThriftStruct("Abstract"); AbstractClass>]
type Abstract() = class end

type UnmarkedStruct() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 0 with get, set
    
[<ThriftStruct("NoFields")>]
type StructWithoutFields() = class end

[<ThriftStruct("OnlyUnmarkedFields")>]
type StructWithOnlyUnmarkedFields() =   
    member val Property = 1 with get, set

[<ThriftStruct("UnmarkedFields")>]
type StructWithUnmarkedFields() =   
    member val Unmarked = 1 with get, set

    [<ThriftField(1s, true, "Marked")>]
    member val Marked = 1 with get, set

[<ThriftStruct("SelfReferencing")>]
type StructWithSelfReference() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = Unchecked.defaultof<StructWithSelfReference> with get, set

    [<ThriftField(2s, true, "Array")>]
    member val List = null :> List<StructWithSelfReference> with get, set

    [<ThriftField(3s, true, "Array")>]
    member val Dictionary = null :> Dictionary<StructWithSelfReference,StructWithSelfReference> with get, set


let parse<'T> =
    ThriftAttributesParser.ParseStruct(typeof<'T>.GetTypeInfo())

let ok typ isReq =
    let name = "Struct"
    let fieldName = "Field"
    let genType = makeClass [ <@ ThriftStructAttribute(name) @> ] [fieldName, typ, [ <@ ThriftFieldAttribute(0s, isReq, fieldName) @> ]]
    let thriftStruct = ThriftAttributesParser.ParseStruct(genType.GetTypeInfo())

    thriftStruct.TypeInfo <=> genType.GetTypeInfo()
    thriftStruct.Header.Name <=> name

    thriftStruct.Fields.Count <=> 1
    thriftStruct.Fields.[0].BackingProperty.PropertyType <=> typ
    thriftStruct.Fields.[0].IsRequired <=> isReq 

let fails typ isReq =
    let typ = makeClass [ <@ ThriftStructAttribute("Struct") @> ] ["Field", typ, [ <@ ThriftFieldAttribute(0s, isReq, "Field") @> ]]
    throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseStruct(typ.GetTypeInfo()) |> box) |> ignore

let failsOn<'T> =
    throws<ThriftParsingException> (fun () -> ThriftAttributesParser.ParseStruct(typeof<'T>.GetTypeInfo()) |> box) |> ignore


(* TestClass *)
type ``Primitives``() =
    (* Test *) member __.``Boolean required field``() = ok typeof<bool> true
    (* Test *) member __.``SByte required field``() =   ok typeof<sbyte> true
    (* Test *) member __.``Int16 required field``() =   ok typeof<int16> true
    (* Test *) member __.``Int32 required field``() =   ok typeof<int32> true
    (* Test *) member __.``Int64 required field``() =   ok typeof<int64> true
    (* Test *) member __.``Double required field``() =  ok typeof<double> true
    (* Test *) member __.``String required field``() =  ok typeof<string> true
    (* Test *) member __.``Binary required field``() =  ok typeof<sbyte[]> true
    (* Test *) member __.``Enum required field``() =    ok typeof<MyEnum> true

    (* Test *) member __.``String optional field``() = ok typeof<string> false
    (* Test *) member __.``Binary optional field``() = ok typeof<sbyte[]> false

(* TestClass *)
type ``Collections``() =
    (* Test *) member __.``Array required field``() =                ok typeof<int[]> true
    (* Test *) member __.``IList required field``() =                ok typeof<IList<int>> true
    (* Test *) member __.``List required field``() =                 ok typeof<List<int>> true
    (* Test *) member __.``Custom IList required field``() =         ok typeof<CustomList<int>> true
    (* Test *) member __.``ISet required field``() =                 ok typeof<ISet<int>> true
    (* Test *) member __.``HashSet required field``() =              ok typeof<HashSet<int>> true
    (* Test *) member __.``Custom ISet required field``() =          ok typeof<CustomSet<int>> true
    (* Test *) member __.``IDictionary required field``() =          ok typeof<IDictionary<int,int>> true
    (* Test *) member __.``Dictionary required field``() =           ok typeof<Dictionary<int,int>> true
    (* Test *) member __.``Custom IDictionary required field``() =   ok typeof<CustomDictionary<int,int>> true

    (* Test *) member __.``Array optional field``() =                ok typeof<int[]> false
    (* Test *) member __.``IList optional field``() =                ok typeof<IList<int>> false
    (* Test *) member __.``List optional field``() =                 ok typeof<List<int>> false
    (* Test *) member __.``Custom IList optional field``() =         ok typeof<CustomList<int>> false
    (* Test *) member __.``ISet optional field``() =                 ok typeof<ISet<int>> false
    (* Test *) member __.``HashSet optional field``() =              ok typeof<HashSet<int>> false
    (* Test *) member __.``Custom ISet optional field``() =          ok typeof<CustomSet<int>> false
    (* Test *) member __.``IDictionary optional field``() =          ok typeof<IDictionary<int,int>> false
    (* Test *) member __.``Dictionary optional field``() =           ok typeof<Dictionary<int,int>> false
    (* Test *) member __.``Custom IDictionary optional field``() =   ok typeof<CustomDictionary<int,int>> false

(* TestClass *)
type ``Special collections``() =
    (* Test *) member __.``IList implementation without parameterless ctor``() =       fails typeof<BadList<int>> true
    (* Test *) member __.``ISet implementation without parameterless ctor``() =        fails typeof<BadSet<int>> true
    (* Test *) member __.``IDictionary implementation without parameterless ctor``() = fails typeof<BadDictionary<int,int>> true
    (* Test *) member __.``Both IList and ISet implementation``() =                    fails typeof<ListAndSet<int>> true
    (* Test *) member __.``IList implementation with generic arg not from IList``() =  ok typeof<EvilList<int>> true

(* TestClass *)
type ``Special structs``() =
    (* Test *) member __.``No fields at all``() = parse<StructWithoutFields>.Fields.Count <=> 0
    (* Test *) member __.``Only non-Thrift fields``() = parse<StructWithOnlyUnmarkedFields>.Fields.Count <=> 0
    (* Test *) member __.``Some non-Thrift fields``() = parse<StructWithUnmarkedFields>.Fields.Count <=> 1
    
    (* Test *) member __.``Interface``() = failsOn<Interface>
    (* Test *) member __.``Abstract class``() = failsOn<Abstract>

    (* Test *) member __.``Non-Thrift class``() = failsOn<UnmarkedStruct>

    (* Test
    member __.``Self-referencing``() = 
        parse<StructWithSelfReference>.Fields |> Seq.sortBy (fun f -> f.Id) 
                                              |> Seq.map (fun f -> f.BackingProperty.PropertyType)
                                              |> List.ofSeq
        <=>
        [ typeof<StructWithSelfReference>
          typeof<List<StructWithSelfReference>>
          typeof<Dictionary<StructWithSelfReference,StructWithSelfReference>> ] *) 

(* TestClass *)
type ``Wrong primitive fields``() =
    (* Test *) member __.``Required nullable field``() = fails typeof<System.Nullable<int32>> true

    (* Test *) member __.``Unsigned byte field``() =  fails typeof<byte> true
    (* Test *) member __.``Unsigned int16 field``() = fails typeof<uint16> true
    (* Test *) member __.``Unsigned int32 field``() = fails typeof<uint32> true
    (* Test *) member __.``Unsigned int64 field``() = fails typeof<uint64> true
    (* Test *) member __.``Float field``() =          fails typeof<float32> true
    (* Test *) member __.``Decimal field``() =        fails typeof<decimal> true

    (* Test *) member __.``Boolean optional field``() = fails typeof<bool> false
    (* Test *) member __.``SByte optional field``() =   fails typeof<sbyte> false
    (* Test *) member __.``Int16 optional field``() =   fails typeof<int16> false
    (* Test *) member __.``Int32 optional field``() =   fails typeof<int32> false
    (* Test *) member __.``Int64 optional field``() =   fails typeof<int64> false
    (* Test *) member __.``Double optional field``() =  fails typeof<double> false
    (* Test *) member __.``Enum optional field``() = fails typeof<MyEnum> false

    (* Test *) member __.``Non-Int32-based enum field``() = fails typeof<ByteEnum> true

    (* Test *) member __.``Non-Thrift enum field``() = fails typeof<EnumWithoutAttribute> true