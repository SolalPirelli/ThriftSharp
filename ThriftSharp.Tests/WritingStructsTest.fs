namespace ThriftSharp.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp

[<ThriftStruct("NoFields")>]
type StructWithoutFields4() = class end

[<ThriftStruct("OneField")>]
type StructWithOneField4() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 42 with get, set

[<ThriftStruct("ManyPrimitiveFields")>]
type StructWithManyPrimitiveFields4() =
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
type StructWithCollectionFields4() =
    [<ThriftField(1s, true, "ListField")>]
    member val List = List<int>() with get, set
    [<ThriftField(2s, true, "SetField")>]
    member val Set = HashSet<int>() with get, set
    [<ThriftField(3s, true, "MapField")>]
    member val Map = Dictionary<int, string>() with get, set

[<ThriftStruct("StructField")>]
type StructWithStructField4() =
    [<ThriftField(1s, true, "StructField")>]
    member val Struct = StructWithOneField4() with get, set

[<ThriftEnum("Enum")>]
type Enum4 =
    | A = 1
    | [<ThriftEnumMember("B", 3)>] B = 2

[<ThriftStruct("EnumFields")>]
type StructWithEnumFields4() =
    [<ThriftField(1s, true, "Field1")>]
    member val Field1 = Enum4.A with get, set
    [<ThriftField(2s, true, "Field2")>]
    member val Field2 = Enum4.A with get, set

[<ThriftStruct("ArrayFields")>]
type StructWithArrayFields4() =
    [<ThriftField(1s, true, "Field1")>]
    member val Field1 = [| 1 |] with get, set
    [<ThriftField(2s, true, "Field2")>]
    member val Field2 = [| 2 |] with get, set

[<TestClass>]
type ``Writing structs``() =
    let writeSt prot obj = ThriftType.Struct.Write(prot, obj)
    

    [<Test>]
    member x.``No fields``() =
        let m = MemoryProtocol()
        writeSt m (StructWithoutFields4())
        m.WrittenValues <=> [StructHeader "NoFields"
                             FieldStop
                             StructEnd]
        
    [<Test>]
    member x.``One field``() =
        let m = MemoryProtocol()
        writeSt m (StructWithOneField4())
        m.WrittenValues <=> [StructHeader "OneField"
                             FieldHeader (1s, "Field", ThriftType.FromType(typeof<int>))
                             Int32 42
                             FieldEnd
                             FieldStop
                             StructEnd]

    [<Test>]
    member x.``Primitive fields``() =
        let m = MemoryProtocol()
        writeSt m (StructWithManyPrimitiveFields4())
        m.WrittenValues <=> [StructHeader "ManyPrimitiveFields"
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
    member x.``Collection fields``() =
        let m = MemoryProtocol()
        let inst = StructWithCollectionFields4()
        inst.List.AddRange([4; 8; 15])
        inst.Set.Add(16) |> ignore
        inst.Set.Add(23) |> ignore
        inst.Map.Add(42, "Lost")

        writeSt m inst

        m.WrittenValues <===> [StructHeader "CollectionFields"
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
    member x.``Struct field``() =
        let m = MemoryProtocol()
        let inst = StructWithStructField4()
        inst.Struct.Field <- 777

        writeSt m inst

        m.WrittenValues <===> [StructHeader "StructField"
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
    member x.``Enum fields``() =
        let m = MemoryProtocol()
        let inst = StructWithEnumFields4()
        inst.Field1 <- Enum4.A
        inst.Field2 <- Enum4.B

        writeSt m inst

        m.WrittenValues <===> [StructHeader "EnumFields"
                               FieldHeader (1s, "Field1", tid 8uy)
                               Int32 1
                               FieldEnd
                               FieldHeader (2s, "Field2", tid 8uy)
                               Int32 3
                               FieldEnd
                               FieldStop
                               StructEnd]

    [<Test>]
    member x.``Array fields``() =
        let m = MemoryProtocol()
        let inst = StructWithArrayFields4()
        inst.Field1 <- [| 12345; 67890 |]
        inst.Field2 <- [| |]

        writeSt m inst

        m.WrittenValues <===> [StructHeader "ArrayFields"
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