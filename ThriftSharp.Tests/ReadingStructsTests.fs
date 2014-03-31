// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Reading structs``

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
    [<ThriftField(3s, true, "Field3")>]
    member val Field3 = Enum.A with get, set

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

[<ThriftStruct("NullableFieldWithDefault")>]
type StructWithNullableFieldWithDefault() =
    [<ThriftField(1s, false, "Field")>]
    [<ThriftDefaultValue(42)>]
    member val Field = System.Nullable() with get, set


let (==>) data (checker: 'a -> unit) = run <| async {
    let m = MemoryProtocol(data)
    let! inst = readAsync<'a> m
    m.IsEmpty <=> true
    do checker inst
}


[<TestContainer>]
type __() =
    [<Test>]
    member __.``No fields``() =
        [StructHeader "NoFields"
         FieldStop
         StructEnd]
        ==>
        fun (_: StructWithoutFields) -> ()

    [<Test>]
    member __.``One field``() =
        [StructHeader "OneField"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 34
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithOneField) ->
           inst.Field <=> 34

    [<Test>]
    member __.``Primitive fields``() =
        [StructHeader "ManyPrimitiveFields"
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
         StructEnd]
        ==>
        fun (inst: StructWithManyPrimitiveFields) ->
            inst.Bool <=> false
            inst.SByte <=> -1y
            inst.Double <=> -1.0
            inst.Int16 <=> -1s
            inst.Int32 <=> -1
            inst.Int64 <=> -1L
            inst.String <=> "xyzzy"
            inst.Binary <=> [| 2y; 3y |]

    [<Test>]
    member __.``Collection fields``() =
        [StructHeader "CollectionFields"
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
         StructEnd]
        ==>
        fun (inst: StructWithCollectionFields) ->
            inst.List <===> [1]
            inst.Set <===> [2; 3; 4]
            inst.Map <===> dict([5, "Five"; 6, "Six"])

    [<Test>]
    member __.``Struct field``() =
        [StructHeader "StructField"
         FieldHeader (1s, "StructField", tid 12uy)
         StructHeader "OneField"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 23
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithStructField) ->
            inst.Struct.Field <=> 23

    [<Test>]
    member __.``Enum fields``() =
        [StructHeader "EnumFields"
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
         StructEnd]
        ==>
        fun (inst: StructWithEnumFields) ->
            inst.Field1 <=> Enum.B
            inst.Field2 <=> LanguagePrimitives.EnumOfValue(0)
            inst.Field3 <=> Enum.A

    [<Test>]
    member __.``Array fields``() =
        [StructHeader "ArrayFields"
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
         StructEnd]
        ==>
        fun (inst: StructWithArrayFields) ->
            inst.Field1 <===> [23; 42]
            inst.Field2 <===> []

    [<Test>]
    member __.``UnixDate converter``() =
        [StructHeader "ConvertingField"
         FieldHeader (1s, "UnixDate", tid 8uy)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithConvertingField) ->
            let date = inst.UnixDate
            date.ToUniversalTime() <=> utcDate(18, 12, 1994)

    [<Test>]
    member __.``Nullable field, not set``() =
        [StructHeader "NullableField"
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithNullableField) ->
            inst.Field <=> System.Nullable()

    [<Test>]
    member __.``Nullable field, set``() =
        [StructHeader "NullableField"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 12345
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithNullableField) ->
            inst.Field <=> nullable 12345

    [<Test>]
    member __.``Nullable field, not set, with default value``() =
        [StructHeader "NullableFieldWithDefault"
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithNullableFieldWithDefault) ->
            inst.Field <=> nullable 42