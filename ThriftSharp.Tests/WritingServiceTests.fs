// Copyright (c) 2013 Solal Pirelli
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

let (==>) (methodName, args) data =
    let m = writeMsg<Service> methodName (Array.ofSeq args)
    m.WrittenValues <===> data

let throwsOnWrite<'T when 'T :> System.Exception> methodName args =
    throws<'T> (fun () -> writeMsg<Service> methodName (Array.ofSeq args) |> ignore) |> ignore


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
         FieldHeader (1s, "p1", ThriftType.Int32)
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
         FieldHeader (1s, "p1", ThriftType.Int32)
         Int32 16
         FieldEnd
         FieldHeader (2s, "p2", ThriftType.String)
         String "Sayid"
         FieldEnd
         FieldHeader (10s, "p10", ThriftType.String)
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
        [utcDate(18, 12, 1994)]
        ==>
        [MessageHeader (0, "withUnixDateParam", ThriftMessageType.Call)
         StructHeader ""
         FieldHeader (1s, "date", ThriftType.Int32)
         Int32 787708800
         FieldEnd
         FieldStop
         StructEnd
         MessageEnd]

    [<Test>]
    member __.``ArgumentException is thrown when there are too few params``() =
        throwsOnWrite<System.ArgumentException> "OneParameter" []
        
    [<Test>]
    member __.``ArgumentException is thrown when there are too many params``() =
        throwsOnWrite<System.ArgumentException> "OneParameter" [1; 2]