﻿namespace ThriftSharp.Tests

open System.IO
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

        member x.ReadByte() =
            (x :> IThriftTransport).ReadBytes(1).[0]
            
        member x.ReadBytes(len) =
            if not hasRead then
                hasRead <- true
                stream.Position <- 0L
            let bs = Array.zeroCreate len
            if stream.Read(bs, 0, bs.Length) = len then
                bs
            else
                failwith "Not enough bytes were read."

        member x.Close() =
            hasRead <- false
            stream.Position <- 0L