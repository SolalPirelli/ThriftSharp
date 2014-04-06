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
    member val Optional = 2 with get, set

[<ThriftStruct("WithDefaultValue")>]
type StructWithDefaultValue() =
    [<ThriftField(1s, false, "Field")>]
    [<ThriftDefaultValue(456)>]
    member val Field = 123 with get, set


let (==>) data (checker: 'a -> unit) = run <| async {
    let m = MemoryProtocol(data)
    let! inst = readAsync<'a> m
    m.IsEmpty <=> true
    do checker inst
}

let throwsOnRead<'S, 'T when 'T :> exn> data = run <| async {
    let m = MemoryProtocol(data)
    do! throwsAsync<'T> (fun () -> 
        async { 
            let! res = readAsync<'S> m
            return box res 
        }) |> Async.Ignore
}


[<TestContainer>]
type __() =
    [<Test>]
    member __.``Missing optional field``() = 
        [StructHeader "OptionalFields"
         FieldHeader (1s, "Required", tid 8uy)
         Int32 10
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithOptionalFields) ->
            inst.Required <=> 10
            inst.Optional <=> 2

    [<Test>]
    member __.``Missing required field``() =
        throwsOnRead<StructWithOptionalFields, ThriftSerializationException>
            [StructHeader "OptionalFields"
             FieldHeader (2s, "Optional", tid 8uy)
             Int32 10
             FieldEnd
             FieldStop
             StructEnd]

    [<Test>]
    member __.``Present optional w/ default value field``() =
        [StructHeader "WithDefaultValue"
         FieldHeader (1s, "Field", tid 8uy)
         Int32 789
         FieldEnd
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithDefaultValue) ->
            inst.Field <=> 789

    [<Test>]
    member __.``Missing optional w/ default value field``() =
        [StructHeader "WithDefaultValue"
         FieldStop
         StructEnd]
        ==>
        fun (inst: StructWithDefaultValue) ->
            inst.Field <=> 456