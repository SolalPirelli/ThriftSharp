// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Reading structs``

open System.Collections.Generic
open ThriftSharp

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
    
[<ThriftStruct("ListField")>]
type StructWithListField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = List<int>() with get, set

[<ThriftStruct("SetField")>]
type StructWithSetField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = HashSet<int>() with get, set

[<ThriftStruct("MapField")>]
type StructWithMapField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = Dictionary<int, string>() with get, set

[<ThriftStruct("StructField")>]
type StructWithStructField() =
    [<ThriftField(1s, true, "StructField")>]
    member val Field = StructWithOneField() with get, set

[<ThriftEnum>]
type Enum =
    | A = 1
    | B = 2

[<ThriftStruct("EnumField")>]
type StructWithEnumField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = Enum.A with get, set

[<ThriftStruct("NullableEnumField")>]
type StructWithNullableEnumField() =
    [<ThriftField(1s, false, "Field")>]
    member val Field = nullable Enum.A with get, set

[<ThriftStruct("ArrayField")>]
type StructWithArrayField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = [| Enum.B |] with get, set

[<ThriftStruct("ConvertedField")>]
type StructWithConvertedField() =
    [<ThriftField(1s, true, "UnixDate")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    member val UnixDate = System.DateTime.Now with get, set

    [<ThriftField(2s, false, "NullableUnixDate")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    member val NullableUnixDate = nullable System.DateTime.Now with get, set

[<ThriftStruct("NullableField")>]
type StructWithNullableField() =
    [<ThriftField(1s, false, "Field")>]
    member val Field = System.Nullable() with get, set

[<ThriftStruct("NullableFieldWithDefault")>]
type StructWithNullableFieldWithDefault() =
    [<ThriftField(1s, false, "Field")>]
    [<ThriftDefaultValue(42)>]
    member val Field = System.Nullable() with get, set


let (==>) data (checker: 'a -> unit) =
    let m = MemoryProtocol(data)
    let inst = read<'a> m
    m.IsEmpty <=> true
    checker inst

let fails<'data> data =
    let m = MemoryProtocol(data)
    throws<ThriftSerializationException> (fun () -> read<'data> m |> box) |> ignore

[<TestContainer>]
type __() =
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
    member __.``List field``() =
        [StructHeader "ListField"
         FieldHeader (1s, "Field", tid 15uy)
         ListHeader (1, tid 8uy)
         Int32 1
         ListEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithListField) ->
            inst.Field <===> [1]

    [<Test>]
    member __.``List field, empty``() =
        [StructHeader "ListField"
         FieldHeader (1s, "Field", tid 15uy)
         ListHeader (0, tid 8uy)
         ListEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithListField) ->
            inst.Field <===> []

    [<Test>]
    member __.``Set field``() =
        [StructHeader "SetField"
         FieldHeader (1s, "Field", tid 14uy)
         SetHeader (3, tid 8uy)
         Int32 2
         Int32 3
         Int32 4
         SetEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithSetField) ->
            inst.Field <===> [2; 3; 4]

    [<Test>]
    member __.``Set field, empty``() =
        [StructHeader "SetField"
         FieldHeader (1s, "Field", tid 14uy)
         SetHeader (0, tid 8uy)
         SetEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithSetField) ->
            inst.Field <===> []

    [<Test>]
    member __.``Map field``() =
        [StructHeader "MapField"
         FieldHeader (1s, "Field", tid 13uy)
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
        fun (inst: StructWithMapField) ->
            inst.Field <===> dict([5, "Five"; 6, "Six"])

    [<Test>]
    member __.``Map field, empty``() =
        [StructHeader "MapField"
         FieldHeader (1s, "Field", tid 13uy)
         MapHeader (0, tid 8uy, tid 11uy)
         MapEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithMapField) ->
            inst.Field <===> dict([])

    [<Test>]
    member __.``Struct field``() =
        [StructHeader "StructField"
         FieldHeader (1s, "Field", tid 12uy)
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
            inst.Field.Field <=> 23

    [<Test>]
    member __.``Enum field``() =
        [StructHeader "EnumFields"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 1
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithEnumField) ->
            inst.Field <=> Enum.A

    [<Test>]
    member __.``Nullable enum field``() =
        [StructHeader "NullableEnumField"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 2
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithNullableEnumField) ->
            inst.Field <=> nullable Enum.B

    [<Test>]
    member __.``Array field``() =
        [StructHeader "ArrayField"
         FieldHeader (1s, "Field", tid 15uy)
         ListHeader (1, tid 8uy)
         Int32 1
         ListEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithArrayField) ->
            inst.Field <===> [ Enum.A ]

    [<Test>]
    member __.``Array field, empty``() =
        [StructHeader "ArrayField"
         FieldHeader (1s, "Field", tid 15uy)
         ListHeader (0, tid 8uy)
         ListEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithArrayField) ->
            inst.Field <===> []

    [<Test>]
    member __.``Converted field``() =
        [StructHeader "ConvertingField"
         FieldHeader (1s, "UnixDate", tid 8uy)
         Int32 787708800
         FieldEnd
         FieldHeader (2s, "NullableUnixDate", tid 8uy)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithConvertedField) ->
            let date = inst.UnixDate
            date.ToUniversalTime() <=> utcDate(18, 12, 1994)

            let nullableDate = inst.NullableUnixDate
            nullableDate.HasValue <=> true
            let date2 = nullableDate.Value
            date2.ToUniversalTime() <=> utcDate(18, 12, 1994)

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

    [<Test>]
    member __.``ThriftSerializationException is thrown when the field type doesn't match its declaration``() =
        fails<StructWithOneField>
            [StructHeader "OneField"
             FieldHeader (1s, "Field", tid 9uy)
             Int64 0L
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the list element type doesn't match its declaration``() =
        fails<StructWithListField>
            [StructHeader "ListField"
             FieldHeader (1s, "Field", tid 15uy)
             ListHeader (1, tid 9uy)
             Int64 1L
             ListEnd
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the set element type doesn't match its declaration``() =
        fails<StructWithSetField>
            [StructHeader "SetField"
             FieldHeader (1s, "Field", tid 14uy)
             SetHeader (3, tid 9uy)
             Int64 2L
             Int64 3L
             Int64 4L
             SetEnd
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the map key type doesn't match its declaration``() =
        fails<StructWithMapField>
            [StructHeader "MapField"
             FieldHeader (1s, "Field", tid 13uy)
             MapHeader (2, tid 9uy, tid 11uy)
             Int64 5L
             String "Five"
             Int64 6L
             String "Six"
             MapEnd
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the map value type doesn't match its declaration``() =
        fails<StructWithMapField>
            [StructHeader "MapField"
             FieldHeader (1s, "Field", tid 13uy)
             MapHeader (2, tid 8uy, tid 9uy)
             Int32 5
             Int64 10L
             Int32 6
             Int64 11L
             MapEnd
             FieldEnd
             FieldStop
             StructEnd]


    [<Test>]
    member __.``ThriftSerializationException is thrown when the array value type doesn't match its declaration``() =
        fails<StructWithArrayField>
            [StructHeader "ArrayField"
             FieldHeader (1s, "Field", tid 15uy)
             ListHeader (1, tid 9uy)
             Int64 1L
             ListEnd
             FieldEnd
             FieldStop
             StructEnd]