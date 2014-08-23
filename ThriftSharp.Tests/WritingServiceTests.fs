// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Writing service queries``

open System.Threading.Tasks
open ThriftSharp
open ThriftSharp.Internals

[<ThriftService("Service")>]
type Service =
    [<ThriftMethod("noParameters")>]
    abstract NoParameters: unit -> Task

    [<ThriftMethod("oneParameter")>]
    abstract OneParameter: [<ThriftParameter(1s, "p1")>] p1: int -> Task

    [<ThriftMethod("manyParameters")>]
    abstract ManyParameters: [<ThriftParameter(1s, "p1")>] p1: int 
                           * [<ThriftParameter(2s, "p2")>] p2: string 
                           * [<ThriftParameter(10s, "p10")>] p10: string -> Task

    [<ThriftMethod("oneWay", true)>]
    abstract OneWay: unit -> Task

    [<ThriftMethod("withUnixDateParam")>]
    abstract WithUnixDateParam: 
        [<ThriftParameter(1s, "date"); ThriftConverter(typeof<ThriftUnixDateConverter>)>] date: System.DateTime -> Task


let (--) a b = a,b

let (==>) (methodName, args) data = run <| async {
    let! m = writeMsgAsync<Service> methodName (Array.ofSeq args)
    m.WrittenValues <=> data
}

let fails<'E when 'E :> exn> methodName args =
    throwsAsync<'E> (async { 
        let! res = writeMsgAsync<Service> methodName (Array.ofSeq args)
        return box res
    }) |> run


[<TestContainer>]
type __() =
    [<Test>]
    member __.``No parameters``() =
        "NoParameters" 
        -- 
        []
        ==>
        [MessageHeader (0, "noParameters", ThriftMessageType.Call)
         StructHeader ""
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``One parameter``() =
        "OneParameter"
        --
        [123]
        ==>
        [MessageHeader (0, "oneParameter", ThriftMessageType.Call)
         StructHeader ""
         FieldHeader (1s, "p1", ThriftTypeId.Int32)
         Int32 123
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``Many parameters``() =
        "ManyParameters"
        -- 
        [16; "Sayid"; "Jarrah"]
        ==>
        [MessageHeader (0, "manyParameters", ThriftMessageType.Call)
         StructHeader ""
         FieldHeader (1s, "p1", ThriftTypeId.Int32)
         Int32 16
         FieldEnd
         FieldHeader (2s, "p2", ThriftTypeId.Binary)
         String "Sayid"
         FieldEnd
         FieldHeader (10s, "p10", ThriftTypeId.Binary)
         String "Jarrah"
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``One-way method``() =
        "OneWay" 
        --
        []
        ==>
        [MessageHeader (0, "oneWay", ThriftMessageType.OneWay)
         StructHeader ""
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``UnixDate parameter``() =
        "WithUnixDateParam" 
        --
        [date(18, 12, 1994)]
        ==>
        [MessageHeader (0, "withUnixDateParam", ThriftMessageType.Call)
         StructHeader ""
         FieldHeader (1s, "date", ThriftTypeId.Int32)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``ArgumentException is thrown when there are too few params``() =
        fails<System.ArgumentException> "OneParameter" []
        
    [<Test>]
    member __.``ArgumentException is thrown when there are too many params``() =
        fails<System.ArgumentException> "OneParameter" [1; 2]