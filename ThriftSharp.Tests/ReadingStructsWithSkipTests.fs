namespace ThriftSharp.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals

[<ThriftStruct("NoFields")>]
type StructWithoutFields2() = class end

[<ThriftStruct("OneField")>]
type StructWithOneField2() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 42 with get, set


[<TestClass>]
type ``Reading structs skipping fields``() =
    let readSt prot typ = ThriftSerializer.Struct.Read(prot, typ) |> ignore

    [<Test>]
    member x.``One field for a struct without fields``() =
        let m = MemoryProtocol([StructHeader "NoFields"
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 42
                                FieldEnd
                                FieldStop
                                StructEnd])
        readSt m typeof<StructWithoutFields2>
        m.IsEmpty <=> true


    [<Test>]
    member x.``Many fields for a struct with one primitive fields``() =
        let m = MemoryProtocol([StructHeader "OneField"
                                FieldHeader (3s, "Field3", tid 11uy)
                                String "abc"
                                FieldEnd
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 42
                                FieldEnd
                                FieldHeader(2s, "Field2", tid 8uy)
                                Int32 34
                                FieldEnd
                                FieldStop
                                StructEnd])
        readSt m typeof<StructWithOneField2>
        m.IsEmpty <=> true