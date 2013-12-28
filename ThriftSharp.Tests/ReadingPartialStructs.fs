// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals

[<ThriftStruct("OptionalFields")>]
type StructWithOptionalFields3() =
    [<ThriftField(1s, true, "Required")>]
    member val Required = 1 with get, set
    [<ThriftField(2s, false, "Optional")>]
    member val Optional = 2 with get, set

[<ThriftStruct("WithDefaultValue")>]
type StructWithDefaultValue3() =
    [<ThriftField(1s, false, "Field")>]
    [<ThriftDefaultValue(456)>]
    member val Field = 123 with get, set

[<TestClass>]
type ``Reading partial structs``() =
    member x.ReadStruct<'T>(prot) = ThriftSerializer.FromType(typeof<'T>).Read(prot, typeof<'T>) :?> 'T

    [<Test>]
    member x.``Missing optional field``() =
        let m = MemoryProtocol([StructHeader "OptionalFields"
                                FieldHeader (1s, "Required", tid 8uy)
                                Int32 10
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithOptionalFields3>(m)
        inst.Required <=> 10
        inst.Optional <=> 2

    [<Test>]
    member x.``Missing required field``() =
        let m = MemoryProtocol([StructHeader "OptionalFields"
                                FieldHeader (2s, "Optional", tid 8uy)
                                Int32 10
                                FieldEnd
                                FieldStop
                                StructEnd])
        throws<ThriftProtocolException> (fun () -> box (x.ReadStruct<StructWithOptionalFields3>(m))) |> ignore

    [<Test>]
    member x.``Present optional w/ default value field``() =
        let m = MemoryProtocol([StructHeader "WithDefaultValue"
                                FieldHeader (1s, "Field", tid 8uy)
                                Int32 789
                                FieldEnd
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithDefaultValue3>(m)
        inst.Field <=> 789

    [<Test>]
    member x.``Missing optional w/ default value field``() =
        let m = MemoryProtocol([StructHeader "WithDefaultValue"
                                FieldStop
                                StructEnd])
        let inst = x.ReadStruct<StructWithDefaultValue3>(m)
        inst.Field <=> 456