// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals
open ThriftSharp.Protocols

[<TestClass>]
type ``Binary protocol reading``() =
    let (==>) res read =
        let trans = MemoryTransport(res |> List.map byte)
        read(new ThriftBinaryProtocol(trans))
        ()

    [<Test>]
    member x.``MessageHeader - reply``() =  
        [ 0x80; 0x01; 0x00; 0x02 // Version & message type (Int32)
          0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8
          0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
          0x00; 0x00; 0x00; 0x02 ] // ID (Int32)
        ==>
        fun p -> let h = p.ReadMessageHeader()
                 h.Id <=> 2
                 h.MessageType <=> ThriftMessageType.Reply
                 h.Name <=> "Message"

    [<Test>]
    member x.``MessageHeader - exception``() =   
        [ 0x80; 0x01; 0x00; 0x03 // Version & message type (Int32)
          0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8
          0x4D; 0x65; // "Me" in UTF-8 (string)
          0x00; 0x00; 0x00; 0x00 ] // ID (Int32)
        ==>
        fun p -> let h = p.ReadMessageHeader()
                 h.Id <=> 0
                 h.MessageType <=> ThriftMessageType.Exception
                 h.Name <=> "Me"

    [<Test>]
    member x.``Message header - wrong version``() =
        [ 0x80; 0x02; 0x00; 0x03 // WRONG Version & message type (Int32)
          0x00; 0x00; 0x00; 0x02 // "Me" length in UTF-8
          0x4D; 0x65; // "Me" in UTF-8 (string)
          0x00; 0x00; 0x00; 0x00 ] // ID (Int32)
        ==>
        fun p -> let exn = throws<ThriftProtocolException>(fun () -> box (p.ReadMessageHeader()))
                 exn.ExceptionType <=> ThriftProtocolExceptionType.InvalidProtocol

    [<Test>]
    member x.``Message header - old version``() =
        [ 0x00; 0x00; 0x00; 0x07 // "Message" length in UTF-8
          0x4D; 0x65; 0x73; 0x73; 0x61; 0x67; 0x65 // "Message" in UTF-8 (string)
          0x03 // Message type (byte)
          0x00; 0x00; 0x00; 0x09 // ID (Int32)
          ]
        ==>
        fun p -> let h = p.ReadMessageHeader()
                 h.Name <=> "Message"
                 h.MessageType <=> ThriftMessageType.Exception
                 h.Id <=> 9

    [<Test>]
    member x.``MessageEnd``() =
        []
        ==>
        fun p -> p.ReadMessageEnd()     

    [<Test>]
    member x.``StructHeader``() =
        []
        ==>
        fun p -> let h = p.ReadStructHeader()
                 h.Name <=> ""

    [<Test>]
    member x.``StructEnd``() =
        []
        ==>
        fun p -> p.ReadStructEnd()

    [<Test>]
    member x.``FieldHeader``() =             
        [ 11 // Type (byte)
          0; 10 ] // ID (Int16)
        ==>
        fun p -> let h = p.ReadFieldHeader()
                 h.Id <=> 10s
                 h.Name <=> ""
                 h.FieldType <=> ThriftType.String

    [<Test>]
    member x.``FieldEnd``() =
        []
        ==>
        fun p -> p.ReadFieldEnd()

    [<Test>]
    member x.``FieldStop``() =
        [ 0 ]
        ==>
        fun p -> p.ReadFieldHeader() <=> null

    [<Test>]
    member x.``ListHeader``() =       
        [ 8 // element type (byte)
          0; 0; 0; 20 ] // size (Int32)
        ==>
        fun p -> let h = p.ReadListHeader()
                 h.Count <=> 20
                 h.ElementType <=> ThriftType.Int32

    [<Test>]
    member x.``ListEnd``() =
        []
        ==>
        fun p -> p.ReadListEnd()

    [<Test>]
    member x.``SetHeader``() =     
        [ 4 // element type (byte)
          0; 0; 0; 0 ] // size (Int32)
        ==>
        fun p -> let h = p.ReadSetHeader()
                 h.Count <=> 0
                 h.ElementType <=> ThriftType.Double

    [<Test>]
    member x.``SetEnd``() =
        []
        ==>
        fun p -> p.ReadSetEnd()

    [<Test>]
    member x.``MapHeader``() =
        [ 6
          10
          0; 0; 1; 0 ]
        ==>
        fun p -> let h = p.ReadMapHeader()
                 h.Count <=> 256
                 h.KeyType <=> ThriftType.Int16
                 h.ValueType <=> ThriftType.Int64

    [<Test>]
    member x.``MapEnd``() =
        []
        ==>
        fun p -> p.ReadMapEnd()

    [<Test>]
    member x.``Boolean - true``() =
        [ 1 ]
        ==>
        fun p -> p.ReadBoolean() <=> true

    [<Test>]
    member x.``Boolean - false``() =
        [ 0 ]
        ==>
        fun p -> p.ReadBoolean() <=> false

    [<Test>]
    member x.``SByte``() =
        [ 123 ]
        ==>
        fun p -> p.ReadSByte() <=> 123y

    [<Test>]
    member x.``Double - positive``() =
        [ 0x41; 0x32; 0xD6; 0x87; 0xE3; 0xD7; 0x0A; 0x3D ]
        ==>
        fun p -> p.ReadDouble() <=> 1234567.89

    [<Test>]
    member x.``Double - zero``() =
        [ 0; 0; 0; 0; 0; 0; 0; 0 ]
        ==>
        fun p -> p.ReadDouble() <=> 0.0

    [<Test>]
    member x.``Double - negative``() =
        [ 0xC5; 0xF8; 0xEE; 0x90; 0xFF; 0x6C; 0x37; 0x3E ]
        ==>
        fun p -> p.ReadDouble() <=> -123456789012345678901234567890.1234567890

    [<Test>]
    member x.``Double - PositiveInfinity``() =
        [ 0x7F; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00 ]
        ==>
        fun p -> System.Double.IsPositiveInfinity(p.ReadDouble()) <=> true

    [<Test>]
    member x.``Double - NegativeInfinity``() =
        [ 0xFF; 0xF0; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00 ]
        ==>
        fun p -> System.Double.IsNegativeInfinity(p.ReadDouble()) <=> true

    [<Test>]
    member x.``Double - Epsilon``() =
        [ 0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x00; 0x01 ]
        ==>
        fun p -> p.ReadDouble() <=> System.Double.Epsilon

    [<Test>]
    member x.``Int16 - positive``() =
        [ 48; 57 ]
        ==>
        fun p -> p.ReadInt16() <=> 12345s

    [<Test>]
    member x.``Int16 - zero``() =
        [ 0; 0 ]
        ==>
        fun p -> p.ReadInt16() <=> 0s

    [<Test>]
    member x.``Int16 - negative``() =
        [ 251; 35 ]
        ==>
        fun p -> p.ReadInt16() <=> -1245s

    [<Test>]
    member x.``Int32 - positive``() =
        [ 73; 150; 2; 210 ]
        ==>
        fun p -> p.ReadInt32() <=> 1234567890

    [<Test>]
    member x.``Int32 - zero``() =
        [ 0; 0; 0; 0 ]
        ==>
        fun p -> p.ReadInt32() <=> 0

    [<Test>]
    member x.``Int32 - negative``() =
        [ 197; 33; 151; 79 ]
        ==>
        fun p -> p.ReadInt32() <=> -987654321

    [<Test>]
    member x.``Int64 - positive``() =
        [ 17; 34; 16; 244; 177; 108; 28; 177 ]
        ==>
        fun p -> p.ReadInt64() <=> 1234567890987654321L

    [<Test>]
    member x.``Int64 - zero``() =
        [ 0; 0; 0; 0; 0; 0; 0; 0 ]
        ==>
        fun p -> p.ReadInt64() <=> 0L

    [<Test>]
    member x.``Int64 - negative``() =
        [ 242; 75; 37; 160; 129; 11; 237; 79 ]
        ==>
        fun p -> p.ReadInt64() <=> -987654321987654321L

    [<Test>]
    member x.``String - ASCII range``() =
        [ 0; 0; 0; 22 // length (Int32)
          84; 104; 101; 32; 113; 117; 105; 99; 107; 32; 98;
          114; 111; 119; 110; 32; 102; 111; 120; 46; 46; 46 ]
        ==>
        fun p -> p.ReadString() <=> "The quick brown fox..."

    [<Test>]
    member x.``String - empty``() =
        [ 0; 0; 0; 0 // length (Int32)
          ]
        ==>
        fun p -> p.ReadString() <=> ""

    [<Test>]
    member x.``String - UTF-8 range``() =
        [ 0; 0; 0; 17 // length (Int32)
          239; 183; 178; 226; 150; 188; 225; 190; 162; 
          225; 185; 152; 195; 136; 224; 175; 171 ]
        ==>
        fun p -> p.ReadString() <=> "ﷲ▼ᾢṘÈ௫"

    [<Test>]
    member x.``Binary - empty``() =
        [ 0; 0; 0; 0 // length (Int32)
          ]
        ==>
        fun p -> p.ReadBinary() <=> [| |]
        
    [<Test>]
    member x.``Binary``() =
        [ 0; 0; 0; 7 // length (Int32)
          4; 8; 15; 16; 23; 42; 128 ]
        ==>
        fun p -> p.ReadBinary() <=> [| 4y; 8y; 15y; 16y; 23y; 42y; -128y |]