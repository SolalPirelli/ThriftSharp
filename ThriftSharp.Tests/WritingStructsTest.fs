namespace ThriftSharp.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp

[<ThriftStruct("NoFields")>]
type StructWithoutFields4() = class end

[<ThriftStruct("OnePrimitiveField")>]
type StructWithOnePrimitiveField4() =
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
    member x.``One primitive field``() =
        let m = MemoryProtocol()
        writeSt m (StructWithOnePrimitiveField4())
        m.WrittenValues <=> [StructHeader "OnePrimitiveField"
                             FieldHeader (1s, "Field", ThriftType.FromType(typeof<int>))
                             Int32 42
                             FieldEnd
                             FieldStop
                             StructEnd]

    [<Test>]
    member x.``Many primitive fields``() =
        let m = MemoryProtocol()
        writeSt m (StructWithManyPrimitiveFields4())
        m.WrittenValues <=> [StructHeader "ManyPrimitiveFields"
                             FieldHeader (1s, "BoolField", ttype typeof<bool>)
                             Bool true
                             FieldEnd
                             FieldHeader (2s, "SByteField", ttype typeof<sbyte>)
                             SByte 1y
                             FieldEnd
                             FieldHeader (3s, "DoubleField", ttype typeof<float>)
                             Double 1.0
                             FieldEnd
                             FieldHeader (4s, "Int16Field", ttype typeof<int16>)
                             Int16 1s
                             FieldEnd
                             FieldHeader (5s, "Int32Field", ttype typeof<int>)
                             Int32 1
                             FieldEnd
                             FieldHeader (6s, "Int64Field", ttype typeof<int64>)
                             Int64 1L
                             FieldEnd
                             FieldHeader (7s, "StringField", ttype typeof<string>)
                             String "abc"
                             FieldEnd
                             FieldHeader (8s, "BinaryField", ttype typeof<sbyte[]>)
                             Binary [| 1y |]
                             FieldEnd
                             FieldStop
                             StructEnd]