// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Parsing structs``

open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals


// Special types used as fields

type EnumWithoutAttribute = A = 1 | B = 2

[<ThriftEnum>]
type MyEnum = A = 1 | B = 2

[<ThriftEnum>]
type ByteEnum = A = 1uy | B = 2uy

type CustomCollection<'T>( thisIsNotAParameterlessConstructor: obj ) =
    interface ICollection<'T> with
        member x.Count with get() = 0
        member x.IsReadOnly with get() = false
        member x.Add(_) = ()
        member x.Remove(_) = false
        member x.Clear() = ()
        member x.Contains(_) = false
        member x.CopyTo(_,_) = ()
        member x.GetEnumerator() = null :> IEnumerator<'T>
        member x.GetEnumerator() = null :> System.Collections.IEnumerator

type CustomSet<'T>( thisIsNotAParameterlessConstructor: obj ) =
    interface ISet<'T> with
        member x.Count with get() = 0
        member x.IsReadOnly with get() = false
        member x.Add(_: 'T) = ()
        member x.Add(_: 'T) = false
        member x.Remove(_) = false
        member x.Clear() = ()
        member x.UnionWith(_) = ()
        member x.IntersectWith(_) = ()
        member x.ExceptWith(_) = ()
        member x.SymmetricExceptWith(_) = ()
        member x.IsSubsetOf(_) = false
        member x.IsProperSubsetOf(_) = false
        member x.IsSupersetOf(_) = false
        member x.IsProperSupersetOf(_) = false
        member x.Overlaps(_) = false
        member x.SetEquals(_) = false
        member x.Contains(_) = false
        member x.CopyTo(_,_) = ()
        member x.GetEnumerator() = null :> IEnumerator<'T>
        member x.GetEnumerator() = null :> System.Collections.IEnumerator

type CustomDictionary<'K,'V>( thisIsNotAParameterlessConstructor: obj ) =
    interface IDictionary<'K,'V> with
        member x.Count with get() = 0
        member x.IsReadOnly with get() = false
        member x.Item with get(_: 'K) = Unchecked.defaultof<'V> and set _ _ = ()
        member x.Keys with get () = null
        member x.Values with get () = null
        member x.Add(_) = ()
        member x.Add(_: 'K,_: 'V) = ()
        member x.Remove(_: 'K) = false
        member x.Remove(_: KeyValuePair<'K,'V>) = false
        member x.Clear() = ()
        member x.Contains(_) = false
        member x.ContainsKey(_) = false
        member x.TryGetValue(_: 'K, _: byref<'V>) = false
        member x.CopyTo(_,_) = ()
        member x.GetEnumerator() = null :> IEnumerator<KeyValuePair<'K,'V>>
        member x.GetEnumerator() = null :> System.Collections.IEnumerator

// Special structs

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
    thriftStruct.Fields.[0].TypeInfo.AsType() <=> typ
    thriftStruct.Fields.[0].IsRequired <=> isReq 

let fails typ isReq =
    let typ = makeClass [ <@ ThriftStructAttribute("Struct") @> ] ["Field", typ, [ <@ ThriftFieldAttribute(0s, isReq, "Field") @> ]]
    throws<ThriftParsingException>(fun () -> ThriftAttributesParser.ParseStruct(typ.GetTypeInfo()) |> box) |> ignore

let failsOn<'T> =
    throws<ThriftParsingException> (fun () -> ThriftAttributesParser.ParseStruct(typeof<'T>.GetTypeInfo()) |> box) |> ignore


[<TestClass>]
type __() =
    // Errors should be thrown when parsing structs without Thrift fields
    [<Test>] member __.``Error on no fields``() =        failsOn<StructWithoutFields>
    [<Test>] member __.``Error on no Thrift fields``() = failsOn<StructWithOnlyUnmarkedFields>

    // Required fields of primitive types should be parsed correctly
    [<Test>] member __.``Boolean required field``() = ok typeof<bool> true
    [<Test>] member __.``SByte required field``() =   ok typeof<sbyte> true
    [<Test>] member __.``Int16 required field``() =   ok typeof<int16> true
    [<Test>] member __.``Int32 required field``() =   ok typeof<int32> true
    [<Test>] member __.``Int64 required field``() =   ok typeof<int64> true
    [<Test>] member __.``Double required field``() =  ok typeof<double> true
    [<Test>] member __.``String required field``() =  ok typeof<string> true
    [<Test>] member __.``Binary required field``() =  ok typeof<sbyte[]> true

    // Optional fields of primitive reference types should be parsed correctly
    [<Test>] member __.``String optional field``() = ok typeof<string> false
    [<Test>] member __.``Binary optional field``() = ok typeof<sbyte[]> false

    // Errors should be thrown when parsing unsupported primitive types
    [<Test>] member __.``Error on unsigned byte field``() =  fails typeof<byte> true
    [<Test>] member __.``Error on unsigned int16 field``() = fails typeof<uint16> true
    [<Test>] member __.``Error on unsigned int32 field``() = fails typeof<uint32> true
    [<Test>] member __.``Error on unsigned int64 field``() = fails typeof<uint64> true
    [<Test>] member __.``Error on float field``() =          fails typeof<float32> true
    [<Test>] member __.``Error on decimal field``() =        fails typeof<decimal> true

    // Errors should be thrown when parsing optional fields of primitive value types
    [<Test>] member __.``Error on boolean optional field``() = fails typeof<bool> false
    [<Test>] member __.``Error on sbyte optional field``() =   fails typeof<sbyte> false
    [<Test>] member __.``Error on int16 optional field``() =   fails typeof<int16> false
    [<Test>] member __.``Error on int32 optional field``() =   fails typeof<int32> false
    [<Test>] member __.``Error on int64 optional field``() =   fails typeof<int64> false
    [<Test>] member __.``Error on double optional field``() =  fails typeof<double> false

    // Required enum fields should be parsed correctly
    [<Test>] member __.``Enum required field``() = ok typeof<MyEnum> true

    // Errors should be thrown when parsing optional enum fields
    [<Test>] member __.``Error on enum optional field``() = fails typeof<MyEnum> false

    // Errors should be thrown when parsing non-int32-based enum fields
    [<Test>] member __.``Error on non-int32-based enum field``() = fails typeof<ByteEnum> true

    // Errors should be thrown when parsing non-Thrift enum fields
    [<Test>] member __.``Error on non-Thrift enum field``() = fails typeof<EnumWithoutAttribute> true

    // Required fields of known interfaces or concrete collection types should be parsed correctly
    [<Test>] member __.``Array required field``() =                ok typeof<int[]> true
    [<Test>] member __.``ICollection required field``() =          ok typeof<ICollection<int>> true
    [<Test>] member __.``ObservableCollection required field``() = ok typeof<ObservableCollection<int>> true
    [<Test>] member __.``IList required field``() =                ok typeof<IList<int>> true
    [<Test>] member __.``List required field``() =                 ok typeof<List<int>> true
    [<Test>] member __.``ISet required field``() =                 ok typeof<ISet<int>> true
    [<Test>] member __.``HashSet required field``() =              ok typeof<HashSet<int>> true
    [<Test>] member __.``IDictionary required field``() =          ok typeof<IDictionary<int,int>> true
    [<Test>] member __.``Dictionary required field``() =           ok typeof<Dictionary<int,int>> true

    // Optional fields of known interfaces or concrete collection types should be parsed correctly
    [<Test>] member __.``Array optional field``() =                ok typeof<int[]> false
    [<Test>] member __.``ICollection optional field``() =          ok typeof<ICollection<int>> false
    [<Test>] member __.``ObservableCollection optional field``() = ok typeof<ObservableCollection<int>> false
    [<Test>] member __.``IList optional field``() =                ok typeof<IList<int>> false
    [<Test>] member __.``List optional field``() =                 ok typeof<List<int>> false
    [<Test>] member __.``ISet optional field``() =                 ok typeof<ISet<int>> false
    [<Test>] member __.``HashSet optional field``() =              ok typeof<HashSet<int>> false
    [<Test>] member __.``IDictionary optional field``() =          ok typeof<IDictionary<int,int>> false
    [<Test>] member __.``Dictionary optional field``() =           ok typeof<Dictionary<int,int>> false

    // Errors should be thrown when encountering unknown collections
    [<Test>] member __.``Error on field with unknown ICollection implementation``() = fails typeof<CustomCollection<int>> true
    [<Test>] member __.``Error on field with unknown ISet implementation``() =        fails typeof<CustomSet<int>> true
    [<Test>] member __.``Error on field with unknown IDictionary implementation``() = fails typeof<CustomDictionary<int,int>> true

    // Errors should be thrown when parsing interface and abstract classes
    [<Test>] member __.``Error on interface``() = failsOn<Interface>
    [<Test>] member __.``Error on abstract class``() = failsOn<Abstract>

    // Errors should be thrown when parsing non-Thrift classes
    [<Test>] member __.``Error on non-Thrift class``() = failsOn<UnmarkedStruct>

    // Struct which contain non-Thrift fields should be parsed correctly
    [<Test>] member __.``Some non-Thrift fields``() = parse<StructWithUnmarkedFields>.Fields.Count <=> 1

    // Structs with fields that reference the struct itself should be parsed correctly
    [<Test>] 
    member __.``Self-referencing struct``() = 
        parse<StructWithSelfReference>.Fields |> Seq.sortBy (fun f -> f.Id) 
                                              |> Seq.map (fun f -> f.TypeInfo.AsType())
                                              |> List.ofSeq
        <=>
        [ typeof<StructWithSelfReference>
          typeof<List<StructWithSelfReference>>
          typeof<Dictionary<StructWithSelfReference,StructWithSelfReference>> ]