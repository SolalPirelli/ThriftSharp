// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Writing structs``

open System.Collections.Generic
open ThriftSharp

[<ThriftStruct("NoFields")>]
type StructWithoutFields() = class end

[<ThriftStruct("OneField")>]
type StructWithOneField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 42 with get, set

[<ThriftStruct("ManyPrimitiveFields")>]
type StructWithManyPrimitiveFields() =
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
type StructWithCollectionFields() =
    [<ThriftField(1s, true, "ListField")>]
    member val List = List<int>() with get, set
    [<ThriftField(2s, true, "SetField")>]
    member val Set = HashSet<int>() with get, set
    [<ThriftField(3s, true, "MapField")>]
    member val Map = Dictionary<int, string>() with get, set

[<ThriftStruct("StructField")>]
type StructWithStructField() =
    [<ThriftField(1s, true, "StructField")>]
    member val Struct = StructWithOneField() with get, set

[<ThriftEnum("Enum")>]
type Enum =
    | A = 1
    | [<ThriftEnumMember("B", 3)>] B = 2

[<ThriftStruct("EnumFields")>]
type StructWithEnumFields() =
    [<ThriftField(1s, true, "Field1")>]
    member val Field1 = Enum.A with get, set
    [<ThriftField(2s, true, "Field2")>]
    member val Field2 = Enum.A with get, set

[<ThriftStruct("ArrayFields")>]
type StructWithArrayFields() =
    [<ThriftField(1s, true, "Field1")>]
    member val Field1 = [| 1 |] with get, set
    [<ThriftField(2s, true, "Field2")>]
    member val Field2 = [| 2 |] with get, set

[<ThriftStruct("ConvertingField")>]
type StructWithConvertingField() =
    [<ThriftField(1s, true, "UnixDate")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    member val UnixDate = System.DateTime.Now with get, set

[<ThriftStruct("NullableField")>]
type StructWithNullableField() =
    [<ThriftField(1s, false, "Field")>]
    member val Field = System.Nullable() with get, set


let (==>) obj data =
    let m = MemoryProtocol()
    write m obj
    m.WrittenValues <===> data


[<TestContainer>]
type __() =
    [<Test>]
    member __.``No fields``() =
        StructWithoutFields()
        ==>
        [StructHeader "NoFields"
         FieldStop
         StructEnd]
        
    [<Test>]
    member __.``One field``() =
        StructWithOneField()
        ==>
        [StructHeader "OneField"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 42
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Primitive fields``() =
        StructWithManyPrimitiveFields()
        ==>
        [StructHeader "ManyPrimitiveFields"
         FieldHeader (1s, "BoolField", tid 2uy)
         Bool true
         FieldEnd
         FieldHeader (2s, "SByteField", tid 3uy)
         SByte 1y
         FieldEnd
         FieldHeader (3s, "DoubleField", tid 4uy)
         Double 1.0
         FieldEnd
         FieldHeader (4s, "Int16Field", tid 6uy)
         Int16 1s
         FieldEnd
         FieldHeader (5s, "Int32Field", tid 8uy)
         Int32 1
         FieldEnd
         FieldHeader (6s, "Int64Field", tid 10uy)
         Int64 1L
         FieldEnd
         FieldHeader (7s, "StringField", tid 11uy)
         String "abc"
         FieldEnd
         FieldHeader (8s, "BinaryField", tid 11uy)
         Binary [| 1y |]
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Collection fields``() =
        StructWithCollectionFields( List = List([4; 8; 15]), Set = HashSet([16; 23]), Map = dict([42, "Lost"]) )
        ==>
        [StructHeader "CollectionFields"
         FieldHeader (1s, "ListField", tid 15uy)
         ListHeader (3, tid 8uy)
         Int32 4
         Int32 8
         Int32 15
         ListEnd
         FieldEnd
         FieldHeader (2s, "SetField", tid 14uy)
         SetHeader (2, tid 8uy)
         Int32 16
         Int32 23
         SetEnd
         FieldEnd
         FieldHeader (3s, "MapField", tid 13uy)
         MapHeader (1, tid 8uy, tid 11uy)
         Int32 42
         String "Lost"
         MapEnd
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Struct field``() =
        StructWithStructField( Struct = StructWithOneField( Field = 777 ) )
        ==>
        [StructHeader "StructField"
         FieldHeader (1s, "StructField", tid 12uy)
         StructHeader "OneField"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 777
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Enum fields``() =
        StructWithEnumFields( Field1 = Enum.A, Field2 = Enum.B )
        ==>
        [StructHeader "EnumFields"
         FieldHeader (1s, "Field1", tid 8uy)
         Int32 1
         FieldEnd
         FieldHeader (2s, "Field2", tid 8uy)
         Int32 3
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Array fields``() =
        StructWithArrayFields( Field1 = [| 12345; 67890 |], Field2 = [| |] )
        ==>
        [StructHeader "ArrayFields"
         FieldHeader (1s, "Field1", tid 15uy)
         ListHeader (2, tid 8uy)
         Int32 12345
         Int32 67890
         ListEnd
         FieldEnd
         FieldHeader (2s, "Field2", tid 15uy)
         ListHeader (0, tid 8uy)
         ListEnd
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``UnixDate converter``() =
        StructWithConvertingField( UnixDate = utcDate(18, 12, 1994) )
        ==>
        [StructHeader "ConvertingField"
         FieldHeader (1s, "UnixDate", tid 8uy)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Nullable field, not set``() =
        StructWithNullableField()
        ==>
        [StructHeader "NullableField"
         FieldStop
         StructEnd]                             

    [<Test>]
    member __.``Nullable field, set``() =
        StructWithNullableField( Field = nullable 112233 )
        ==>
        [StructHeader "NullableField"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 112233
         FieldEnd
         FieldStop
         StructEnd]