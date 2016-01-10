// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Tests

open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open ThriftSharp.Transport

type MemoryTransport(toRead: byte list, ?token: CancellationToken) =
    let mutable writtenVals = []
    let mutable hasRead = false
    let mutable isDisposed = false
    let toRead = Queue(toRead)

    member x.WrittenValues with get() = writtenVals
    member x.IsEmpty with get() = toRead.Count = 0
    member x.IsDisposed with get() = isDisposed
    member x.HasRead with get() = hasRead

    new() = MemoryTransport([])

    interface IThriftTransport with
        member x.WriteBytes(bs, off, count) =
            if isDisposed then
                failwith "Already disposed."

            if hasRead then 
                failwith "Cannot write after a read. Close the transport first."

            writtenVals <- writtenVals @ (bs |> Array.skip off |> Array.take count |> Array.toList)

        member x.ReadBytes(out, off, count) =
            if isDisposed then
                failwith "Already disposed."

            hasRead <- true
            for x = 0 to count - 1 do
                if toRead.Count > 0 then out.[off+x] <- toRead.Dequeue()
                else failwith "Not enough bytes were read."

        member x.FlushAndReadAsync() =
            if isDisposed then
                failwith "Already disposed."

            match token with
            | Some t when t.IsCancellationRequested -> Task.FromCanceled(t)
            | _ -> hasRead <- true
                   Task.CompletedTask

        member x.Dispose() =
            isDisposed <- true