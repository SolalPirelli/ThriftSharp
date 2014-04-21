// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Parsing structs``

open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals

[<ThriftStruct("Interface")>]
type Interface =
    interface end

[<ThriftStruct("Abstract"); AbstractClass>]
type Abstract() =
    class end

type UnmarkedStruct() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 0 with get, set

[<ThriftStruct("NoFields")>]
type StructWithoutFields() =
    class end

[<ThriftStruct("UnmarkedFields")>]
type StructWithUnmarkedFields() =   
    member val Property = 1 with get, set

[<ThriftStruct("PrimitiveFields")>]
type StructWithPrimitiveFields() =
    [<ThriftField(1s, true, "Bool")>]
    member val Bool = true with get, set
    [<ThriftField(2s, true, "SByte")>]
    member val SByte = 1y with get, set
    [<ThriftField(3s, true, "Double")>]
    member val Double = 1.0 with get, set
    [<ThriftField(4s, true, "Int16")>]
    member val Int16 = 1s with get, set
    [<ThriftField(5s, true, "Int3")>]
    member val Int32 = 1 with get, set
    [<ThriftField(6s, true, "Int64")>]
    member val Int64 = 1L with get, set
    [<ThriftField(7s, true, "String")>]
    member val String = "abc" with get, set
    [<ThriftField(8s, true, "Binary")>]
    member val Binary = [| 1y |] with get, set


[<ThriftStruct("UnknownPrimitiveFields")>]
type StructWithUnknownPrimitiveFields() =
    [<ThriftField(1s, true, "UInt32")>]
    member val UInt32 = 1u with get, set
    [<ThriftField(1s, true, "UnsignedByte")>]
    member val UnsignedByte = 1uy with get, set


[<ThriftStruct("OptionalValueTypeFields")>]
type StructWithOptionalValueTypeFields() =
    [<ThriftField(1s, false, "Field")>]
    member val Field = 0 with get, set 
    

[<ThriftEnum>]
type Enum =
  | A = 1
  | B = 2
  
[<ThriftStruct("Enum")>]
type StructWithEnumField() =
    [<ThriftField(1s, true, "Enum")>]
    member val Field = Enum.A with get, set


type EnumWithoutAttribute =
  | A = 1
  | B = 2

[<ThriftStruct("EnumWithoutAttribute")>]
type StructWithEnumWithoutAttribute() =
    [<ThriftField(1s, true, "Enum")>]
    member val Field = EnumWithoutAttribute.A with get, set


[<ThriftEnum>]
type ByteEnum =
  | A = 1uy
  | B = 2uy

[<ThriftStruct("ByteEnum")>]
type StructWithByteEnum() =
    [<ThriftField(1s, true, "Enum")>]
    member val Field = ByteEnum.A with get, set


[<ThriftStruct("Collections")>]
type StructWithCollections() =
    [<ThriftField(1s, true, "ICollection")>]
    member val ICollection = null :> ICollection<int> with get, set
    [<ThriftField(2s, true, "IList")>]
    member val IList = null :> IList<int> with get, set
    [<ThriftField(3s, true, "List")>]
    member val List = null :> List<int> with get, set
    [<ThriftField(4s, true, "Array")>]
    member val Array = [| 0 |] with get, set
    [<ThriftField(5s, true, "ISet")>]
    member val ISet = null :> ISet<int> with get, set
    [<ThriftField(6s, true, "HashSet")>]
    member val HashSet = null :> HashSet<int> with get, set
    [<ThriftField(7s, true, "IDictionary")>]
    member val IDictionary = null :> IDictionary<int,int> with get, set
    [<ThriftField(8s, true, "Dictionary")>]
    member val Dictionary = null :> Dictionary<int,int> with get, set
    [<ThriftField(9s, true, "ObservableCollection")>]
    member val ObservableCollection = null :> ObservableCollection<int> with get, set


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

[<ThriftStruct("UnknownCollection")>]
type StructWithUnknownCollection() =
    [<ThriftField(1s, true, "CustomCollection")>]
    member val CustomCollection = CustomCollection<int>( 0 ) with get, set


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

[<ThriftStruct("UnknownSet")>]
type StructWithUnknownSet() =
    [<ThriftField(1s, true, "CustomSet")>]
    member val CustomSet = CustomSet<int>( 0 ) with get, set


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

[<ThriftStruct("UnknownDictionary")>]
type StructWithUnknownDictionary() =
    [<ThriftField(1s, true, "CustomDictionary")>]
    member val CustomDictionary = CustomDictionary<int,int>( 0 ) with get, set

[<ThriftStruct("SelfReferencing")>]
type StructWithSelfReference() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = Unchecked.defaultof<StructWithSelfReference> with get, set

    [<ThriftField(2s, true, "Array")>]
    member val Array = Unchecked.defaultof<StructWithSelfReference[]> with get, set


let parseOk<'T> () =
    ThriftAttributesParser.ParseStruct(typeof<'T>.GetTypeInfo()) |> ignore

let parseError<'T> () =
    throwsAsync<ThriftParsingException> (fun () -> async { return box (ThriftAttributesParser.ParseStruct(typeof<'T>.GetTypeInfo())) })
 |> Async.RunSynchronously
 |> ignore

[<TestContainer>]
type __() =
    [<Test>]
    member __.``Error on interface``() =
        parseError<Interface>()

    [<Test>]
    member __.``Error on abstract class``() =
        parseError<Abstract>()

    [<Test>]
    member __.``Error when the struct isn't marked with an attribute``() =
        parseError<UnmarkedStruct>()

    [<Test>]
    member __.``No fields``() = 
        parseOk<StructWithoutFields>()

    [<Test>]
    member __.``Fields without attributes``() =
        parseOk<StructWithUnmarkedFields>()

    [<Test>]
    member __.``Primitive fields``() =
        parseOk<StructWithPrimitiveFields>()

    [<Test>]
    member __.``Error on unknown primitive fields``() =
        parseError<StructWithUnknownPrimitiveFields>()

    [<Test>]
    member __.``Error on optional value-type fields``() =
        parseError<StructWithOptionalValueTypeFields>()

    [<Test>]
    member __.``Enum field``() =
        parseOk<StructWithEnumField>()

    [<Test>]
    member __.``Error on enum without attribute``() =
        parseError<StructWithEnumWithoutAttribute>()

    [<Test>]
    member __.``Error on enum not int32-based``() =
        parseError<StructWithByteEnum>()

    [<Test>]
    member __.``Collection fields``() =
        parseOk<StructWithCollections>()

    [<Test>]
    member __.``Error on unknown collection field``() =
        parseError<StructWithUnknownCollection>()

    [<Test>]
    member __.``Error on unknown set field``() =
        parseError<StructWithUnknownSet>()

    [<Test>]
    member __.``Error on unknown dictionary field``() =
        parseError<StructWithUnknownDictionary>()

    [<Test>]
    member __.``Self-referencing struct``() =
        parseOk<StructWithSelfReference>()