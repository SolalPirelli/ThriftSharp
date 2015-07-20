// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Tests

open System.IO
open System.Threading.Tasks
open ThriftSharp.Transport

type CircularTransport() =
    let stream = new MemoryStream()
    let mutable hasRead = false

    interface IThriftTransport with
        member x.WriteBytes(bs) =
            if hasRead then failwith "Cannot write after a read. Close the transport first."
            stream.Write(bs, 0, bs.Length)
            
        member x.ReadBytes(out) =
            if not hasRead then
                hasRead <- true
                stream.Position <- 0L
            stream.Read(out, 0, out.Length) |> ignore

        member x.FlushAndReadAsync() =
            Task.CompletedTask

        member x.Dispose() =
            ()