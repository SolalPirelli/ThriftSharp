// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Binary protocol writing``

open ThriftSharp.Internals
open ThriftSharp.Protocols


let (==>) write res =
    let trans = MemoryTransport()
    write(ThriftBinaryProtocol(trans))
    trans.WrittenValues <=> (res |> List.map byte)


[<TestContainer>]
type __() =
    [<Test>]
    member __.``MessageHeader - call``() =
        fun p -> p.WriteMessageHeader(ThriftMessageHeader(2, "Message", ThriftMessageType.Call))
        ==>
        [0x80; 0x01; 0x00; 0x01 // version & message type (int32)
         0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8
         0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
         0x00; 0x00; 0x00; 0x02] // ID (int32)

    [<Test>]
    member __.``MessageHeader - one-way``() =
        fun p -> p.WriteMessageHeader(ThriftMessageHeader(0, "Me", ThriftMessageType.OneWay))
        ==>
        [0x80; 0x01; 0x00; 0x04 // version & message type (int32)
         0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8
         0x4D; 0x65; // "Me" in UTF-8 (string)
         0x00; 0x00; 0x00; 0x00] // ID (int32)

    [<Test>]
    member __.``MessageEnd``() =
        fun p -> p.WriteMessageEnd()
        ==>
        []

    [<Test>]
    member __.``StructHeader``() =
        fun p -> p.WriteStructHeader(ThriftStructHeader("Struct"))
        ==>
        []

    [<Test>]
    member __.``StructEnd``() =
        fun p -> p.WriteStructEnd()
        ==>
        []

    [<Test>]
    member __.``FieldHeader``() =
        fun p -> p.WriteFieldHeader(ThriftFieldHeader(10s, "field", ThriftTypeId.Binary))
        ==>
        [11 // type (byte)
         0; 10] // ID (Int16)

    [<Test>]
    member __.``FieldEnd``() =
        fun p -> p.WriteFieldEnd()
        ==>
        []

    [<Test>]
    member __.``FieldStop``() =
        fun p -> p.WriteFieldStop()
        ==>
        [0] // field stop

    [<Test>]
    member __.``ListHeader``() =
        fun p -> p.WriteListHeader(ThriftCollectionHeader(20, ThriftTypeId.Int32))
        ==>
        [8 // element type (byte)
         0; 0; 0; 20] // size (int32)

    [<Test>]
    member __.``ListEnd``() =
        fun p -> p.WriteListEnd()
        ==>
        []

    [<Test>]
    member __.``SetHeader``() =
        fun p -> p.WriteSetHeader(ThriftCollectionHeader(0, ThriftTypeId.Double))
        ==>
        [4 // element type (byte)
         0; 0; 0; 0] // size (int32)

    [<Test>]
    member __.``SetEnd``() =
        fun p -> p.WriteSetEnd()
        ==>
        []

    [<Test>]
    member __.``MapHeader``() =
        fun p -> p.WriteMapHeader(ThriftMapHeader(256, ThriftTypeId.Int16, ThriftTypeId.Int64))
        ==>
        [6 // key type (byte)
         10 // element type (byte)
         0; 0; 1; 0] // length (int32)

    [<Test>]
    member __.``MapEnd``() =
        fun p -> p.WriteMapEnd()
        ==>
        []

    [<Test>]
    member __.``Boolean - true``() =
        fun p -> p.WriteBoolean(true)
        ==>
        [1] // not zero

    [<Test>]
    member __.``Boolean - false``() =
        fun p -> p.WriteBoolean(false)
        ==>
        [0] // zero

    [<Test>]
    member __.``SByte``() =
        fun p -> p.WriteSByte(123y)
        ==>
        [123] // byte

    [<Test>]
    member __.``Double - positive``() =
        fun p -> p.WriteDouble(1234567.89)
        ==>
        [0x41; 0x32; 0xD6; 0x87; 0xE3; 0xD7; 0x0A; 0x3D] // 64-bit IEEE-754 floating-point number

    [<Test>]
    member __.``Double - zero``() =
        fun p -> p.WriteDouble(0.0)
        ==>
        [0; 0; 0; 0; 0; 0; 0; 0] // 64-bit IEEE-754 floating-point number

    [<Test>]
    member __.``Double - negative``() =
        fun p -> p.WriteDouble(-123456789012345678901234567890.1234567890)
        ==>
        [0xC5; 0xF8; 0xEE; 0x90; 0xFF; 0x6C; 0x37; 0x3E] // 64-bit IEEE-754 floating-point number

    [<Test>]
    member __.``Double - PositiveInfinity``() =
        fun p -> p.WriteDouble(System.Double.PositiveInfinity)
        ==>
        [0x7F; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00] // 64-bit IEEE-754 floating-point number

    [<Test>]
    member __.``Double - NegativeInfinity``() =
        fun p -> p.WriteDouble(System.Double.NegativeInfinity)
        ==>
        [0xFF; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00] // 64-bit IEEE-754 floating-point number

    [<Test>]
    member __.``Double - Epsilon``() =
        fun p -> p.WriteDouble(System.Double.Epsilon)
        ==>
        [0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x01] // 64-bit IEEE-754 floating-point number

    [<Test>]
    member __.``Int16 - positive``() =
        fun p -> p.WriteInt16(12345s)
        ==>
        [48; 57] // int16

    [<Test>]
    member __.``Int16 - zero``() =
        fun p -> p.WriteInt16(0s)
        ==>
        [0; 0] // int16

    [<Test>]
    member __.``Int16 - negative``() =
        fun p -> p.WriteInt16(-1245s)
        ==>
        [251; 35] // int16

    [<Test>]
    member __.``Int32 - positive``() =
        fun p -> p.WriteInt32(1234567890)
        ==>
        [73; 150; 2; 210] // int32

    [<Test>]
    member __.``Int32 - zero``() =
        fun p -> p.WriteInt32(0)
        ==>
        [0; 0; 0; 0] // int32

    [<Test>]
    member __.``Int32 - negative``() =
        fun p -> p.WriteInt32(-987654321)
        ==>
        [197; 33; 151; 79] // int32

    [<Test>]
    member __.``Int64 - positive``() =
        fun p -> p.WriteInt64(1234567890987654321L)
        ==>
        [17; 34; 16; 244; 177; 108; 28; 177] // int64

    [<Test>]
    member __.``Int64 - zero``() =
        fun p -> p.WriteInt64(0L)
        ==>
        [0; 0; 0; 0; 0; 0; 0; 0] // int64

    [<Test>]
    member __.``Int64 - negative``() =
        fun p -> p.WriteInt64(-987654321987654321L)
        ==>
        [242; 75; 37; 160; 129; 11; 237; 79] // int64

    [<Test>]
    member __.``String - ASCII range``() =
        fun p -> p.WriteString("The quick brown fo__...")
        ==>
        [0; 0; 0; 23 // length (int32)
         84; 104; 101; 32; 113; 117; 105; 99; 107; 32; 98      // data
         114; 111; 119; 110; 32; 102; 111; 95; 95; 46; 46; 46] // data 

    [<Test>]
    member __.``String - empty``() =
        fun p -> p.WriteString("")
        ==>
        [0; 0; 0; 0] // length (int32)

    [<Test>]
    member __.``String - UTF-8 range``() =
        fun p -> p.WriteString("ﷲ▼ᾢṘÈ௫")
        ==>
        [0; 0; 0; 17 // length (int32)
         239; 183; 178; 226; 150; 188; 225; 190; 162 // data 
         225; 185; 152; 195; 136; 224; 175; 171]     // data

    [<Test>]
    member __.``Binary - empty``() =
        fun p -> p.WriteBinary([| |])
        ==>
        [0; 0; 0; 0] // length (int32)

    [<Test>]
    member __.``Binary``() =
        fun p -> p.WriteBinary([|4y; 8y; 15y; 16y; 23y; 42y; -128y|])
        ==>
        [0; 0; 0; 7 // length (int32)
         4; 8; 15; 16; 23; 42; 128] // data