// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

namespace ThriftSharp.Tests

open System.Collections.Generic
open System.Text
open System.Threading.Tasks
open ThriftSharp.Protocols
open ThriftSharp.Transport
open ThriftSharp.Models

[<NoComparison>]
type ThriftProtocolValue =
    | MessageHeader of string * ThriftMessageType
    | MessageEnd
    | StructHeader of string
    | StructEnd
    | FieldHeader of int16 * string * ThriftTypeId
    | FieldEnd
    | FieldStop
    | MapHeader of int * ThriftTypeId * ThriftTypeId
    | MapEnd
    | ListHeader of int * ThriftTypeId
    | ListEnd
    | SetHeader of int * ThriftTypeId
    | SetEnd
    | Bool of bool
    | SByte of sbyte
    | Double of float
    | Int16 of int16
    | Int32 of int
    | Int64 of int64
    | Binary of sbyte[]

[<AutoOpen>]
module ThriftProtocolValueExtensions =
    let String (str: string) = Binary(Encoding.UTF8.GetBytes(str) |> Array.map sbyte)

type MemoryProtocol(toRead: ThriftProtocolValue list, ?transport: IThriftTransport) =
    let mutable writtenVals = []
    let toRead = Queue(toRead)

    let write value = writtenVals <- value :: writtenVals
    let read = toRead.Dequeue

    member __.WrittenValues with get() = List.rev writtenVals
    member __.IsEmpty with get() = toRead.Count = 0

    new() = new MemoryProtocol([])

    interface IThriftProtocol with
        member __.WriteMessageHeader(h) =
           write (MessageHeader (h.Name, h.MessageType))

        member __.WriteMessageEnd() =
            write MessageEnd

        member __.WriteStructHeader(h) =
            write (StructHeader (h.Name))

        member __.WriteStructEnd() =
            write StructEnd

        member __.WriteFieldHeader(h) =
            write (FieldHeader (h.Id, h.Name, h.TypeId))

        member __.WriteFieldEnd() =
            write FieldEnd

        member __.WriteFieldStop() =
            write FieldStop

        member __.WriteMapHeader(h) =
            write (MapHeader (h.Count, h.KeyTypeId, h.ValueTypeId))

        member __.WriteMapEnd() =
            write MapEnd

        member __.WriteListHeader(h) =
            write (ListHeader (h.Count, h.ElementTypeId))

        member __.WriteListEnd() =
            write ListEnd

        member __.WriteSetHeader(h) =
            write (SetHeader (h.Count, h.ElementTypeId))

        member __.WriteSetEnd() =
            write SetEnd

        member __.WriteBoolean(b) =
            write (Bool b)

        member __.WriteSByte(b) =
            write (SByte b)

        member __.WriteDouble(d) =
            write (Double d)

        member __.WriteInt16(n) =
            write (Int16 n)

        member __.WriteInt32(n) =
            write (Int32 n)

        member __.WriteInt64(n) =
            write (Int64 n)

        member __.WriteString(s) =
            write (String s)

        member __.WriteBinary(bs) =
            write (Binary bs)


        member __.FlushAndReadAsync() =
            match transport with
            | Some t -> t.FlushAndReadAsync()
            | None -> Task.FromResult(0) :> Task


        member __.ReadMessageHeader() =
            match read() with
            | MessageHeader (name, typ) -> ThriftMessageHeader(name, typ)
            | x -> failwithf "Expected a message header, got %A" x

        member __.ReadMessageEnd() =
            match read() with
            | MessageEnd -> ()
            | x -> failwithf "Expected a message end, got %A" x

        member __.ReadStructHeader() =
            match read() with
            | StructHeader name -> ThriftStructHeader(name)
            | x -> failwithf "Expected a struct header, got %A" x

        member __.ReadStructEnd() =
            match read() with
            | StructEnd -> ()
            | x -> failwithf "Expected a struct end, got %A" x

        member __.ReadFieldHeader() =
            match read() with
            | FieldHeader (id, name, typ) -> ThriftFieldHeader(id, name, typ)
            | FieldStop -> Unchecked.defaultof<ThriftFieldHeader>
            | x -> failwithf "Expected a field header or stop, got %A" x

        member __.ReadFieldEnd() =
            match read() with
            | FieldEnd -> ()
            | x -> failwithf "Expected a field end, got %A" x

        member __.ReadMapHeader() =
            match read() with
            | MapHeader (len, kt, valt) -> ThriftMapHeader(len, kt, valt)
            | x -> failwithf "Expected a map header, got %A" x

        member __.ReadMapEnd() =
            match read() with
            | MapEnd -> ()
            | x -> failwithf "Expected a map end, got %A" x

        member __.ReadListHeader() =
            match read() with
            | ListHeader (len, typ) -> ThriftCollectionHeader(len, typ)
            | x -> failwithf "Expected a list header, got %A" x

        member __.ReadListEnd() =
            match read() with
            | ListEnd -> ()
            | x -> failwithf "Expected a list end, got %A" x

        member __.ReadSetHeader() =
            match read() with
            | SetHeader (len, typ) -> ThriftCollectionHeader(len, typ)
            | x -> failwithf "Expected a set header, got %A" x

        member __.ReadSetEnd() =
            match read() with
            | SetEnd -> ()
            | x -> failwithf "Expected a set end, got %A" x

        member __.ReadBoolean() =
            match read() with
            | Bool b -> b
            | x -> failwithf "Expected a bool, got %A" x

        member __.ReadSByte() =
            match read() with
            | SByte b -> b
            | x -> failwithf "Expected a sbyte, got %A" x

        member __.ReadDouble() =
            match read() with
            | Double d -> d
            | x -> failwithf "Expected a double, got %A" x

        member __.ReadInt16() =
            match read() with
            | Int16 n -> n
            | x -> failwithf "Expected an int16, got %A" x

        member __.ReadInt32() =
            match read() with
            | Int32 n -> n
            | x -> failwithf "Expected an int32, got %A" x

        member __.ReadInt64() =
            match read() with
            | Int64 n -> n
            | x -> failwithf "Expected an int64, got %A" x

        member __.ReadString() =
            match read() with
            | Binary bytes -> Encoding.UTF8.GetString(bytes |> Array.map byte)
            | x -> failwithf "Expected a string, got %A" x

        member __.ReadBinary() =
            match read() with
            | Binary bs -> bs
            | x -> failwithf "Expected binary, got %A" x

        member __.Dispose() = ()