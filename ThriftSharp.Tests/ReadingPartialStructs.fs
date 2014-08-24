// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Reading partial structs``

open ThriftSharp

[<ThriftStruct("OptionalFields")>]
type StructWithOptionalFields() =
    [<ThriftField(1s, true, "Required")>]
    member val Required = 1 with get, set
    [<ThriftField(2s, false, "Optional")>]
    member val Optional = nullable 2 with get, set

[<ThriftStruct("WithDefaultValue")>]
type StructWithDefaultValue() =
    [<ThriftField(1s, false, "Field")>]
    [<ThriftDefaultValue(456)>]
    member val Field = nullable 123 with get, set


let (==>) data (expected: 'a) =
    let m = MemoryProtocol(data)
    let inst = read<'a> m
    m.IsEmpty <=> true
    inst <=> expected

let fails<'S> data =
    let m = MemoryProtocol(data)
    throws<ThriftSerializationException> (fun () -> read<'S> m |> box) |> ignore


[<TestContainer>]
type __() =
    [<Test>]
    member __.``Missing optional field``() = 
        [StructHeader "OptionalFields"
         FieldHeader (1s, "Required", tid 8)
         Int32 10
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        StructWithOptionalFields(Required = 10, Optional = nullable 2)

    [<Test>]
    member __.``Error on missing required field``() =
        fails<StructWithOptionalFields>
            [StructHeader "OptionalFields"
             FieldHeader (2s, "Optional", tid 8)
             Int32 10
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``Present optional w/ default value field``() =
        [StructHeader "WithDefaultValue"
         FieldHeader (1s, "Field", tid 8)
         Int32 789
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        StructWithDefaultValue(Field = nullable 789)

    [<Test>]
    member __.``Missing optional w/ default value field``() =
        [StructHeader "WithDefaultValue"
         FieldStop
         StructEnd]
        ==>
        StructWithDefaultValue(Field = nullable 456)