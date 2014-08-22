﻿// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Writing structs``

open System.Collections.Generic
open ThriftSharp

[<ThriftStruct("OneField"); AllowNullLiteral>]
type StructWithOneField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 0 with get, set

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
    [<ThriftField(7s, false, "StringField")>]
    member val String = "abc" with get, set
    [<ThriftField(8s, false, "BinaryField")>]
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
    member val Struct = null :> StructWithOneField with get, set

[<ThriftEnum>]
type Enum =
    | A = 1
    | B = 3

[<ThriftStruct("EnumFields")>]
type StructWithEnumFields() =
    [<ThriftField(1s, true, "Field1")>]
    member val Field1 = Enum.A with get, set
    [<ThriftField(2s, false, "Field2")>]
    member val Field2 = nullable Enum.A with get, set

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
    m.WrittenValues <=> data

let throws<'E when 'E :> System.Exception> obj =
    throwsAsync<'E>(fun () -> async { write (MemoryProtocol()) obj; return System.Object() }) |> run


[<TestContainer>]
type __() =
    [<Test>]
    member __.``One primitive field``() =
        StructWithOneField( Field = 42 )
        ==>
        [StructHeader "OneField"
         FieldHeader (1s, "Field", tid 8)
         Int32 42
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Primitive fields``() =
        StructWithManyPrimitiveFields()
        ==>
        [StructHeader "ManyPrimitiveFields"
         FieldHeader (1s, "BoolField", tid 2)
         Bool true
         FieldEnd
         FieldHeader (2s, "SByteField", tid 3)
         SByte 1y
         FieldEnd
         FieldHeader (3s, "DoubleField", tid 4)
         Double 1.0
         FieldEnd
         FieldHeader (4s, "Int16Field", tid 6)
         Int16 1s
         FieldEnd
         FieldHeader (5s, "Int32Field", tid 8)
         Int32 1
         FieldEnd
         FieldHeader (6s, "Int64Field", tid 10)
         Int64 1L
         FieldEnd
         FieldHeader (7s, "StringField", tid 11)
         String "abc"
         FieldEnd
         FieldHeader (8s, "BinaryField", tid 11)
         Binary [| 1y |]
         FieldEnd
         FieldStop
         StructEnd]
         
    [<Test>]
    member __.``Primitive fields default values``() =
        StructWithManyPrimitiveFields( Bool = false, SByte = 0y, Double = 0.0, Int16 = 0s, Int32 = 0, Int64 = 0L, String = null, Binary = null )
        ==>
        [StructHeader "ManyPrimitiveFields"
         FieldHeader (1s, "BoolField", tid 2)
         Bool false
         FieldEnd
         FieldHeader (2s, "SByteField", tid 3)
         SByte 0y
         FieldEnd
         FieldHeader (3s, "DoubleField", tid 4)
         Double 0.0
         FieldEnd
         FieldHeader (4s, "Int16Field", tid 6)
         Int16 0s
         FieldEnd
         FieldHeader (5s, "Int32Field", tid 8)
         Int32 0
         FieldEnd
         FieldHeader (6s, "Int64Field", tid 10)
         Int64 0L
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Collection fields``() =
        StructWithCollectionFields( List = List([4; 8; 15]), Set = HashSet([16; 23]), Map = dict([42, "Lost"]) )
        ==>
        [StructHeader "CollectionFields"
         FieldHeader (1s, "ListField", tid 15)
         ListHeader (3, tid 8)
         Int32 4
         Int32 8
         Int32 15
         ListEnd
         FieldEnd
         FieldHeader (2s, "SetField", tid 14)
         SetHeader (2, tid 8)
         Int32 16
         Int32 23
         SetEnd
         FieldEnd
         FieldHeader (3s, "MapField", tid 13)
         MapHeader (1, tid 8, tid 11)
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
         FieldHeader (1s, "StructField", tid 12)
         StructHeader "OneField"
         FieldHeader (1s, "Field", tid 8)
         Int32 777
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd]
     
    [<Test>]
    member __.``Error on required but unset struct field``() =
        throws<ThriftSerializationException> (StructWithStructField())

    [<Test>]
    member __.``Enum fields``() =
        StructWithEnumFields( Field1 = Enum.A, Field2 = nullable Enum.B )
        ==>
        [StructHeader "EnumFields"
         FieldHeader (1s, "Field1", tid 8)
         Int32 1
         FieldEnd
         FieldHeader (2s, "Field2", tid 8)
         Int32 3
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Array fields``() =
        StructWithArrayFields( Field1 = [| 12345; 67890 |], Field2 = [| |] )
        ==>
        [StructHeader "ArrayFields"
         FieldHeader (1s, "Field1", tid 15)
         ListHeader (2, tid 8)
         Int32 12345
         Int32 67890
         ListEnd
         FieldEnd
         FieldHeader (2s, "Field2", tid 15)
         ListHeader (0, tid 8)
         ListEnd
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``UnixDate converter``() =
        StructWithConvertingField( UnixDate = utcDate(18, 12, 1994) )
        ==>
        [StructHeader "ConvertingField"
         FieldHeader (1s, "UnixDate", tid 8)
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
         FieldHeader (1s, "Field", tid 8)
         Int32 112233
         FieldEnd
         FieldStop
         StructEnd]