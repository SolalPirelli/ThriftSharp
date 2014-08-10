// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Binary protocol reading``

open ThriftSharp
open ThriftSharp.Internals
open ThriftSharp.Protocols

let (--) a b = a,b

let (==>) (data, selector) check =
    let trans = MemoryTransport(data |> List.map byte)
    let obj = new ThriftBinaryProtocol(trans) |> selector
    check(obj)

let (<==>) (data, selector) value =
    (data, selector) ==> (fun o -> o <=> value)

let (=//=>) (data, selector) (check: 'a -> unit) =
    let trans = MemoryTransport(data |> List.map byte)
    let ex = throws<'a> (fun () -> new ThriftBinaryProtocol(trans) |> selector |> box)
    check(ex)

let needsNothing selector =
    let trans = MemoryTransport([])
    selector(new ThriftBinaryProtocol(trans))

[<TestContainer>]
type __()  =
    [<Test>]
    member __.``MessageHeader - reply``() =
        [0x80; 0x01; 0x00; 0x02 // version & message type (int32)
         0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8 (int32)
         0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
         0x00; 0x00; 0x00; 0x02] // ID (int32)
        --
        fun p -> p.ReadMessageHeader()
        ==>
        fun header ->
            header.Id <=> 2
            header.MessageType <=> ThriftMessageType.Reply
            header.Name <=> "Message"

    [<Test>]
    member __.``MessageHeader - exception``() =   
        [ 0x80; 0x01; 0x00; 0x03 // version & message type (int32)
          0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8 (int32)
          0x4D; 0x65; // "Me" in UTF-8 (string)
          0x00; 0x00; 0x00; 0x00] // ID (int32)
        --
        fun p -> p.ReadMessageHeader()
        ==>
        fun header ->
            header.Id <=> 0
            header.MessageType <=> ThriftMessageType.Exception
            header.Name <=> "Me"

    [<Test>]
    member __.``Message header - wrong version``() =
        [0x80; 0x02; 0x00; 0x03 // WRONG version & message type (int32)
         0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8 (int32)
         0x4D; 0x65; // "Me" in UTF-8 (string)
         0x00; 0x00; 0x00; 0x00] // ID (int32)
        --
        fun p -> p.ReadMessageHeader()
        =//=>
        fun (ex: ThriftProtocolException) ->
            ex.ExceptionType <=> nullable ThriftProtocolExceptionType.InvalidProtocol

    [<Test>]
    member __.``Message header - old version``() =
        [0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8 (int32)
         0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
         0x03 // Message type (byte)
         0x00; 0x00; 0x00; 0x09] // ID (int32)
        --
        fun p -> p.ReadMessageHeader()
        ==>
        fun header ->
            header.Name <=> "Message"
            header.MessageType <=> ThriftMessageType.Exception
            header.Id <=> 9

    [<Test>]
    member __.``MessageEnd``() =
        needsNothing (fun p -> p.ReadMessageEnd())

    [<Test>]
    member __.``StructHeader``() =
        []
        --
        fun p -> p.ReadStructHeader()
        ==>
        fun header ->
            header.Name <=> ""

    [<Test>]
    member __.``StructEnd``() =
        needsNothing (fun p -> p.ReadStructEnd())

    [<Test>]
    member __.``FieldHeader``() =             
        [11 // type (byte)
         0; 10] // ID (int16)
        --
        fun p -> p.ReadFieldHeader()
        ==>
        fun header ->
            header.Id <=> 10s
            header.Name <=> ""
            header.FieldTypeId <=> ThriftTypeId.Binary

    [<Test>]
    member __.``FieldEnd``() =
        needsNothing (fun p -> p.ReadFieldEnd())

    [<Test>]
    member __.``FieldStop``() =
        [0] // end of field
        --
        fun p -> p.ReadFieldHeader()
        <==>
        null

    [<Test>]
    member __.``ListHeader``() =       
        [8 // element type (byte)
         0; 0; 0; 20] // size (Int32)
        --
        fun p -> p.ReadListHeader()
        ==>
        fun header ->
            header.Count <=> 20
            header.ElementTypeId <=> ThriftTypeId.Int32

    [<Test>]
    member __.``ListEnd``() =
        needsNothing (fun p -> p.ReadListEnd())

    [<Test>]
    member __.``SetHeader``() =     
        [4 // element type (byte)
         0; 0; 0; 0] // size (int32)
        --
        fun p -> p.ReadSetHeader()
        ==>
        fun header ->
            header.Count <=> 0
            header.ElementTypeId <=> ThriftTypeId.Double

    [<Test>]
    member __.``SetEnd``() =
        needsNothing (fun p -> p.ReadSetEnd())

    [<Test>]
    member __.``MapHeader``() =
        [6 // key type (byte)
         10 // value type (byte)
         0; 0; 1; 0 ] // size (int32)
        --
        fun p -> p.ReadMapHeader()
        ==>
        fun header ->
            header.Count <=> 256
            header.KeyTypeId <=> ThriftTypeId.Int16
            header.ValueTypeId <=> ThriftTypeId.Int64

    [<Test>]
    member __.``MapEnd``() =
        needsNothing (fun p -> p.ReadMapEnd())

    [<Test>]
    member __.``Boolean - true``() =
        [1] // not zero (byte)
        --
        fun p -> p.ReadBoolean()
        <==>
        true

    [<Test>]
    member __.``Boolean - false``() =
        [0] // zero (byte)
        --
        fun p -> p.ReadBoolean()
        <==>
        false

    [<Test>]
    member __.``SByte``() =
        [123]
        --
        fun p -> p.ReadSByte()
        <==>
        123y

    [<Test>]
    member __.``Double - positive``() =
        [0x41; 0x32; 0xD6; 0x87; 0xE3; 0xD7; 0x0A; 0x3D] // 64 bit IEEE-754 floating-point number
        --
        fun p -> p.ReadDouble()
        <==>
        1234567.89

    [<Test>]
    member __.``Double - zero``() =
        [0; 0; 0; 0; 0; 0; 0; 0] // 64 bit IEEE-754 floating-point number
        --
        fun p -> p.ReadDouble()
        <==>
        0.0

    [<Test>]
    member __.``Double - negative``() =
        [0xC5; 0xF8; 0xEE; 0x90; 0xFF; 0x6C; 0x37; 0x3E] // 64-bit IEEE-754 floating-point number
        --
        fun p -> p.ReadDouble()
        <==>
        -123456789012345678901234567890.1234567890

    [<Test>]
    member __.``Double - PositiveInfinity``() =
        [0x7F; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00] // 64-bit IEEE-754 floating-point number
        --
        fun p -> p.ReadDouble()
        ==>
        fun double ->
            System.Double.IsPositiveInfinity(double) <=> true

    [<Test>]
    member __.``Double - NegativeInfinity``() =
        [0xFF; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00] // 64-bit IEEE-754 floating-point number
        --
        fun p -> p.ReadDouble()
        ==>
        fun double ->
            System.Double.IsNegativeInfinity(double) <=> true

    [<Test>]
    member __.``Double - Epsilon``() =
        [0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x01] // 64-bit IEEE-754 floating-point number
        --
        fun p -> p.ReadDouble()
        <==>
        System.Double.Epsilon

    [<Test>]
    member __.``Int16 - positive``() =
        [48; 57] // int16
        --
        fun p -> p.ReadInt16()
        <==>
        12345s

    [<Test>]
    member __.``Int16 - zero``() =
        [0; 0] // int16
        --
        fun p -> p.ReadInt16()
        <==>
        0s

    [<Test>]
    member __.``Int16 - negative``() =
        [251; 35] // int16
        --
        fun p -> p.ReadInt16()
        <==>
        -1245s

    [<Test>]
    member __.``Int32 - positive``() =
        [73; 150; 2; 210] // int32
        --
        fun p -> p.ReadInt32()
        <==>
        1234567890

    [<Test>]
    member __.``Int32 - zero``() =
        [0; 0; 0; 0] // int32
        --
        fun p -> p.ReadInt32()
        <==>
        0

    [<Test>]
    member __.``Int32 - negative``() =
        [197; 33; 151; 79] // int32
        --
        fun p -> p.ReadInt32()
        <==>
        -987654321

    [<Test>]
    member __.``Int64 - positive``() =
        [17; 34; 16; 244; 177; 108; 28; 177] // int64
        --
        fun p -> p.ReadInt64()
        <==>
        1234567890987654321L

    [<Test>]
    member __.``Int64 - zero``() =
        [0; 0; 0; 0; 0; 0; 0; 0] // int64
        --
        fun p -> p.ReadInt64()
        <==>
        0L

    [<Test>]
    member __.``Int64 - negative``() =
        [242; 75; 37; 160; 129; 11; 237; 79] // int64
        --
        fun p -> p.ReadInt64()
        <==>
        -987654321987654321L

    [<Test>]
    member __.``String - ASCII range``() =
        [0; 0; 0; 22 // length (int32)
         84; 104; 101; 32; 113; 117; 105; 99; 107; 32; 98   // data
         114; 111; 119; 110; 32; 102; 111; 120; 46; 46; 46] // data
        --
        fun p -> p.ReadString()
        <==>
        "The quick brown fox..."

    [<Test>]
    member __.``String - empty``() =
        [0; 0; 0; 0] // length (int32)
        --
        fun p -> p.ReadString()
        <==>
        ""

    [<Test>]
    member __.``String - UTF-8 range``() =
        [0; 0; 0; 17 // length (int32)
         239; 183; 178; 226; 150; 188; 225; 190; 162
         225; 185; 152; 195; 136; 224; 175; 171]
        --
        fun p -> p.ReadString()
        <==>
        "ﷲ▼ᾢṘÈ௫"

    [<Test>]
    member __.``Binary - empty``() =
        [0; 0; 0; 0] // length (int32)
        --
        fun p -> p.ReadBinary()
        <==>
        [| |]
        
    [<Test>]
    member __.``Binary``() =
        [0; 0; 0; 7 // length (int32)
         4; 8; 15; 16; 23; 42; 128] // data
        --
        fun p -> p.ReadBinary()
        <==>
        [|4y; 8y; 15y; 16y; 23y; 42y; -128y|]