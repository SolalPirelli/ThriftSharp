// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open System.Collections.Generic
open System.Threading.Tasks
open ThriftSharp
open ThriftSharp.Protocols
open ThriftSharp.Internals

type ThriftProtocolValue =
    | MessageHeader of int * string * ThriftMessageType
    | MessageEnd
    | StructHeader of string
    | StructEnd
    | FieldHeader of int16 * string * ThriftType
    | FieldEnd
    | FieldStop
    | MapHeader of int * ThriftType * ThriftType
    | MapEnd
    | ListHeader of int * ThriftType
    | ListEnd
    | SetHeader of int * ThriftType
    | SetEnd
    | Bool of bool
    | SByte of sbyte
    | Double of float
    | Int16 of int16
    | Int32 of int
    | Int64 of int64
    | String of string
    | Binary of sbyte[]

type MemoryProtocol(toRead: ThriftProtocolValue list) =
    let mutable writtenVals = []
    let toRead = Queue(toRead)

    let write value = writtenVals <- value::writtenVals
    let read() = toRead.Dequeue()
    let makeTask o = Task.FromResult(o)
    let emptyTask = Task.FromResult(0) :> Task

    member x.WrittenValues with get() = List.rev writtenVals
    member x.IsEmpty with get() = toRead.Count = 0

    new() = MemoryProtocol([])

    interface IThriftProtocol with
        member x.WriteMessageHeader(h) =
           write (MessageHeader (h.Id, h.Name, h.MessageType))

        member x.WriteMessageEnd() =
            write MessageEnd

        member x.WriteStructHeader(h) =
            write (StructHeader (h.Name))

        member x.WriteStructEnd() =
            write StructEnd

        member x.WriteFieldHeader(h) =
            write (FieldHeader (h.Id, h.Name, h.FieldType))

        member x.WriteFieldEnd() =
            write FieldEnd

        member x.WriteFieldStop() =
            write FieldStop

        member x.WriteMapHeader(h) =
            write (MapHeader (h.Count, h.KeyType, h.ValueType))

        member x.WriteMapEnd() =
            write MapEnd

        member x.WriteListHeader(h) =
            write (ListHeader (h.Count, h.ElementType))

        member x.WriteListEnd() =
            write ListEnd

        member x.WriteSetHeader(h) =
            write (SetHeader (h.Count, h.ElementType))

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

        member x.FlushAsync() =
            emptyTask


        member x.ReadMessageHeaderAsync() =
            match read() with
            | MessageHeader (id, name, typ) -> makeTask (ThriftMessageHeader(id, name, typ))
            | x -> failwithf "Expected a message header, got %A" x

        member x.ReadMessageEndAsync() =
            match read() with
            | MessageEnd -> emptyTask
            | x -> failwithf "Expected a message end, got %A" x

        member x.ReadStructHeaderAsync() =
            match read() with
            | StructHeader name -> makeTask (ThriftStructHeader(name))
            | x -> failwithf "Expected a struct header, got %A" x

        member x.ReadStructEndAsync() =
            match read() with
            | StructEnd -> emptyTask
            | x -> failwithf "Expected a struct end, got %A" x

        member x.ReadFieldHeaderAsync() =
            match read() with
            | FieldHeader (id, name, typ) -> makeTask (ThriftFieldHeader(id, name, typ))
            | FieldStop -> makeTask null
            | x -> failwithf "Expected a field header or stop, got %A" x

        member x.ReadFieldEndAsync() =
            match read() with
            | FieldEnd -> emptyTask
            | x -> failwithf "Expected a field end, got %A" x

        member x.ReadMapHeaderAsync() =
            match read() with
            | MapHeader (len, kt, valt) -> makeTask (ThriftMapHeader(len, kt, valt))
            | x -> failwithf "Expected a map header, got %A" x

        member x.ReadMapEndAsync() =
            match read() with
            | MapEnd -> emptyTask
            | x -> failwithf "Expected a map end, got %A" x

        member x.ReadListHeaderAsync() =
            match read() with
            | ListHeader (len, typ) -> makeTask (ThriftCollectionHeader(len, typ))
            | x -> failwithf "Expected a list header, got %A" x

        member x.ReadListEndAsync() =
            match read() with
            | ListEnd -> emptyTask
            | x -> failwithf "Expected a list end, got %A" x

        member x.ReadSetHeaderAsync() =
            match read() with
            | SetHeader (len, typ) -> makeTask (ThriftCollectionHeader(len, typ))
            | x -> failwithf "Expected a set header, got %A" x

        member x.ReadSetEndAsync() =
            match read() with
            | SetEnd -> emptyTask
            | x -> failwithf "Expected a set end, got %A" x

        member x.ReadBooleanAsync() =
            match read() with
            | Bool b -> makeTask b
            | x -> failwithf "Expected a bool, got %A" x

        member x.ReadSByteAsync() =
            match read() with
            | SByte b -> makeTask b
            | x -> failwithf "Expected a sbyte, got %A" x

        member x.ReadDoubleAsync() =
            match read() with
            | Double d -> makeTask d
            | x -> failwithf "Expected a double, got %A" x

        member x.ReadInt16Async() =
            match read() with
            | Int16 n -> makeTask n
            | x -> failwithf "Expected an int16, got %A" x

        member x.ReadInt32Async() =
            match read() with
            | Int32 n -> makeTask n
            | x -> failwithf "Expected an int32, got %A" x

        member x.ReadInt64Async() =
            match read() with
            | Int64 n -> makeTask n
            | x -> failwithf "Expected an int64, got %A" x

        member x.ReadStringAsync() =
            match read() with
            | String s -> makeTask s
            | x -> failwithf "Expected a string, got %A" x

        member x.ReadBinaryAsync() =
            match read() with
            | Binary bs -> makeTask bs
            | x -> failwithf "Expected binary, got %A" x

        member x.Dispose() = ()