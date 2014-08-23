// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Writing structs``

open System.Collections.Generic
open ThriftSharp

[<ThriftEnum>]
type SimpleEnum =
    | A = 1
    | B = 2

[<ThriftStruct("SimpleStruct"); AllowNullLiteral>]
type SimpleStruct() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 1 with get, set

[<ThriftStruct("StructField")>]
type StructWithStructField() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = null :> SimpleStruct with get, set

[<ThriftStruct("ConvertingField")>]
type StructWithConvertingField() =
    [<ThriftField(1s, true, "UnixDate")>]
    [<ThriftConverter(typeof<ThriftUnixDateConverter>)>]
    member val UnixDate = System.DateTime.Now with get, set

[<ThriftStruct("NullableField")>]
type StructWithNullableField() =
    [<ThriftField(1s, false, "Field")>]
    member val Field = System.Nullable() with get, set

let ok (value: 'a) typeId fieldData =
    let typ = 
        makeClass 
            [ <@ ThriftStructAttribute("Struct") @> ] 
            [ "Field", typeof<'a>, [ <@ ThriftFieldAttribute(1s, true, "Field") @> ] ]
    let inst = System.Activator.CreateInstance(typ)
    typ.GetProperty("Field").SetValue(inst, value)

    let data = 
        [StructHeader "Struct"; FieldHeader (1s, "Field", tid typeId)]
      @ fieldData 
      @ [FieldEnd; FieldStop; StructEnd]

    let m = MemoryProtocol()
    write m inst
    m.WrittenValues <=> data

let (==>) obj data =
    let m = MemoryProtocol()
    write m obj
    m.WrittenValues <=> data

let fails obj =
    throwsAsync<ThriftSerializationException>(fun () -> async { write (MemoryProtocol()) obj; return System.Object() }) |> run


[<TestContainer>]
type __() =
    // Primitive fields
    [<Test>] member __.Bool()   = ok true          2 [Bool true]
    [<Test>] member __.SByte()  = ok 1y            3 [SByte 1y]
    [<Test>] member __.Double() = ok 1.0           4 [Double 1.0]
    [<Test>] member __.Int16()  = ok 1s            6 [Int16 1s]
    [<Test>] member __.Int32()  = ok 1             8 [Int32 1]
    [<Test>] member __.Int64()  = ok 1L           10 [Int64 1L]
    [<Test>] member __.String() = ok "x"          11 [String "x"]
    [<Test>] member __.Binary() = ok [| 1y |]     11 [Binary [| 1y |]]
    [<Test>] member __.Enum()   = ok SimpleEnum.A  8 [Int32 1]

    // Collection fields
    [<Test>] member __.List()  = ok (List [1])     15 [ListHeader (1, tid 8); Int32 1; ListEnd]
    [<Test>] member __.Set()   = ok (HashSet [1])  14 [SetHeader (1, tid 8); Int32 1; SetEnd]
    [<Test>] member __.Map()   = ok (dict [1, 1L]) 13 [MapHeader (1, tid 8, tid 10); Int32 1; Int64 1L; MapEnd]
    [<Test>] member __.Array() = ok [| 1 |]        15 [ListHeader (1, tid 8); Int32 1; ListEnd]

    // Struct fields
    [<Test>] member __.Struct() = ok (SimpleStruct()) 12 [StructHeader "SimpleStruct"; FieldHeader (1s, "Field", tid 8); Int32 1; FieldEnd; FieldStop; StructEnd]
     

    // Other, more specialized, tests
    [<Test>]
    member __.``Error on required but unset struct field``() =
        fails (StructWithStructField())

    [<Test>]
    member __.``UnixDate converter``() =
        StructWithConvertingField( UnixDate = date(18, 12, 1994) )
        ==>
        [StructHeader "ConvertingField"
         FieldHeader (1s, "UnixDate", tid 8)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd]

    [<Test>]
    member __.``Nullable field, not set``() =
        StructWithNullableField()
        ==>
        [StructHeader "NullableField"
         FieldStop
         StructEnd]                             

    [<Test>]
    member __.``Nullable field, set``() =
        StructWithNullableField( Field = nullable 112233 )
        ==>
        [StructHeader "NullableField"
         FieldHeader (1s, "Field", tid 8)
         Int32 112233
         FieldEnd
         FieldStop
         StructEnd]