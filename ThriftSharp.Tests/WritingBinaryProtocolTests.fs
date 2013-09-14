// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp.Internals
open ThriftSharp.Protocols

[<TestClass>]
type ``Binary protocol writing``() =
    let (==>) write res =
        let trans = MemoryTransport()
        write(ThriftBinaryProtocol(trans))
        trans.WrittenValues <===> (res |> List.map byte)

    [<Test>]
    member x.``MessageHeader - call``() =
        fun p -> p.WriteMessageHeader(ThriftMessageHeader(2, "Message", ThriftMessageType.Call))
        ==>
        [ 0x80; 0x01; 0x00; 0x01 // Version & message type (Int32)
          0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8
          0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
          0x00; 0x00; 0x00; 0x02 ] // ID (Int32)

    [<Test>]
    member x.``MessageHeader - one-way``() =
        fun p -> p.WriteMessageHeader(ThriftMessageHeader(0, "Me", ThriftMessageType.OneWay))
        ==>
        [ 0x80; 0x01; 0x00; 0x04 // Version & message type (Int32)
          0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8
          0x4D; 0x65; // "Me" in UTF-8 (string)
          0x00; 0x00; 0x00; 0x00 ] // ID (Int32)

    [<Test>]
    member x.``MessageEnd``() =
        fun p -> p.WriteMessageEnd()
        ==>
        []

    [<Test>]
    member x.``StructHeader``() =
        fun p -> p.WriteStructHeader(ThriftStructHeader("Struct"))
        ==>
        []

    [<Test>]
    member x.``StructEnd``() =
        fun p -> p.WriteStructEnd()
        ==>
        []

    [<Test>]
    member x.``FieldHeader``() =
        fun p -> p.WriteFieldHeader(ThriftFieldHeader(10s, "field", ThriftType.String))
        ==>
        [ 11 // Type (byte)
          0; 10 ] // ID (Int16)

    [<Test>]
    member x.``FieldEnd``() =
        fun p -> p.WriteFieldEnd()
        ==>
        []

    [<Test>]
    member x.``FieldStop``() =
        fun p -> p.WriteFieldStop()
        ==>
        [ 0 ]

    [<Test>]
    member x.``ListHeader``() =
        fun p -> p.WriteListHeader(ThriftCollectionHeader(20, ThriftType.Int32))
        ==>
        [ 8 // element type (byte)
          0; 0; 0; 20 ] // size (Int32)

    [<Test>]
    member x.``ListEnd``() =
        fun p -> p.WriteListEnd()
        ==>
        []

    [<Test>]
    member x.``SetHeader``() =
        fun p -> p.WriteSetHeader(ThriftCollectionHeader(0, ThriftType.Double))
        ==>
        [ 4 // element type (byte)
          0; 0; 0; 0 ] // size (Int32)

    [<Test>]
    member x.``SetEnd``() =
        fun p -> p.WriteSetEnd()
        ==>
        []

    [<Test>]
    member x.``MapHeader``() =
        fun p -> p.WriteMapHeader(ThriftMapHeader(256, ThriftType.Int16, ThriftType.Int64))
        ==>
        [ 6
          10
          0; 0; 1; 0 ]

    [<Test>]
    member x.``MapEnd``() =
        fun p -> p.WriteMapEnd()
        ==>
        []

    [<Test>]
    member x.``Boolean - true``() =
        fun p -> p.WriteBoolean(true)
        ==>
        [ 1 ]

    [<Test>]
    member x.``Boolean - false``() =
        fun p -> p.WriteBoolean(false)
        ==>
        [ 0 ]

    [<Test>]
    member x.``SByte``() =
        fun p -> p.WriteSByte(123y)
        ==>
        [ 123 ]

    [<Test>]
    member x.``Double - positive``() =
        fun p -> p.WriteDouble(1234567.89)
        ==>
        [ 0x41; 0x32; 0xD6; 0x87; 0xE3; 0xD7; 0x0A; 0x3D ]

    [<Test>]
    member x.``Double - zero``() =
        fun p -> p.WriteDouble(0.0)
        ==>
        [ 0; 0; 0; 0; 0; 0; 0; 0 ]

    [<Test>]
    member x.``Double - negative``() =
        fun p -> p.WriteDouble(-123456789012345678901234567890.1234567890)
        ==>
        [ 0xC5; 0xF8; 0xEE; 0x90; 0xFF; 0x6C; 0x37; 0x3E ]

    [<Test>]
    member x.``Double - PositiveInfinity``() =
        fun p -> p.WriteDouble(System.Double.PositiveInfinity)
        ==>
        [ 0x7F; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00 ]

    [<Test>]
    member x.``Double - NegativeInfinity``() =
        fun p -> p.WriteDouble(System.Double.NegativeInfinity)
        ==>
        [ 0xFF; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00 ]

    [<Test>]
    member x.``Double - Epsilon``() =
        fun p -> p.WriteDouble(System.Double.Epsilon)
        ==>
        [ 0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x01 ]

    [<Test>]
    member x.``Int16 - positive``() =
        fun p -> p.WriteInt16(12345s)
        ==>
        [ 48; 57 ]

    [<Test>]
    member x.``Int16 - zero``() =
        fun p -> p.WriteInt16(0s)
        ==>
        [ 0; 0 ]

    [<Test>]
    member x.``Int16 - negative``() =
        fun p -> p.WriteInt16(-1245s)
        ==>
        [ 251; 35 ]

    [<Test>]
    member x.``Int32 - positive``() =
        fun p -> p.WriteInt32(1234567890)
        ==>
        [ 73; 150; 2; 210 ]

    [<Test>]
    member x.``Int32 - zero``() =
        fun p -> p.WriteInt32(0)
        ==>
        [ 0; 0; 0; 0 ]

    [<Test>]
    member x.``Int32 - negative``() =
        fun p -> p.WriteInt32(-987654321)
        ==>
        [ 197; 33; 151; 79 ]

    [<Test>]
    member x.``Int64 - positive``() =
        fun p -> p.WriteInt64(1234567890987654321L)
        ==>
        [ 17; 34; 16; 244; 177; 108; 28; 177 ]

    [<Test>]
    member x.``Int64 - zero``() =
        fun p -> p.WriteInt64(0L)
        ==>
        [ 0; 0; 0; 0; 0; 0; 0; 0 ]

    [<Test>]
    member x.``Int64 - negative``() =
        fun p -> p.WriteInt64(-987654321987654321L)
        ==>
        [ 242; 75; 37; 160; 129; 11; 237; 79 ]

    [<Test>]
    member x.``String - ASCII range``() =
        fun p -> p.WriteString("The quick brown fox...")
        ==>
        [ 0; 0; 0; 22 // length (Int32)
          84; 104; 101; 32; 113; 117; 105; 99; 107; 32; 98;
          114; 111; 119; 110; 32; 102; 111; 120; 46; 46; 46 ]

    [<Test>]
    member x.``String - empty``() =
        fun p -> p.WriteString("")
        ==>
        [ 0; 0; 0; 0 // length (Int32)
         ]

    [<Test>]
    member x.``String - UTF-8 range``() =
        fun p -> p.WriteString("ﷲ▼ᾢṘÈ௫")
        ==>
        [ 0; 0; 0; 17 // length (Int32)
          239; 183; 178; 226; 150; 188; 225; 190; 162; 
          225; 185; 152; 195; 136; 224; 175; 171 ]

    [<Test>]
    member x.``Binary - empty``() =
        fun p -> p.WriteBinary([| |])
        ==>
        [ 0; 0; 0; 0 // length (Int32)
          ]

    [<Test>]
    member x.``Binary``() =
        fun p -> p.WriteBinary([| 4y; 8y; 15y; 16y; 23y; 42y; -128y |])
        ==>
        [ 0; 0; 0; 7 // length (Int32)
          4; 8; 15; 16; 23; 42; 128 ]