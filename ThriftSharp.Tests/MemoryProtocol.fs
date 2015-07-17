// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Tests

open System.Collections.Generic
open System.Text
open System.Threading.Tasks
open ThriftSharp.Protocols
open ThriftSharp.Internals

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

type MemoryProtocol(toRead: ThriftProtocolValue list) =
    let mutable writtenVals = []
    let toRead = Queue(toRead)

    let write value = writtenVals <- value::writtenVals
    let read() = toRead.Dequeue()

    member x.WrittenValues with get() = List.rev writtenVals
    member x.IsEmpty with get() = toRead.Count = 0

    new() = MemoryProtocol([])

    interface IThriftProtocol with
        member x.WriteMessageHeader(h) =
           write (MessageHeader (h.Name, h.MessageType))

        member x.WriteMessageEnd() =
            write MessageEnd

        member x.WriteStructHeader(h) =
            write (StructHeader (h.Name))

        member x.WriteStructEnd() =
            write StructEnd

        member x.WriteFieldHeader(h) =
            write (FieldHeader (h.Id, h.Name, h.FieldTypeId))

        member x.WriteFieldEnd() =
            write FieldEnd

        member x.WriteFieldStop() =
            write FieldStop

        member x.WriteMapHeader(h) =
            write (MapHeader (h.Count, h.KeyTypeId, h.ValueTypeId))

        member x.WriteMapEnd() =
            write MapEnd

        member x.WriteListHeader(h) =
            write (ListHeader (h.Count, h.ElementTypeId))

        member x.WriteListEnd() =
            write ListEnd

        member x.WriteSetHeader(h) =
            write (SetHeader (h.Count, h.ElementTypeId))

        member x.WriteSetEnd() =
            write SetEnd

        member x.WriteBoolean(b) =
            write (Bool b)

        member x.WriteSByte(b) =
            write (SByte b)

        member x.WriteDouble(d) =
            write (Double d)

        member x.WriteInt16(n) =
            write (Int16 n)

        member x.WriteInt32(n) =
            write (Int32 n)

        member x.WriteInt64(n) =
            write (Int64 n)

        member x.WriteString(s) =
            write (String s)

        member x.WriteBinary(bs) =
            write (Binary bs)

        member x.FlushAndReadAsync() =
            Task.FromResult(0) :> Task


        member x.ReadMessageHeader() =
            match read() with
            | MessageHeader (name, typ) -> ThriftMessageHeader(name, typ)
            | x -> failwithf "Expected a message header, got %A" x

        member x.ReadMessageEnd() =
            match read() with
            | MessageEnd -> ()
            | x -> failwithf "Expected a message end, got %A" x

        member x.ReadStructHeader() =
            match read() with
            | StructHeader name -> ThriftStructHeader(name)
            | x -> failwithf "Expected a struct header, got %A" x

        member x.ReadStructEnd() =
            match read() with
            | StructEnd -> ()
            | x -> failwithf "Expected a struct end, got %A" x

        member x.ReadFieldHeader() =
            match read() with
            | FieldHeader (id, name, typ) -> ThriftFieldHeader(id, name, typ)
            | FieldStop -> Unchecked.defaultof<ThriftFieldHeader>
            | x -> failwithf "Expected a field header or stop, got %A" x

        member x.ReadFieldEnd() =
            match read() with
            | FieldEnd -> ()
            | x -> failwithf "Expected a field end, got %A" x

        member x.ReadMapHeader() =
            match read() with
            | MapHeader (len, kt, valt) -> ThriftMapHeader(len, kt, valt)
            | x -> failwithf "Expected a map header, got %A" x

        member x.ReadMapEnd() =
            match read() with
            | MapEnd -> ()
            | x -> failwithf "Expected a map end, got %A" x

        member x.ReadListHeader() =
            match read() with
            | ListHeader (len, typ) -> ThriftCollectionHeader(len, typ)
            | x -> failwithf "Expected a list header, got %A" x

        member x.ReadListEnd() =
            match read() with
            | ListEnd -> ()
            | x -> failwithf "Expected a list end, got %A" x

        member x.ReadSetHeader() =
            match read() with
            | SetHeader (len, typ) -> ThriftCollectionHeader(len, typ)
            | x -> failwithf "Expected a set header, got %A" x

        member x.ReadSetEnd() =
            match read() with
            | SetEnd -> ()
            | x -> failwithf "Expected a set end, got %A" x

        member x.ReadBoolean() =
            match read() with
            | Bool b -> b
            | x -> failwithf "Expected a bool, got %A" x

        member x.ReadSByte() =
            match read() with
            | SByte b -> b
            | x -> failwithf "Expected a sbyte, got %A" x

        member x.ReadDouble() =
            match read() with
            | Double d -> d
            | x -> failwithf "Expected a double, got %A" x

        member x.ReadInt16() =
            match read() with
            | Int16 n -> n
            | x -> failwithf "Expected an int16, got %A" x

        member x.ReadInt32() =
            match read() with
            | Int32 n -> n
            | x -> failwithf "Expected an int32, got %A" x

        member x.ReadInt64() =
            match read() with
            | Int64 n -> n
            | x -> failwithf "Expected an int64, got %A" x

        member x.ReadString() =
            match read() with
            | Binary bytes -> Encoding.UTF8.GetString(bytes |> Array.map byte)
            | x -> failwithf "Expected a string, got %A" x

        member x.ReadBinary() =
            match read() with
            | Binary bs -> bs
            | x -> failwithf "Expected binary, got %A" x

        member x.Dispose() = ()