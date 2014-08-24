// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open System.Collections.Generic
open System.Threading.Tasks
open ThriftSharp.Transport

type MemoryTransport(toRead: byte list) =
    let mutable writtenVals = []
    let mutable hasRead = false
    let mutable isDisposed = false
    let toRead = Queue(toRead)

    let write values = for value in values do writtenVals <- value::writtenVals
    let read len = [| for x = 1 to len do
                          if toRead.Count > 0 then yield toRead.Dequeue()
                          else failwith "Not enough bytes were read." |]

    member x.WrittenValues with get() = List.rev writtenVals
    member x.IsEmpty with get() = toRead.Count = 0
    member x.IsDisposed with get() = isDisposed
    member x.HasRead with get() = hasRead

    new() = MemoryTransport([])

    interface IThriftTransport with
        member x.WriteByte(b) =
            (x :> IThriftTransport).WriteBytes([| b |])

        member x.WriteBytes(bs) =
            if isDisposed then
                failwith "Already disposed."
            if hasRead then failwith "Cannot write after a read. Close the transport first."
            write bs

        member x.ReadByte() =
            (x :> IThriftTransport).ReadBytes(1).[0]
            
        member x.ReadBytes(len) =
            if isDisposed then
                failwith "Already disposed."
            if not hasRead then
                hasRead <- true
            read len

        member x.FlushAndReadAsync() =
            if isDisposed then
                failwith "Already disposed."
            hasRead <- true
            Task.FromResult(0) :> Task

        member x.Dispose() =
            if isDisposed then
                failwith "Cannot dispose twice."
            isDisposed <- true