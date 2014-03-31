// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Tests

open System.IO
open System.Threading.Tasks
open ThriftSharp.Transport

type CircularTransport() =
    let stream = new MemoryStream()
    let mutable hasRead = false

    interface IThriftTransport with
        member x.WriteByte(b) =
            (x :> IThriftTransport).WriteBytes([| b |])

        member x.WriteBytes(bs) =
            if hasRead then failwith "Cannot write after a read. Close the transport first."
            stream.Write(bs, 0, bs.Length)

        member x.ReadByteAsync() =
            Task.FromResult((x :> IThriftTransport).ReadBytesAsync(1).Result.[0])
            
        member x.ReadBytesAsync(len) =
            if not hasRead then
                hasRead <- true
                stream.Position <- 0L
            let bs = Array.zeroCreate len
            if stream.Read(bs, 0, bs.Length) = len then
                Task.FromResult(bs)
            else
                failwith "Not enough bytes were read."

        member x.FlushAsync() =
            Task.FromResult(0) :> Task

        member x.Dispose() =
            ()