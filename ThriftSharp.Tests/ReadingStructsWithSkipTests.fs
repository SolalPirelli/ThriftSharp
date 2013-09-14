// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

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
    member x.ReadStruct<'T>(prot) = ThriftSerializer.Struct.Read(prot, typeof<'T>) |> ignore

    [<Test>]
    member x.``One field for a struct without fields``() =
        let m = MemoryProtocol([StructHeader "NoFields"
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 42
                                FieldEnd
                                FieldStop
                                StructEnd])
        x.ReadStruct<StructWithoutFields2>(m)
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
        x.ReadStruct<StructWithOneField2>(m)
        m.IsEmpty <=> true

    [<Test>]
    member x.``Skipping primitive fields``() =
        let m = MemoryProtocol([StructHeader "NoFields"
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
                                FieldStop
                                StructEnd])
        x.ReadStruct<StructWithoutFields2>(m)
        m.IsEmpty <=> true

    [<Test>]
    member x.``Skipping collection fields``() =
        let m = MemoryProtocol([StructHeader "CollectionFields"
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
                                StructEnd])
        x.ReadStruct<StructWithoutFields2>(m)
        m.IsEmpty <=> true

    [<Test>]
    member x.``Skipping struct field``() =
        let m = MemoryProtocol([StructHeader "StructField"
                                FieldHeader (1s, "StructField", tid 12uy)
                                StructHeader "OneField"
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 23
                                FieldEnd
                                FieldStop
                                StructEnd
                                FieldEnd
                                FieldStop
                                StructEnd])
        x.ReadStruct<StructWithoutFields2>(m)
        m.IsEmpty <=> true