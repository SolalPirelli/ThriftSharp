// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Structs``

open System.Collections.Generic
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals


[<ThriftEnum>]
type SimpleEnum = A = 1 | B = 2

[<ThriftStruct("SimpleStruct")>]
type SimpleStruct() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 0 with get, set

// used for the skipping tests
[<ThriftStruct("Struct")>]
type StructWithOneField() =
    [<ThriftField(0s, false, "Field")>]
    member val Field = nullable 42 with get, set

[<ThriftStruct("StructField")>]
type StructWithStructField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = Unchecked.defaultof<SimpleStruct> with get, set

[<ThriftStruct("ConvertedField")>]
type StructWithConvertedField() =
    [<ThriftField(1s, true, "Field", Converter = typeof<ThriftUnixDateConverter>)>]
    member val Field = System.DateTime.Now with get, set

[<ThriftStruct("NullableConvertedField")>]
type StructWithNullableConvertedField() =
    [<ThriftField(1s, false, "Field", Converter = typeof<ThriftUnixDateConverter>)>]
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


[<AbstractClass>]
type Tests() =
    abstract Test: fieldData: ThriftProtocolValue list -> typeId: int -> value: 'a -> unit
    abstract TestStruct: data: ThriftProtocolValue list -> value: 'a -> unit

    // Primitive fields
    [<Test>] member x.Bool() = x.Test              [Bool true]         2  true
    [<Test>] member x.SByte() = x.Test             [SByte 1y]          3  1y
    [<Test>] member x.Double() = x.Test            [Double 1.0]        4  1.0
    [<Test>] member x.Int16() = x.Test             [Int16 1s]          6  1s
    [<Test>] member x.Int32() = x.Test             [Int32 1]           8  1
    [<Test>] member x.Int64() = x.Test             [Int64 1L]         10  1L
    [<Test>] member x.String() = x.Test            [String "x"]       11  "x"   
    [<Test>] member x.Binary() = x.Test            [Binary [| 1y |]]  11  [| 1y |]
    [<Test>] member x.Nullable() = x.Test          [Int32 1]           8  (nullable 1)
    [<Test>] member x.Enum() = x.Test              [Int32 1]           8  SimpleEnum.A
    [<Test>] member x.``Nullable enum``() = x.Test [Int32 2]           8  (nullable SimpleEnum.B)

    // Collection fields
    [<Test>] member x.List() = x.Test             [ListHeader (1, tid 8); Int32 1; ListEnd]                  15  (List [1])
    [<Test>] member x.``List, empty``() = x.Test  [ListHeader (0, tid 8); ListEnd]                           15  (List<int>())
    [<Test>] member x.Set() = x.Test              [SetHeader (1, tid 8); Int32 1; SetEnd]                    14  (HashSet [1])
    [<Test>] member x.``Set, empty``() = x.Test   [SetHeader (0, tid 8); SetEnd]                             14  (HashSet<int>())
    [<Test>] member x.Map() = x.Test              [MapHeader (1, tid 8, tid 10); Int32 1; Int64 1L; MapEnd]  13  (dict [1, 1L])
    [<Test>] member x.``Map, empty``() = x.Test   [MapHeader (0, tid 8, tid 10); MapEnd]                     13  (Dictionary<int, int64>())
    [<Test>] member x.Array() = x.Test            [ListHeader (1, tid 8); Int32 1; ListEnd]                  15  [| 1 |]
    [<Test>] member x.``Array, empty``() = x.Test [ListHeader (0, tid 8); ListEnd]                           15  Array.empty<int>

    // Collections of structs
    [<Test>]
    member x.``List of struct``() =
        x.Test
            [ListHeader (1, tid 12)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             ListEnd]
            15
            (List [SimpleStruct(Field = 1)])

    [<Test>]
    member x.``Set of struct``() =
        x.Test
            [SetHeader (1, tid 12)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             SetEnd]
            14
            (HashSet [SimpleStruct(Field = 1)])

    [<Test>]
    member x.``Map of struct``() =
        x.Test
            [MapHeader (1, tid 12, tid 12)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", tid 8)
             Int32 2
             FieldEnd
             FieldStop
             StructEnd
             MapEnd]
            13
            (dict [SimpleStruct(Field = 1), SimpleStruct(Field = 2)])

    [<Test>]
    member x.``Array of struct``() =
        x.Test
            [ListHeader (1, tid 12)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             ListEnd]
            15
            [| SimpleStruct(Field = 1) |]
         
    // Collections of enums
    [<Test>]
    member x.``List of enum``() =
        x.Test
            [ListHeader (1, tid 8)
             Int32 1
             ListEnd]
            15
            (List [SimpleEnum.A])

    [<Test>]
    member x.``Set of enum``() =
        x.Test
            [SetHeader (1, tid 8)
             Int32 1
             SetEnd]
            14
            (HashSet [SimpleEnum.A])
            
    [<Test>]
    member x.``Map of enum``() =
        x.Test
            [MapHeader (1, tid 8, tid 8)
             Int32 1
             Int32 2
             MapEnd]
            13
            (dict [SimpleEnum.A, SimpleEnum.B])
            
    [<Test>]
    member x.``Array of enum``() =
        x.Test
            [ListHeader (1, tid 8)
             Int32 1
             ListEnd]
            15
            [| SimpleEnum.A |]

    // Struct fields
    [<Test>]
    member x.``Struct``() =
        x.Test
            [StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd]
            12
            (SimpleStruct(Field = 1))

    // Converted fields
    [<Test>]
    member x.``Converted``() =
        x.TestStruct
            [StructHeader "ConvertedField"
             FieldHeader (1s, "Field", tid 8)
             Int32 787708800
             FieldEnd
             FieldStop
             StructEnd]
            (StructWithConvertedField(Field = date(18, 12, 1994)))

    [<Test>]
    member x.``Converted nullable``() =
        x.TestStruct
            [StructHeader "NullableConvertedField"
             FieldHeader (1s, "Field", tid 8)
             Int32 787708800
             FieldEnd
             FieldStop
             StructEnd]
            (StructWithNullableConvertedField(Field = nullable(date(18, 12, 1994))))

    // Unset nullable fields
    [<Test>]
    member x.``Nullable, not set``() =
        x.TestStruct
            [StructHeader "NullableField"
             FieldStop
             StructEnd]
            (StructWithNullableField(Field = System.Nullable()))

    [<Test>]
    member x.``Nullable with default value, not set``() =
        x.TestStruct
            [StructHeader "NullableFieldWithDefault"
             FieldStop
             StructEnd]
            (StructWithNullableFieldWithDefault(Field = nullable 42))

    [<Test>]
    member x.``Nullable with default value, set``() =
        x.TestStruct
            [StructHeader "NullableFieldWithDefault"
             FieldHeader (1s, "Field", tid 8)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd]
            (StructWithNullableFieldWithDefault(Field = nullable 1))


[<TestClass>]
type Reading() =
    inherit Tests()

    let fails fieldType typeId fieldData =
        let typ = makeClass [ <@ ThriftStructAttribute("Struct") @> ] [ "Field", fieldType, [ <@ ThriftFieldAttribute(1s, true, "Field") @> ] ]
        let data = [StructHeader "Struct"; FieldHeader (1s, "Field", tid typeId)] @ fieldData @ [FieldEnd; FieldStop; StructEnd] 
        let m = MemoryProtocol(data)
        let thriftStruct = ThriftAttributesParser.ParseStruct(typ.GetTypeInfo())
        throws<ThriftSerializationException> (fun () -> ThriftStructReader.Read(thriftStruct, m) |> box) |> ignore


    override x.Test fieldData typeId (value: 'a) =
        let isReq = typeof<'a>.IsValueType && System.Nullable.GetUnderlyingType(typeof<'a>) = null
        
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
        let structInst = ThriftStructReader.Read(thriftStruct, m)
        m.IsEmpty <=> true
        (typ.GetProperty("Field").GetValue(structInst) :?> 'a) <=> value

    override x.TestStruct data (value: 'a) =
        let m = MemoryProtocol(data)
        let inst = read<'a> m
        m.IsEmpty <=> true
        inst <=> value


    // Read-only tests    
    [<Test>]
    member __.``Error on missing required field``() =
        let data = 
            [StructHeader "StructField"
             FieldStop
             StructEnd]
        let m = MemoryProtocol(data)
        throws<ThriftSerializationException> (fun () -> read<StructWithStructField> m |> box) |> ignore

    [<Test>]
    member __.``ThriftSerializationException is thrown when the field type doesn't match its declaration``() =
        fails typeof<int> 9 [Int64 0L]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the list element type doesn't match its declaration``() =
        fails typeof<List<int>> 15 [ListHeader (1, tid 9); Int64 1L]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the set element type doesn't match its declaration``() =
        fails typeof<HashSet<int>> 14 [SetHeader (3, tid 9); Int64 1L]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the map key type doesn't match its declaration``() =
        fails typeof<Dictionary<int, string>> 13 [MapHeader (1, tid 9, tid 11); Int64 1L; String "x"]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the map value type doesn't match its declaration``() =
        fails typeof<Dictionary<int, string>> 13 [MapHeader (1, tid 8, tid 9); Int32 1; Int64 1L; MapEnd]

    [<Test>]
    member __.``ThriftSerializationException is thrown when the array value type doesn't match its declaration``() =
        fails typeof<int[]> 15 [ListHeader (1, tid 9); Int64 1L; ListEnd]


[<TestClass>]
type Writing() =
    inherit Tests()

    override x.Test fieldData typeId (value: 'a) =
        let isReq = typeof<'a>.IsValueType && System.Nullable.GetUnderlyingType(typeof<'a>) = null
        
        let typ = 
            makeClass 
                [ <@ ThriftStructAttribute("Struct") @> ] 
                [ "Field", typeof<'a>, [ <@ ThriftFieldAttribute(1s, isReq, "Field") @> ] ]
        let inst = System.Activator.CreateInstance(typ)
        typ.GetProperty("Field").SetValue(inst, value)
        
        let m = MemoryProtocol()
        write m inst

        m.WrittenValues 
        <=>
        [StructHeader "Struct"; FieldHeader (1s, "Field", tid typeId)]
      @ fieldData 
      @ [FieldEnd; FieldStop; StructEnd]

    override x.TestStruct data (value: 'a) =
        let m = MemoryProtocol()
        write m value
        m.WrittenValues <=> data


    // Write-only tests

    [<Test>]
    member __.``Error on required but unset field``() = run <| async {
        do! throwsAsync<ThriftSerializationException>(async {write (MemoryProtocol()) (StructWithStructField()); return obj()}) |> Async.Ignore
    }

[<TestClass>]
type Skipping() =
    inherit Tests()
    
    override x.Test fieldData typeId (value: 'a) =
        let data = 
            [StructHeader "Struct"; 
             FieldHeader (1s, "Field", tid typeId)] 
          @ fieldData 
          @ [FieldEnd; FieldStop; StructEnd]

        let m = MemoryProtocol(data)
        read<StructWithOneField> m |> ignore
        m.IsEmpty <=> true

    override x.TestStruct data (value: 'a) =
        () // not applicable