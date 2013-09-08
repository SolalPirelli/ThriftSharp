namespace ThriftSharp.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp

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

[<TestClass>]
type ``Reading structs``() =
    member x.ReadStruct<'T>(prot) = ThriftType.Struct.Read(prot, typeof<'T>) :?> 'T

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
                                FieldHeader (1s, "Field", ThriftType.FromType(typeof<int>))
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
                                FieldHeader (1s, "BoolField", ttype typeof<bool>)
                                Bool false
                                FieldEnd
                                FieldHeader (2s, "SByteField", ttype typeof<sbyte>)
                                SByte -1y
                                FieldEnd
                                FieldHeader (3s, "DoubleField", ttype typeof<float>)
                                Double -1.0
                                FieldEnd
                                FieldHeader (4s, "Int16Field", ttype typeof<int16>)
                                Int16 -1s
                                FieldEnd
                                FieldHeader (5s, "Int32Field", ttype typeof<int>)
                                Int32 -1
                                FieldEnd
                                FieldHeader (6s, "Int64Field", ttype typeof<int64>)
                                Int64 -1L
                                FieldEnd
                                FieldHeader (7s, "StringField", ttype typeof<string>)
                                String "xyzzy"
                                FieldEnd
                                FieldHeader (8s, "BinaryField", ttype typeof<sbyte[]>)
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
                                FieldHeader (1s, "ListField", ttype typeof<List<int>>)
                                ListHeader (1, ttype typeof<int>)
                                Int32 1
                                ListEnd
                                FieldEnd
                                FieldHeader (2s, "SetField", ttype typeof<HashSet<int>>)
                                SetHeader (3, ttype typeof<int>)
                                Int32 2
                                Int32 3
                                Int32 4
                                SetEnd
                                FieldEnd
                                FieldHeader (3s, "MapField", ttype typeof<Dictionary<int, string>>)
                                MapHeader (2, ttype typeof<int>, ttype typeof<string>)
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
                                FieldHeader (1s, "StructField", ttype typeof<StructWithOneField1>)
                                StructHeader "OneField"
                                FieldHeader (1s, "Field", ttype typeof<int>)
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
                                FieldHeader (2s, "Field2", ttype typeof<Enum1>)
                                Int32 2
                                FieldEnd
                                FieldHeader (1s, "Field1", ttype typeof<Enum1>)
                                Int32 3
                                FieldEnd
                                FieldHeader (3s, "Field3", ttype typeof<Enum1>)
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
                                FieldHeader (1s, "Field1", ttype typeof<int[]>)
                                ListHeader (2, ttype typeof<int>)
                                Int32 23
                                Int32 42
                                ListEnd
                                FieldEnd
                                FieldHeader (2s, "Field2", ttype typeof<int[]>)
                                ListHeader (0, ttype typeof<int>)
                                ListEnd
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithArrayFields1>(m)
        inst.Field1 <===> [23; 42]
        inst.Field2 <===> []