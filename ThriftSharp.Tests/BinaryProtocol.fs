// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Binary protocol``

open Xunit
open ThriftSharp
open ThriftSharp.Models
open ThriftSharp.Protocols

let (--) a b = a, b
let (==) (a,b) c = a, b, c

[<AbstractClass>]
type Tests()  =
    abstract Test : bin: int list * rw: (IThriftProtocol -> (unit -> 'a) * ('a -> unit)) * inst: 'a -> unit

    member x.TestEmpty rw = 
        x.Test ([], rw, ())

    [<Fact>]
    member x.``MessageHeader: call``() =
        x.Test (
            [0x80; 0x01; 0x00; 0x01 // version & message type (int32)
             0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8
             0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
             0x00; 0x00; 0x00; 0x00] // ID (int32)
            --
            fun p -> p.ReadMessageHeader, p.WriteMessageHeader
            ==
            ThriftMessageHeader("Message", ThriftMessageType.Call)
        )

    [<Fact>]
    member x.``MessageHeader: one-way``() =
        x.Test (
            [0x80; 0x01; 0x00; 0x04 // version & message type (int32)
             0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8
             0x4D; 0x65; // "Me" in UTF-8 (string)
             0x00; 0x00; 0x00; 0x00] // ID (int32)
            --
            fun p -> p.ReadMessageHeader, p.WriteMessageHeader
            ==
            ThriftMessageHeader("Me", ThriftMessageType.OneWay)
        )

    [<Fact>]
    member x.``MessageHeader: reply``() =
        x.Test (
            [0x80; 0x01; 0x00; 0x02 // version & message type (int32)
             0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8 (int32)
             0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
             0x00; 0x00; 0x00; 0x00] // ID (int32)
            --
            fun p -> p.ReadMessageHeader, p.WriteMessageHeader
            ==
            ThriftMessageHeader("Message", ThriftMessageType.Reply)
        )

    [<Fact>]
    member x.``MessageHeader: exception``() =
        x.Test ( 
            [0x80; 0x01; 0x00; 0x03 // version & message type (int32)
             0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8 (int32)
             0x4D; 0x65; // "Me" in UTF-8 (string)
             0x00; 0x00; 0x00; 0x00] // ID (int32)
            --
            fun p -> p.ReadMessageHeader, p.WriteMessageHeader
            ==
            ThriftMessageHeader("Me", ThriftMessageType.Exception)
        )

    [<Fact>]
    member x.``MessageEnd``() =
        x.TestEmpty (
            fun p -> p.ReadMessageEnd, p.WriteMessageEnd
        )

    [<Fact>]
    member x.``StructHeader``() =
        x.Test (
            []
            --
            fun p -> p.ReadStructHeader, p.WriteStructHeader
            ==
            ThriftStructHeader(null)
        )

    [<Fact>]
    member x.``StructEnd``() =
        x.TestEmpty (
            fun p -> p.ReadStructEnd, p.WriteStructEnd
        )

    [<Fact>]
    member x.``FieldHeader``() =
        x.Test (
            [11 // type (byte)
             0; 10] // ID (int16)
            --
            fun p -> p.ReadFieldHeader, p.WriteFieldHeader
            ==
            ThriftFieldHeader(10s, null, ThriftTypeId.Binary)
        )

    [<Fact>]
    member x.``FieldEnd``() =
        x.TestEmpty (
            fun p -> p.ReadFieldEnd, p.WriteFieldEnd
        )

    [<Fact>]
    member x.``ListHeader``() =
        x.Test (
            [8 // element type (byte)
             0; 0; 0; 20] // size (Int32)
            --
            fun p -> p.ReadListHeader, p.WriteListHeader
            ==
            ThriftCollectionHeader(20, ThriftTypeId.Int32)
        )

    [<Fact>]
    member x.``ListEnd``() =
        x.TestEmpty (
            fun p -> p.ReadListEnd, p.WriteListEnd
        )

    [<Fact>]
    member x.``SetHeader``() =
        x.Test (
            [4 // element type (byte)
             0; 0; 0; 0] // size (int32)
            --
            fun p -> p.ReadSetHeader, p.WriteSetHeader
            ==
            ThriftCollectionHeader(0, ThriftTypeId.Double)
        )

    [<Fact>]
    member x.``SetEnd``() =
        x.TestEmpty (
            fun p -> p.ReadSetEnd, p.WriteSetEnd
        )

    [<Fact>]
    member x.``MapHeader``() =
        x.Test (
            [6 // key type (byte)
             10 // value type (byte)
             0; 0; 1; 0 ] // size (int32)
            --
            fun p -> p.ReadMapHeader, p.WriteMapHeader
            ==
            ThriftMapHeader(256, ThriftTypeId.Int16, ThriftTypeId.Int64)
        )

    [<Fact>]
    member x.``MapEnd``() =
        x.TestEmpty (
            fun p -> p.ReadMapEnd, p.WriteMapEnd
        )

    [<Fact>]
    member x.``Boolean: true``() =
        x.Test (
            [1] // not zero (byte)
            --
            fun p -> p.ReadBoolean, p.WriteBoolean
            ==
            true
        )

    [<Fact>]
    member x.``Boolean: false``() =
        x.Test (
            [0] // zero (byte)
            --
            fun p -> p.ReadBoolean, p.WriteBoolean
            ==
            false
        )

    [<Fact>]
    member x.``SByte``() =
        x.Test (
            [123]
            --
            fun p -> p.ReadSByte, p.WriteSByte
            ==
            123y
        )

    [<Fact>]
    member x.``Double: positive``() =
        x.Test (
            [0x41; 0x32; 0xD6; 0x87; 0xE3; 0xD7; 0x0A; 0x3D] // 64 bit IEEE-754 floating-point number
            --
            fun p -> p.ReadDouble, p.WriteDouble
            ==
            1234567.89
        )

    [<Fact>]
    member x.``Double: zero``() =
        x.Test (
            [0; 0; 0; 0; 0; 0; 0; 0] // 64 bit IEEE-754 floating-point number
            --
            fun p -> p.ReadDouble, p.WriteDouble
            ==
            0.0
        )

    [<Fact>]
    member x.``Double: negative``() =
        x.Test (
            [0xC5; 0xF8; 0xEE; 0x90; 0xFF; 0x6C; 0x37; 0x3E] // 64-bit IEEE-754 floating-point number
            --
            fun p -> p.ReadDouble, p.WriteDouble
            ==
            -123456789012345678901234567890.1234567890
        )

    [<Fact>]
    member x.``Double: PositiveInfinity``() =
        x.Test (
            [0x7F; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00] // 64-bit IEEE-754 floating-point number
            --
            fun p -> p.ReadDouble, p.WriteDouble
            ==
            System.Double.PositiveInfinity
        )

    [<Fact>]
    member x.``Double: NegativeInfinity``() =
        x.Test (
            [0xFF; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00] // 64-bit IEEE-754 floating-point number
            --
            fun p -> p.ReadDouble, p.WriteDouble
            ==
            System.Double.NegativeInfinity
        )

    [<Fact>]
    member x.``Double: Epsilon``() =
        x.Test (
            [0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x01] // 64-bit IEEE-754 floating-point number
            --
            fun p -> p.ReadDouble,p.WriteDouble
            ==
            System.Double.Epsilon
        )

    [<Fact>]
    member x.``Int16: positive``() =
        x.Test (
            [48; 57] // int16
            --
            fun p -> p.ReadInt16, p.WriteInt16
            ==
            12345s
        )

    [<Fact>]
    member x.``Int16: zero``() =
        x.Test (
            [0; 0] // int16
            --
            fun p -> p.ReadInt16, p.WriteInt16
            ==
            0s
        )

    [<Fact>]
    member x.``Int16: negative``() =
        x.Test (
            [251; 35] // int16
            --
            fun p -> p.ReadInt16, p.WriteInt16
            ==
            -1245s
        )

    [<Fact>]
    member x.``Int32: positive``() =
        x.Test (
            [73; 150; 2; 210] // int32
            --
            fun p -> p.ReadInt32, p.WriteInt32
            ==
            1234567890
        )

    [<Fact>]
    member x.``Int32: zero``() =
        x.Test (
            [0; 0; 0; 0] // int32
            --
            fun p -> p.ReadInt32, p.WriteInt32
            ==
            0
        )

    [<Fact>]
    member x.``Int32: negative``() =
        x.Test (
            [197; 33; 151; 79] // int32
            --
            fun p -> p.ReadInt32, p.WriteInt32
            ==
            -987654321
        )

    [<Fact>]
    member x.``Int64: positive``() =
        x.Test (
            [17; 34; 16; 244; 177; 108; 28; 177] // int64
            --
            fun p -> p.ReadInt64, p.WriteInt64
            ==
            1234567890987654321L
        )

    [<Fact>]
    member x.``Int64: zero``() =
        x.Test (
            [0; 0; 0; 0; 0; 0; 0; 0] // int64
            --
            fun p -> p.ReadInt64, p.WriteInt64
            ==
            0L
        )

    [<Fact>]
    member x.``Int64: negative``() =
        x.Test (
            [242; 75; 37; 160; 129; 11; 237; 79] // int64
            --
            fun p -> p.ReadInt64, p.WriteInt64
            ==
            -987654321987654321L
        )

    [<Fact>]
    member x.``String: ASCII range``() =
        x.Test (
            [0; 0; 0; 22 // length (int32)
             84; 104; 101; 32; 113; 117; 105; 99; 107; 32; 98   // data
             114; 111; 119; 110; 32; 102; 111; 120; 46; 46; 46] // data
            --
            fun p -> p.ReadString, p.WriteString
            ==
            "The quick brown fox..."
        )

    [<Fact>]
    member x.``String: empty``() =
        x.Test (
            [0; 0; 0; 0] // length (int32)
            --
            fun p -> p.ReadString, p.WriteString
            ==
            ""
        )

    [<Fact>]
    member x.``String: UTF-8 range``() =
        x.Test (
            [0; 0; 0; 17 // length (int32)
             239; 183; 178; 226; 150; 188; 225; 190; 162
             225; 185; 152; 195; 136; 224; 175; 171]
            --
            fun p -> p.ReadString, p.WriteString
            ==
            "ﷲ▼ᾢṘÈ௫"
        )

    [<Fact>]
    member x.``Binary: empty``() =
        x.Test (
            [0; 0; 0; 0] // length (int32)
            --
            fun p -> p.ReadBinary, p.WriteBinary
            ==
            [| |]
        )

    [<Fact>]
    member x.``Binary``() =
        x.Test (
            [0; 0; 0; 7 // length (int32)
             4; 8; 15; 16; 23; 42; 128] // data
            --
            fun p -> p.ReadBinary, p.WriteBinary
            ==
            [| 4y; 8y; 15y; 16y; 23y; 42y; -128y |]
        )

type Reading() =
    inherit Tests() 

    [<Fact>]
    let ``Message header: wrong version``() =
        let data = 
            [0x80; 0x02; 0x00; 0x03 // WRONG version & message type (int32)
             0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8 (int32)
             0x4D; 0x65; // "Me" in UTF-8 (string)
             0x00; 0x00; 0x00; 0x00] // ID (int32)
        let trans = MemoryTransport(data |> List.map byte)

        let exn = Assert.Throws<ThriftProtocolException>(ThriftBinaryProtocol(trans).ReadMessageHeader >> box)
        exn.ExceptionType <=> nullable(ThriftProtocolExceptionType.InvalidProtocol)

    [<Fact>]
    let ``Message header: old version``() =
        let data =
            [0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8 (int32)
             0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
             0x03 // Message type (byte)
             0x00; 0x00; 0x00; 0x00] // ID (int32)
        let trans = MemoryTransport(data |> List.map byte)
        ThriftBinaryProtocol(trans).ReadMessageHeader() <=> ThriftMessageHeader("Message", ThriftMessageType.Exception)
        trans.IsEmpty <=> true

    [<Fact>]
    let ``FieldStop``() =
        let trans = MemoryTransport([0uy])
        ThriftBinaryProtocol(trans).ReadFieldHeader() <=> null
        trans.IsEmpty <=> true

    override x.Test (bin, rw, inst) =
        let trans = MemoryTransport(bin |> List.map byte)
        let obj = (ThriftBinaryProtocol(trans) |> rw |> fst) ()
        obj <=> inst
        trans.IsEmpty <=> true

type Writing() =
    inherit Tests()
    [<Fact>]
    let ``FieldStop``() =
        let trans = MemoryTransport()
        do ThriftBinaryProtocol(trans).WriteFieldStop()
        trans.WrittenValues <=> [0uy]

    override x.Test (bin, rw, inst) =
        let trans = MemoryTransport()
        do (ThriftBinaryProtocol(trans) |> rw |> snd) inst
        trans.WrittenValues <=> (bin |> List.map byte)

type Other() =
    [<Fact>]
    let ``Dispose() works``() =
        let trans = MemoryTransport()
        let prot = ThriftBinaryProtocol(trans)
        do prot.Dispose()
        trans.IsDisposed <=> true

    [<Fact>]
    let ``FlushAndReadAsync() works``() = asTask <| async {
        let trans = MemoryTransport()
        let prot = ThriftBinaryProtocol(trans)
        do! prot.FlushAndReadAsync() |> Async.AwaitTask
        trans.HasRead <=> true
    }