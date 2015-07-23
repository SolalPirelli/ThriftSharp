// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Structs``

open System.Collections.Generic
open System.Reflection
open Xunit
open ThriftSharp
open ThriftSharp.Internals


[<ThriftEnum>]
type SimpleEnum = A = 1 | B = 2

// used for the skipping tests
[<ThriftStruct("StructWithoutFields")>]
type StructWithoutFields() =
    class end
    
[<ThriftStruct("SimpleStruct")>]
type SimpleStruct() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 0 with get, set

    // DeepEquals doesn't like comparing this struct inside a dictionary for some reason, so this is needed
    override x.Equals(other) =
        match other with
        | :? SimpleStruct as s -> x.Field = s.Field
        | _ -> false

    override x.GetHashCode() =
        let f = x.Field
        f.GetHashCode()

[<ThriftStruct("StructField")>]
type StructWithStructField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = Unchecked.defaultof<SimpleStruct> with get, set


// A field can be:
// - of a value or reference type
// - required or not
// - with a converter or not
//   - if so, the wire type can be a value or reference type
// - with a default value or not
// That's 24 combinations. Gotta test'em all!
// [value/reference] [required/optional] without a converter nor a default value are already tested by less specific tests
// required with default fields don't make sense
// That's a total of 14 things we need to test separately.

type IntToIntConverter() =
    interface IThriftValueConverter<int, int> with
        member x.Convert(i) = 10 + i
        member x.ConvertBack(i) = i - 10

type IntToStringConverter() =
    interface IThriftValueConverter<int, string> with
        member x.Convert(i) = i.ToString()
        member x.ConvertBack(s) = System.Int32.Parse(s)
        
type StringToIntConverter() =
    interface IThriftValueConverter<string, int> with
        member x.Convert(s) = System.Int32.Parse(s)
        member x.ConvertBack(i) = i.ToString()

type StringToStringConverter() =
    interface IThriftValueConverter<string, string> with
        member x.Convert(s) = if s = null then null else "0" + s
        member x.ConvertBack(s) = if s = null then null else s.Substring(1)
        
[<ThriftStruct("S")>]
type Struct_ValueOptionalFieldWithDefault() =
    [<ThriftField(1s, false, "F", DefaultValue = 1)>]
    member val Field = 0 with get, set

[<ThriftStruct("S")>]
type Struct_ValueRequiredConvertedToValueField() =
    [<ThriftField(1s, true, "F", Converter = typeof<IntToIntConverter>)>]
    member val Field = 0 with get, set

[<ThriftStruct("S")>]
type Struct_ValueOptionalConvertedToValueField() =
    [<ThriftField(1s, false, "F", Converter = typeof<IntToIntConverter>)>]
    member val Field = System.Nullable<int>() with get, set

[<ThriftStruct("S")>]
type Struct_ValueOptionalConvertedToValueFieldWithDefault() =
    [<ThriftField(1s, false, "F", DefaultValue = 1, Converter = typeof<IntToIntConverter>)>]
    member val Field = 0 with get, set

[<ThriftStruct("S")>]
type Struct_ValueRequiredConvertedToRefField() =
    [<ThriftField(1s, true, "F", Converter = typeof<StringToIntConverter>)>]
    member val Field = 0 with get, set

[<ThriftStruct("S")>]
type Struct_ValueOptionalConvertedToRefField() =
    [<ThriftField(1s, false, "F", Converter = typeof<StringToIntConverter>)>]
    member val Field = System.Nullable<int>() with get, set

[<ThriftStruct("S")>]
type Struct_ValueOptionalConvertedToRefFieldWithDefault() =
    [<ThriftField(1s, false, "F", DefaultValue = "1", Converter = typeof<StringToIntConverter>)>]
    member val Field = 0 with get, set
        
[<ThriftStruct("S")>]
type Struct_RefOptionalFieldWithDefault() =
    [<ThriftField(1s, false, "F", DefaultValue = "1")>]
    member val Field = Unchecked.defaultof<string> with get, set

[<ThriftStruct("S")>]
type Struct_RefRequiredConvertedToValueField() =
    [<ThriftField(1s, true, "F", Converter = typeof<IntToStringConverter>)>]
    member val Field = Unchecked.defaultof<string> with get, set

[<ThriftStruct("S")>]
type Struct_RefOptionalConvertedToValueField() =
    [<ThriftField(1s, false, "F", Converter = typeof<IntToStringConverter>)>]
    member val Field = Unchecked.defaultof<string> with get, set

[<ThriftStruct("S")>]
type Struct_RefOptionalConvertedToValueFieldWithDefault() =
    [<ThriftField(1s, false, "F", DefaultValue = 1, Converter = typeof<IntToStringConverter>)>]
    member val Field = Unchecked.defaultof<string> with get, set

[<ThriftStruct("S")>]
type Struct_RefRequiredConvertedToRefField() =
    [<ThriftField(1s, true, "F", Converter = typeof<StringToStringConverter>)>]
    member val Field = Unchecked.defaultof<string> with get, set

[<ThriftStruct("S")>]
type Struct_RefOptionalConvertedToRefField() =
    [<ThriftField(1s, false, "F", Converter = typeof<StringToStringConverter>)>]
    member val Field = Unchecked.defaultof<string> with get, set

[<ThriftStruct("S")>]
type Struct_RefOptionalConvertedToRefFieldWithDefault() =
    [<ThriftField(1s, false, "F", DefaultValue = "1", Converter = typeof<StringToStringConverter>)>]
    member val Field = Unchecked.defaultof<string> with get, set


// These tests use Fact rather than Theory, they're not really suited to no-name tests
[<AbstractClass>]
type Tests() =
    abstract Test: fieldData: ThriftProtocolValue list -> typeId: ThriftTypeId -> value: 'a -> unit
    abstract TestStruct: data: ThriftProtocolValue list -> value: 'a -> unit

    // Primitive fields
    [<Fact>] 
    member x.Bool() = 
        x.Test [Bool true]        
               ThriftTypeId.Boolean
               true

    [<Fact>] 
    member x.SByte() = 
        x.Test [SByte 1y]
               ThriftTypeId.SByte
               1y

    [<Fact>] 
    member x.Double() = 
        x.Test [Double 1.0]
               ThriftTypeId.Double
               1.0

    [<Fact>] 
    member x.Int16() = 
        x.Test [Int16 1s]
               ThriftTypeId.Int16
               1s

    [<Fact>] 
    member x.Int32() = 
        x.Test [Int32 1]
               ThriftTypeId.Int32
               1

    [<Fact>] 
    member x.Int64() = 
        x.Test [Int64 1L]
               ThriftTypeId.Int64
               1L

    [<Fact>] 
    member x.String() = 
        x.Test [String "x"]
               ThriftTypeId.Binary
               "x"

    [<Fact>] 
    member x.Binary() = 
        x.Test [Binary [| 1y |]]
               ThriftTypeId.Binary
               [| 1y |]

    [<Fact>] 
    member x.Nullable() = 
        x.Test [Int32 1]
               ThriftTypeId.Int32
               (nullable 1)

    [<Fact>] 
    member x.Enum() = 
        x.Test [Int32 1]
               ThriftTypeId.Int32
               SimpleEnum.A

    [<Fact>] 
    member x.``Nullable enum``() = 
        x.Test [Int32 2]
               ThriftTypeId.Int32
               (nullable SimpleEnum.B)

    // Collection fields
    [<Fact>] 
    member x.List() = 
        x.Test [ListHeader (1, ThriftTypeId.Int32)
                Int32 1
                ListEnd]
               ThriftTypeId.List
               (List [1])

    [<Fact>] 
    member x.``List, empty``() = 
        x.Test [ListHeader (0, ThriftTypeId.Int32)
                ListEnd]
               ThriftTypeId.List
               (List<int>())

    [<Fact>] 
    member x.Set() = 
        x.Test [SetHeader (1, ThriftTypeId.Int32)
                Int32 1
                SetEnd]
               ThriftTypeId.Set
               (HashSet [1])
    [<Fact>] 
    member x.``Set, empty``() = 
        x.Test [SetHeader (0, ThriftTypeId.Int32)
                SetEnd]
               ThriftTypeId.Set
               (HashSet<int>())

    [<Fact>] 
    member x.Map() = 
        x.Test [MapHeader (1, ThriftTypeId.Int32, ThriftTypeId.Int64)
                Int32 1
                Int64 1L
                MapEnd]
               ThriftTypeId.Map
               (dict [1, 1L])

    [<Fact>] 
    member x.``Map, empty``() = 
        x.Test [MapHeader (0, ThriftTypeId.Int32, ThriftTypeId.Int64)
                MapEnd]
               ThriftTypeId.Map
               (Dictionary<int, int64>())

    [<Fact>] 
    member x.Array() = 
        x.Test [ListHeader (1, ThriftTypeId.Int32)
                Int32 1
                ListEnd]
               ThriftTypeId.List
               [| 1 |]

    [<Fact>] 
    member x.``Array, empty``() = 
        x.Test [ListHeader (0, ThriftTypeId.Int32)
                ListEnd]
               ThriftTypeId.List
               Array.empty<int>

    // Collections of structs
    [<Fact>]
    member x.``List of struct``() =
        x.Test
            [ListHeader (1, ThriftTypeId.Struct)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", ThriftTypeId.Int32)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             ListEnd]
            ThriftTypeId.List
            (List [SimpleStruct(Field = 1)])

    [<Fact>]
    member x.``Set of struct``() =
        x.Test
            [SetHeader (1, ThriftTypeId.Struct)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", ThriftTypeId.Int32)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             SetEnd]
            ThriftTypeId.Set
            (HashSet [SimpleStruct(Field = 1)])

    [<Fact>]
    member x.``Map of struct``() =
        x.Test
            [MapHeader (1, ThriftTypeId.Struct, ThriftTypeId.Struct)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", ThriftTypeId.Int32)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", ThriftTypeId.Int32)
             Int32 2
             FieldEnd
             FieldStop
             StructEnd
             MapEnd]
            ThriftTypeId.Map
            (dict [SimpleStruct(Field = 1), SimpleStruct(Field = 2)])

    [<Fact>]
    member x.``Array of struct``() =
        x.Test
            [ListHeader (1, ThriftTypeId.Struct)
             StructHeader "SimpleStruct"
             FieldHeader (1s, "Field", ThriftTypeId.Int32)
             Int32 1
             FieldEnd
             FieldStop
             StructEnd
             ListEnd]
            ThriftTypeId.List
            [| SimpleStruct(Field = 1) |]
         
    // Collections of enums
    [<Fact>]
    member x.``List of enum``() =
        x.Test
            [ListHeader (1, ThriftTypeId.Int32)
             Int32 1
             ListEnd]
            ThriftTypeId.List
            (List [SimpleEnum.A])

    [<Fact>]
    member x.``Set of enum``() =
        x.Test [SetHeader (1, ThriftTypeId.Int32)
                Int32 1
                SetEnd]
               ThriftTypeId.Set
               (HashSet [SimpleEnum.A])
            
    [<Fact>]
    member x.``Map of enum``() =
        x.Test [MapHeader (1, ThriftTypeId.Int32, ThriftTypeId.Int32)
                Int32 1
                Int32 2
                MapEnd]
               ThriftTypeId.Map
               (dict [SimpleEnum.A, SimpleEnum.B])
            
    [<Fact>]
    member x.``Array of enum``() =
        x.Test [ListHeader (1, ThriftTypeId.Int32)
                Int32 1
                ListEnd]
               ThriftTypeId.List
               [| SimpleEnum.A |]

    // Struct fields
    [<Fact>]
    member x.``Struct``() =
        x.Test [StructHeader "SimpleStruct"
                FieldHeader (1s, "Field", ThriftTypeId.Int32)
                Int32 1
                FieldEnd
                FieldStop
                StructEnd]
               ThriftTypeId.Struct
               (SimpleStruct(Field = 1))

    // All of the special structs we declared earlier
    // And each optional one needs two tests!
    
    [<Fact>]
    member x. ``Value optional field with default, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalFieldWithDefault(Field = 1))

    [<Fact>]
    member x. ``Value optional field with default, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Int32)
                      Int32 2
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalFieldWithDefault(Field = 2))

    [<Fact>]
    member x. ``Value required field converted to a value type``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Int32)
                      Int32 2
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_ValueRequiredConvertedToValueField(Field = 12))

    [<Fact>]
    member x. ``Value optional field converted to a value type, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToValueField())

    [<Fact>]
    member x. ``Value optional field converted to a value type, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Int32)
                      Int32 2
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToValueField(Field = nullable 12))

    [<Fact>]
    member x. ``Value optional field converted to a value type with default, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToValueFieldWithDefault(Field = 11))

    [<Fact>]
    member x. ``Value optional field converted to a value type with default, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Int32)
                      Int32 2
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToValueFieldWithDefault(Field = 12))

    [<Fact>]
    member x. ``Value required field converted to a ref type``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Binary)
                      String "2"
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_ValueRequiredConvertedToRefField(Field = 2))

    [<Fact>]
    member x. ``Value optional field converted to a ref type, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToRefField())

    [<Fact>]
    member x. ``Value optional field converted to a ref type, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Binary)
                      String "2"
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToRefField(Field = nullable 2))

    [<Fact>]
    member x. ``Value optional field converted to a ref type with default, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToRefFieldWithDefault(Field = 1))

    [<Fact>]
    member x. ``Value optional field converted to a ref type with default, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Binary)
                      String "2"
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_ValueOptionalConvertedToRefFieldWithDefault(Field = 2))

    [<Fact>]
    member x. ``Ref optional field with default, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalFieldWithDefault(Field = "1"))

    [<Fact>]
    member x. ``Ref optional field with default, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Binary)
                      String "2"
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalFieldWithDefault(Field = "2"))

    [<Fact>]
    member x. ``Ref required field converted to a value type``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Int32)
                      Int32 2
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_RefRequiredConvertedToValueField(Field = "2"))

    [<Fact>]
    member x. ``Ref optional field converted to a value type, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToValueField())

    [<Fact>]
    member x. ``Ref optional field converted to a value type, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Int32)
                      Int32 2
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToValueField(Field = "2"))

    [<Fact>]
    member x. ``Ref optional field converted to a value type with default, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToValueFieldWithDefault(Field = "1"))

    [<Fact>]
    member x. ``Ref optional field converted to a value type with default, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Int32)
                      Int32 2
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToValueFieldWithDefault(Field = "2"))

    [<Fact>]
    member x. ``Ref required field converted to a ref type``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Binary)
                      String "2"
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_RefRequiredConvertedToRefField(Field = "02"))

    [<Fact>]
    member x. ``Ref optional field converted to a ref type, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToRefField())

    [<Fact>]
    member x. ``Ref optional field converted to a ref type, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Binary)
                      String "2"
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToRefField(Field = "02"))

    [<Fact>]
    member x. ``Ref optional field converted to a ref type with default, not set``() =
        x.TestStruct [StructHeader "S"
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToRefFieldWithDefault(Field = "01"))

    [<Fact>]
    member x. ``Ref optional field converted to a ref type with default, set``() =
        x.TestStruct [StructHeader "S"
                      FieldHeader (1s, "F", ThriftTypeId.Binary)
                      String "2"
                      FieldEnd
                      FieldStop
                      StructEnd]
                     (Struct_RefOptionalConvertedToRefFieldWithDefault(Field = "02"))

type Reading() =
    inherit Tests()

    member x.Throws<'e> fieldData expected =
        let typ = makeClass [{typ = typeof<ThriftStructAttribute>; args = ["Struct"]; namedArgs = []}]
                            [typeof<'e>, [{typ = typeof<ThriftFieldAttribute>; args = [1s; true; "Field"]; namedArgs = []}]]

        let data = [StructHeader "Struct"
                    FieldHeader (1s, "Field", ThriftType.Get(typeof<'e>).Id)] 
                 @  fieldData 
                 @ [FieldEnd
                    FieldStop
                    StructEnd] 
        let m = MemoryProtocol(data)
        let thriftStruct = ThriftAttributesParser.ParseStruct(typ.GetTypeInfo())
        let exn = Assert.Throws<ThriftSerializationException>(fun () -> ThriftStructReader.Read(thriftStruct, m) |> box)
        Assert.Contains(expected, exn.Message)


    override x.Test fieldData typeId (value: 'a) =
        let isReq = typeof<'a>.IsValueType && System.Nullable.GetUnderlyingType(typeof<'a>) = null
        let typ = makeClass [{typ = typeof<ThriftStructAttribute>; args = ["Struct"]; namedArgs = []}]
                            [typeof<'a>, [{typ = typeof<ThriftFieldAttribute>; args = [1s; isReq; "Field"]; namedArgs = []}]]
        
        let data = 
            [StructHeader "Struct"
             FieldHeader (1s, "Field", typeId)]
          @ fieldData 
          @ [FieldEnd; FieldStop; StructEnd]
        
        let m = MemoryProtocol(data)
        let thriftStruct = ThriftAttributesParser.ParseStruct(typ.GetTypeInfo())
        let structInst = ThriftStructReader.Read(thriftStruct, m)
        m.IsEmpty <=> true
        (typ.GetProperty("0").GetValue(structInst) :?> 'a) <=> value

    override x.TestStruct data (value: 'a) =
        let m = MemoryProtocol(data)
        let thriftStruct = ThriftAttributesParser.ParseStruct(typeof<'a>.GetTypeInfo())
        let inst = ThriftStructReader.Read<'a>(thriftStruct, m)
        m.IsEmpty <=> true
        inst <=> value


    [<Fact>]
    member x.``Error on missing required field``() =
        let data = 
            [StructHeader "StructField"
             FieldStop
             StructEnd]
        let m = MemoryProtocol(data)
        let thriftStruct = ThriftAttributesParser.ParseStruct(typeof<StructWithStructField>.GetTypeInfo())
        let exn = Assert.Throws<ThriftSerializationException>(fun () -> ThriftStructReader.Read(thriftStruct, m) |> box)
        Assert.Contains("Field 'Field' is a required field, but was not present", exn.Message)

    [<Fact>]
    member x.``Error when the field type doesn't match its declaration``() =
        x.Throws<SimpleStruct> [StructHeader ("SimpleStruct")
                                FieldHeader (1s, "Field", ThriftTypeId.Int64)
                                Int64 0L
                                FieldEnd
                                FieldStop
                                StructEnd]
                               "Expected type Int32, but type Int64 was read"

    [<Fact>]
    member x.``Error when the list element type doesn't match its declaration``() =
        x.Throws<List<int>> [ListHeader (1, ThriftTypeId.Boolean)
                             Bool true
                             ListEnd]
                            "Expected type Int32, but type Boolean was read"

    [<Fact>]
    member x.``Error when the set element type doesn't match its declaration``() =
        x.Throws<HashSet<int>> [SetHeader (3, ThriftTypeId.Boolean)
                                Bool true
                                SetEnd]
                               "Expected type Int32, but type Boolean was read"

    [<Fact>]
    member x.``Error when the map key type doesn't match its declaration``() =
        x.Throws<Dictionary<int, string>> [MapHeader (1, ThriftTypeId.Boolean, ThriftTypeId.Binary)
                                           Bool true
                                           String "x"
                                           MapEnd]
                                          "Expected type Int32, but type Boolean was read"

    [<Fact>]
    member x.``Error when the map value type doesn't match its declaration``() =
        x.Throws<Dictionary<int, string>> [MapHeader (1, ThriftTypeId.Int32, ThriftTypeId.Boolean)
                                           Int32 1
                                           Bool true
                                           MapEnd]
                                          "Expected type Binary, but type Boolean was read"

    [<Fact>]
    member x.``Error when the array value type doesn't match its declaration``() =
        x.Throws<int[]> [ListHeader (1, ThriftTypeId.Boolean)
                         Bool true
                         ListEnd]
                        "Expected type Int32, but type Boolean was read"


type Writing() =
    inherit Tests()

    let write prot obj =
        let thriftStruct = ThriftAttributesParser.ParseStruct(obj.GetType().GetTypeInfo())
        let meth = typeof<ThriftStructWriter>.GetMethod("Write").MakeGenericMethod([| obj.GetType() |])
        try
            meth.Invoke(null, [| thriftStruct; obj; prot |]) |> ignore
        with
        | :? TargetInvocationException as e -> raise e.InnerException

    override x.Test fieldData typeId (value: 'a) =
        let isReq = typeof<'a>.IsValueType && System.Nullable.GetUnderlyingType(typeof<'a>) = null
        let typ = makeClass [{typ = typeof<ThriftStructAttribute>; args = ["Struct"]; namedArgs = []}]
                            [typeof<'a>, [{typ = typeof<ThriftFieldAttribute>; args = [1s; isReq; "Field"]; namedArgs = []}]]

        let inst = System.Activator.CreateInstance(typ)
        typ.GetProperty("0").SetValue(inst, value)
        
        let m = MemoryProtocol()
        write m inst

        m.WrittenValues <=> [StructHeader "Struct"
                             FieldHeader (1s, "Field", typeId)]
                          @  fieldData 
                          @ [FieldEnd
                             FieldStop
                             StructEnd]

    override x.TestStruct data (value: 'a) =
        let m = MemoryProtocol()
        write m value
        m.WrittenValues <=> data


    [<Fact>]
    member __.``Error on required but unset field``() =
        let exn = Assert.Throws<ThriftSerializationException>(fun () -> write (MemoryProtocol()) (StructWithStructField()))
        Assert.Contains("Field 'Field' is a required field but was null", exn.Message)


type Skipping() =
    inherit Tests()
    
    override x.Test fieldData typeId (value: 'a) =
        let data = 
            [StructHeader "Struct"; 
             FieldHeader (1s, "Field", typeId)] 
          @ fieldData 
          @ [FieldEnd; FieldStop; StructEnd]

        let m = MemoryProtocol(data)
        let thriftStruct = ThriftAttributesParser.ParseStruct(typeof<StructWithoutFields>.GetTypeInfo())
        ThriftStructReader.Read<StructWithoutFields>(thriftStruct, m) |> ignore
        m.IsEmpty <=> true

    override x.TestStruct data (value: 'a) =
        () // not applicable