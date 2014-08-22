// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Reading structs``

open System.Collections.Generic
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals


[<ThriftEnum>]
type SimpleEnum =
    | A = 1
    | B = 2

[<ThriftStruct("SimpleStruct")>]
type SimpleStruct() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 0 with get, set

[<ThriftStruct("StructField")>]
type StructWithStructField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = SimpleStruct() with get, set

[<ThriftStruct("ConvertedField")>]
type StructWithConvertedField() =
    [<ThriftField(1s, true, "UnixDate")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    member val Field = System.DateTime.Now with get, set

[<ThriftStruct("NullableConvertedField")>]
type StructWithNullableConvertedField() =
    [<ThriftField(1s, false, "NullableUnixDate")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    member val Field = nullable System.DateTime.Now with get, set

[<ThriftStruct("NullableField")>]
type StructWithNullableField() =
    [<ThriftField(1s, false, "Field")>]
    member val Field = System.Nullable() with get, set

[<ThriftStruct("NullableFieldWithDefault")>]
type StructWithNullableFieldWithDefault() =
    [<ThriftField(1s, false, "Field")>]
    [<ThriftDefaultValue(42)>]
    member val Field = System.Nullable() with get, set

let (--) a b = (a, b)

let (-->) (fieldData, typeId) (expected: 'a) =
    let isReq = typeof<'a>.IsValueType && not typeof<'a>.IsGenericType // enough to detect nullables

    let typ = 
        makeClass 
            [ <@ ThriftStructAttribute("Struct") @> ] 
            [ "Field", typeof<'a>, [ <@ ThriftFieldAttribute(1s, isReq, "Field") @> ] ]

    let data = 
        [StructHeader "Struct"; FieldHeader (1s, "Field", tid typeId)]
      @ fieldData 
      @ [FieldEnd; FieldStop; StructEnd]

    let m = MemoryProtocol(data)
    let thriftStruct = ThriftAttributesParser.ParseStruct(typ.GetTypeInfo())
    let inst = ThriftReader.Read(thriftStruct, m)
    m.IsEmpty <=> true
    expected <=> (typ.GetProperty("Field").GetValue(inst) :?> 'a)

let (==>) data expected (checker: 'a -> unit) =
    let m = MemoryProtocol(data)
    let inst = read<'a> m
    m.IsEmpty <=> true
    checker expected

let fails<'field> typeId fieldData =
    let typ = makeClass [ <@ ThriftStructAttribute("Struct") @> ] [ "Field", typeof<'field>, [ <@ ThriftFieldAttribute(1s, true, "Field") @> ] ]
    let data = [StructHeader "Struct"; FieldHeader (1s, "Field", tid typeId)] @ fieldData @ [FieldEnd; FieldStop; StructEnd] 
    let m = MemoryProtocol(data)
    let thriftStruct = ThriftAttributesParser.ParseStruct(typ.GetTypeInfo())
    throws<ThriftSerializationException> (fun () -> ThriftReader.Read(thriftStruct, m) |> box) |> ignore

[<TestContainer>]
type __() =
    // Primitive fields
    [<Test>] member __.``Bool``() =          [Bool true]       --  2 --> true
    [<Test>] member __.``SByte``() =         [SByte 1y]        --  3 --> 1y
    [<Test>] member __.``Double``() =        [Double 1.0]      --  4 --> 1.0
    [<Test>] member __.``Int16``() =         [Int16 1s]        --  6 --> 1s
    [<Test>] member __.``Int32``() =         [Int32 1]         --  8 --> 1
    [<Test>] member __.``Int64``() =         [Int64 1L]        -- 10 --> 1L
    [<Test>] member __.``String``() =        [String "x"]      -- 11 --> "x"   
    [<Test>] member __.``Binary``() =        [Binary [| 1y |]] -- 11 --> [| 1y |]
    [<Test>] member __.``Nullable``() =      [Int32 1]         --  8 --> nullable 1
    [<Test>] member __.``Enum``() =          [Int32 1]         --  8 --> SimpleEnum.A
    [<Test>] member __.``Nullable enum``() = [Int32 2]         --  8 --> nullable SimpleEnum.B

    // Collection fields
    [<Test>] member __.``List``() =         [ListHeader (1, tid 8); Int32 1; ListEnd]                     -- 15 --> List([1])
    [<Test>] member __.``List, empty``() =  [ListHeader (0, tid 8); ListEnd]                              -- 15 --> List<int>()
    [<Test>] member __.``Set``() =          [SetHeader (1, tid 8); Int32 1; SetEnd]                       -- 14 --> HashSet([1])
    [<Test>] member __.``Set, empty``() =   [SetHeader (0, tid 8); SetEnd]                                -- 14 --> HashSet<int>()
    [<Test>] member __.``Map``() =          [MapHeader (1, tid 8, tid 11); Int32 1; String "x"; MapEnd] -- 13 --> dict([1, "x"])
    [<Test>] member __.``Map, empty``() =   [MapHeader (0, tid 8, tid 11); MapEnd]                      -- 13 --> Dictionary<int, string>()
    [<Test>] member __.``Array``() =        [ListHeader (1, tid 8); Int32 1; ListEnd]                     -- 15 --> [| 1 |]
    [<Test>] member __.``Array, empty``() = [ListHeader (0, tid 8); ListEnd]                              -- 15 --> Array.empty<int>


    // Other special cases

    [<Test>]
    member __.``Struct``() =
        [StructHeader "StructField"
         FieldHeader (1s, "Field", tid 12)
         StructHeader "SimpleStruct"
         FieldHeader (1s, "Field", tid 8)
         Int32 1
         FieldEnd
         FieldStop
         StructEnd
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithStructField) ->
            inst.Field.Field = 1

    [<Test>]
    member __.``Converted``() =
        [StructHeader "ConvertingField"
         FieldHeader (1s, "Field", tid 8)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithConvertedField) ->
            let date = inst.Field
            date.ToUniversalTime() <=> utcDate(18, 12, 1994)

    [<Test>]
    member __.``Converted nullable``() =
        [StructHeader "ConvertingField"
         FieldHeader (1s, "Field", tid 8)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithNullableConvertedField) ->
            let nullableDate = inst.Field
            nullableDate.HasValue <=> true
            let date2 = nullableDate.Value
            date2.ToUniversalTime() <=> utcDate(18, 12, 1994)

    [<Test>]
    member __.``Nullable, not set``() =
        [StructHeader "NullableField"
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithNullableField) ->
            inst.Field <=> System.Nullable()

    [<Test>]
    member __.``Nullable, not set, with default value``() =
        [StructHeader "NullableFieldWithDefault"
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithNullableFieldWithDefault) ->
            inst.Field <=> nullable 42

    [<Test>]
    member __.``ThriftSerializationException is thrown when the field type doesn't match its declaration``() =
        fails<int> 9 [Int64 0L]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the list element type doesn't match its declaration``() =
        fails<List<int>> 15 [ListHeader (1, tid 9); Int64 1L]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the set element type doesn't match its declaration``() =
        fails<HashSet<int>> 14 [SetHeader (3, tid 9); Int64 1L]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the map key type doesn't match its declaration``() =
        fails<Dictionary<int, string>> 13 [MapHeader (1, tid 9, tid 11); Int64 1L; String "x"]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the map value type doesn't match its declaration``() =
        fails<Dictionary<int, string>> 13 [MapHeader (1, tid 8, tid 9); Int32 1; Int64 1L; MapEnd]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the array value type doesn't match its declaration``() =
        fails<int[]> 15 [ListHeader (1, tid 9); Int64 1L; ListEnd]
