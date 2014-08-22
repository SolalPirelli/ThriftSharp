// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Reading structs skipping fields``

open ThriftSharp

[<ThriftStruct("Struct")>]
type StructWithOneField() =
    [<ThriftField(0s, false, "Field")>]
    member val Field = nullable 42 with get, set

let (--) thriftType fieldData =
    let data = [StructHeader "Struct"; FieldHeader (1s, "Field", tid thriftType)] @ fieldData @ [FieldEnd; FieldStop; StructEnd]
    let m = MemoryProtocol(data)
    read<StructWithOneField> m |> ignore
    m.IsEmpty <=> true

[<TestContainer>]
type __() =
    // Primitive fields
    [<Test>] member __.``Bool``()   =  2 -- [Bool true]
    [<Test>] member __.``SByte``()  =  3 -- [SByte 1y]
    [<Test>] member __.``Double``() =  4 -- [Double 1.0] 
    [<Test>] member __.``Int16``()  =  6 -- [Int16 1s]
    [<Test>] member __.``Int32``()  =  8 -- [Int32 1]
    [<Test>] member __.``Int64``()  = 10 -- [Int64 1L]
    [<Test>] member __.``Binary``() = 11 -- [Binary [| 1y |]]

    // Collection fields
    [<Test>] member __.``List``() = 15 -- [ListHeader(1, tid 8); Int32 1; ListEnd]
    [<Test>] member __.``Set``()  = 14 -- [SetHeader(1, tid 8); Int32 1; SetEnd]
    [<Test>] member __.``Map``()  = 13 -- [MapHeader(1, tid 8, tid 10); Int32 1; Int64 1L; MapEnd]

    // Struct fields
    [<Test>] member __.``Struct``() = 12 -- [StructHeader "S"; FieldStop; StructEnd]