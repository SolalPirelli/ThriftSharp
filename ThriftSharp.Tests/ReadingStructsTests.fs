// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals

[<ThriftStruct("NoFields")>]
type StructWithoutFields1() = class end

[<ThriftStruct("OneField")>]
type StructWithOneField1() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 42 with get, set

[<ThriftStruct("ManyPrimitiveFields")>]
type StructWithManyPrimitiveFields1() =
    [<ThriftField(1s, true, "BoolField")>]
    member val Bool = true with get, set
    [<ThriftField(2s, true, "SByteField")>]
    member val SByte = 1y with get, set
    [<ThriftField(3s, true, "DoubleField")>]
    member val Double = 1.0 with get, set
    [<ThriftField(4s, true, "Int16Field")>]
    member val Int16 = 1s with get, set
    [<ThriftField(5s, true, "Int32Field")>]
    member val Int32 = 1 with get, set
    [<ThriftField(6s, true, "Int64Field")>]
    member val Int64 = 1L with get, set
    [<ThriftField(7s, true, "StringField")>]
    member val String = "abc" with get, set
    [<ThriftField(8s, true, "BinaryField")>]
    member val Binary = [| 1y |] with get, set
    
[<ThriftStruct("CollectionFields")>]
type StructWithCollectionFields1() =
    [<ThriftField(1s, true, "ListField")>]
    member val List = List<int>() with get, set
    [<ThriftField(2s, true, "SetField")>]
    member val Set = HashSet<int>() with get, set
    [<ThriftField(3s, true, "MapField")>]
    member val Map = Dictionary<int, string>() with get, set

[<ThriftStruct("StructField")>]
type StructWithStructField1() =
    [<ThriftField(1s, true, "StructField")>]
    member val Struct = StructWithOneField1() with get, set

[<ThriftEnum("Enum")>]
type Enum1 =
    | A = 1
    | [<ThriftEnumMember("B", 3)>] B = 2

[<ThriftStruct("EnumFields")>]
type StructWithEnumFields1() =
    [<ThriftField(1s, true, "Field1")>]
    member val Field1 = Enum1.A with get, set
    [<ThriftField(2s, true, "Field2")>]
    member val Field2 = Enum1.A with get, set
    [<ThriftField(3s, true, "Field3")>]
    member val Field3 = Enum1.A with get, set

[<ThriftStruct("ArrayFields")>]
type StructWithArrayFields1() =
    [<ThriftField(1s, true, "Field1")>]
    member val Field1 = [| 1 |] with get, set
    [<ThriftField(2s, true, "Field2")>]
    member val Field2 = [| 2 |] with get, set

[<ThriftStruct("ConvertingField")>]
type StructWithConvertingField1() =
    [<ThriftField(1s, true, "UnixDate")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    member val UnixDate = System.DateTime.Now with get, set

[<ThriftStruct("NullableField")>]
type StructWithNullableField1() =
    [<ThriftField(1s, false, "Field")>]
    member val Field = System.Nullable() with get, set

[<ThriftStruct("NullableFieldWithDefault")>]
type StructWithNullableFieldWithDefault1() =
    [<ThriftField(1s, false, "Field")>]
    [<ThriftDefaultValue(42)>]
    member val Field = System.Nullable() with get, set

[<TestClass>]
type ``Reading structs``() =
    member x.ReadStruct<'T>(prot) = ThriftSerializer.Struct.Read(prot, typeof<'T>) :?> 'T

    [<Test>]
    member x.``No fields``() =
        let m = MemoryProtocol([StructHeader "NoFields"
                                FieldStop
                                StructEnd])
        x.ReadStruct<StructWithoutFields1>(m) |> ignore
        m.IsEmpty <=> true

    [<Test>]
    member x.``One field``() =
        let m = MemoryProtocol([StructHeader "OneField"
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 34
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithOneField1>(m)
        inst.Field <=> 34
        m.IsEmpty <=> true

    [<Test>]
    member x.``Primitive fields``() =
        let m = MemoryProtocol([StructHeader "ManyPrimitiveFields"
                                FieldHeader (1s, "BoolField", tid 2uy)
                                Bool false
                                FieldEnd
                                FieldHeader (2s, "SByteField", tid 3uy)
                                SByte -1y
                                FieldEnd
                                FieldHeader (3s, "DoubleField", tid 4uy)
                                Double -1.0
                                FieldEnd
                                FieldHeader (4s, "Int16Field", tid 6uy)
                                Int16 -1s
                                FieldEnd
                                FieldHeader (5s, "Int32Field", tid 8uy)
                                Int32 -1
                                FieldEnd
                                FieldHeader (6s, "Int64Field", tid 10uy)
                                Int64 -1L
                                FieldEnd
                                FieldHeader (7s, "StringField", tid 11uy)
                                String "xyzzy"
                                FieldEnd
                                FieldHeader (8s, "BinaryField", tid 11uy)
                                Binary [| 2y; 3y |]
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithManyPrimitiveFields1>(m)
        inst.Bool <=> false
        inst.SByte <=> -1y
        inst.Double <=> -1.0
        inst.Int16 <=> -1s
        inst.Int32 <=> -1
        inst.Int64 <=> -1L
        inst.String <=> "xyzzy"
        inst.Binary <=> [| 2y; 3y |]
        m.IsEmpty <=> true

    [<Test>]
    member x.``Collection fields``() =
        let m = MemoryProtocol([StructHeader "CollectionFields"
                                FieldHeader (1s, "ListField", tid 15uy)
                                ListHeader (1, tid 8uy)
                                Int32 1
                                ListEnd
                                FieldEnd
                                FieldHeader (2s, "SetField", tid 14uy)
                                SetHeader (3, tid 8uy)
                                Int32 2
                                Int32 3
                                Int32 4
                                SetEnd
                                FieldEnd
                                FieldHeader (3s, "MapField", tid 13uy)
                                MapHeader (2, tid 8uy, tid 11uy)
                                Int32 5
                                String "Five"
                                Int32 6
                                String "Six"
                                MapEnd
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithCollectionFields1>(m)

        let expDict = Dictionary()
        expDict.Add(5, "Five")
        expDict.Add(6, "Six")

        inst.List <===> [1]
        inst.Set <===> [2; 3; 4]
        inst.Map <===> expDict

    [<Test>]
    member x.``Struct field``() =
        let m = MemoryProtocol([StructHeader "StructField"
                                FieldHeader (1s, "StructField", tid 12uy)
                                StructHeader "OneField"
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 23
                                FieldEnd
                                FieldStop
                                StructEnd
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithStructField1>(m)
        inst.Struct.Field <=> 23

    [<Test>]
    member x.``Enum fields``() =
        let m = MemoryProtocol([StructHeader "EnumFields"
                                FieldHeader (2s, "Field2", tid 8uy)
                                Int32 2
                                FieldEnd
                                FieldHeader (1s, "Field1", tid 8uy)
                                Int32 3
                                FieldEnd
                                FieldHeader (3s, "Field3", tid 8uy)
                                Int32 1
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithEnumFields1>(m)
        inst.Field1 <=> Enum1.B
        inst.Field2 <=> (LanguagePrimitives.EnumOfValue(0))
        inst.Field3 <=> Enum1.A

    [<Test>]
    member x.``Array fields``() =
        let m = MemoryProtocol([StructHeader "ArrayFields"
                                FieldHeader (1s, "Field1", tid 15uy)
                                ListHeader (2, tid 8uy)
                                Int32 23
                                Int32 42
                                ListEnd
                                FieldEnd
                                FieldHeader (2s, "Field2", tid 15uy)
                                ListHeader (0, tid 8uy)
                                ListEnd
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithArrayFields1>(m)
        inst.Field1 <===> [23; 42]
        inst.Field2 <===> []

    [<Test>]
    member x.``UnixDate converter``() =
        let m = MemoryProtocol([StructHeader "ConvertingField"
                                FieldHeader (1s, "UnixDate", tid 8uy)
                                Int32 787708800
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithConvertingField1>(m)
        inst.UnixDate <=> System.DateTime(1994, 12, 18, 0, 0, 0)
        m.IsEmpty <=> true

    [<Test>]
    member x.``Nullable field, not set``() =
        let m = MemoryProtocol([StructHeader "NullableField"
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithNullableField1>(m)
        inst.Field <=> System.Nullable()
        m.IsEmpty <=> true

    [<Test>]
    member x.``Nullable field, set``() =
        let m = MemoryProtocol([StructHeader "NullableField"
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 12345
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithNullableField1>(m)
        inst.Field <=> System.Nullable(12345)
        m.IsEmpty <=> true

    [<Test>]
    member x.``Nullable field, not set, with default value``() =
        let m = MemoryProtocol([StructHeader "NullableFieldWithDefault"
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithNullableFieldWithDefault1>(m)
        inst.Field <=> System.Nullable(42)
        m.IsEmpty <=> true