﻿// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Reading structs skipping fields``

open ThriftSharp

[<ThriftStruct("OneField")>]
type StructWithOneField() =
    [<ThriftField(1000s, false, "Field")>]
    member val Field = nullable 42 with get, set


let check<'T> data =
    let m = MemoryProtocol(data)
    read<'T> m |> ignore
    m.IsEmpty <=> true


[<TestContainer>]
type __() =
    [<Test>]
    member __.``Many fields for a struct with one primitive fields``() =
        check<StructWithOneField>
            [StructHeader "OneField"
             FieldHeader (3s, "Field3", tid 2)
             Bool true
             FieldEnd
             FieldHeader (1000s, "Field", tid 8)
             Int32 42
             FieldEnd
             FieldHeader(2s, "Field2", tid 8)
             Int32 34
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``Skipping primitive fields``() =
        check<StructWithOneField>
            [StructHeader "NoFields"
             FieldHeader (1s, "BoolField", tid 2)
             Bool false
             FieldEnd
             FieldHeader (2s, "SByteField", tid 3)
             SByte -1y
             FieldEnd
             FieldHeader (3s, "DoubleField", tid 4)
             Double -1.0
             FieldEnd
             FieldHeader (4s, "Int16Field", tid 6)
             Int16 -1s
             FieldEnd
             FieldHeader (5s, "Int32Field", tid 8)
             Int32 -1
             FieldEnd
             FieldHeader (6s, "Int64Field", tid 10)
             Int64 -1L
             FieldEnd
             FieldHeader (7s, "BinaryField", tid 11)
             Binary [| 1y; 2y; 42y; |]
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``Skipping collection fields``() =
        check<StructWithOneField>
            [StructHeader "CollectionFields"
             FieldHeader (1s, "ListField", tid 15)
             ListHeader (1, tid 8)
             Int32 1
             ListEnd
             FieldEnd
             FieldHeader (2s, "SetField", tid 14)
             SetHeader (3, tid 8)
             Int32 2
             Int32 3
             Int32 4
             SetEnd
             FieldEnd
             FieldHeader (3s, "MapField", tid 13)
             MapHeader (2, tid 8, tid 10)
             Int32 5
             Int64 555L
             Int32 6
             Int64 666L
             MapEnd
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``Skipping struct field``() =
        check<StructWithOneField>
            [StructHeader "StructField"
             FieldHeader (1s, "StructField", tid 12)
             StructHeader "OneField"
             FieldHeader (1s, "Field", tid 8)
             Int32 23
             FieldEnd
             FieldStop
             StructEnd
             FieldEnd
             FieldStop
             StructEnd]