module ThriftSharp.Tests.``Memory transport``

open System
open Xunit
open ThriftSharp.Transport

[<Fact>]
let ``Write once, read all``() =
    let transport = new ThriftMemoryTransport()

    transport.WriteBytes([| 0uy; 1uy |], 0, 2)

    transport.FlushAndReadAsync().Wait()

    let bytes = Array.zeroCreate 2
    transport.ReadBytes(bytes, 0, 2)

    transport.Dispose()

    bytes <=> [| 0uy; 1uy |]
    
[<Fact>]
let ``Write once, read in two batches``() =
    let transport = new ThriftMemoryTransport()

    transport.WriteBytes([| 0uy; 1uy |], 0, 2)

    transport.FlushAndReadAsync().Wait()

    let bytes = Array.zeroCreate 2
    transport.ReadBytes(bytes, 0, 1)
    transport.ReadBytes(bytes, 1, 1)

    transport.Dispose()

    bytes <=> [| 0uy; 1uy |]

[<Fact>]
let ``Write twice, read all``() =
    let transport = new ThriftMemoryTransport()
    
    transport.WriteBytes([| 0uy; 0uy |], 0, 1)
    transport.WriteBytes([| 1uy; 1uy |], 1, 1)

    transport.FlushAndReadAsync().Wait()

    let bytes = Array.zeroCreate 2
    transport.ReadBytes(bytes, 0, 2)

    transport.Dispose()

    bytes <=> [| 0uy; 1uy |]
    
[<Fact>]
let ``Read from pre-existing data``() =
    let transport = new ThriftMemoryTransport([| 0uy; 1uy |])

    let bytes = Array.zeroCreate 2
    transport.ReadBytes(bytes, 0, 2)

    transport.Dispose()

    bytes <=> [| 0uy; 1uy |]

[<Fact>]
let ``Internal buffer works as expected``() =
    let transport = new ThriftMemoryTransport()
    
    transport.WriteBytes([| 0uy; 1uy |], 0, 2)

    let buffer = transport.GetInternalBuffer()

    buffer.[0] <=> 0uy
    buffer.[1] <=> 1uy

[<Fact>]
let ``Cannot write after flushing``() =
    let transport = new ThriftMemoryTransport()

    transport.FlushAndReadAsync().Wait()

    Assert.Throws<InvalidOperationException>(fun () -> transport.WriteBytes([| 0uy; 1uy |], 0, 2))

[<Fact>]
let ``Cannot write after disposing``() =
    let transport = new ThriftMemoryTransport()

    transport.Dispose()

    Assert.Throws<ObjectDisposedException>(fun () -> transport.WriteBytes([| 0uy; 1uy |], 0, 2))

[<Fact>]
let ``Cannot write if created with pre-existing data``() =
    let transport = new ThriftMemoryTransport([| 0uy; 1uy |])
    
    Assert.Throws<InvalidOperationException>(fun () -> transport.WriteBytes([| 0uy; 1uy |], 0, 2))
    
[<Fact>]
let ``Cannot read before flushing``() =
    let transport = new ThriftMemoryTransport()
    
    Assert.Throws<InvalidOperationException>(fun () -> transport.ReadBytes([| 0uy; 1uy |], 0, 2))

[<Fact>]
let ``Cannot read after disposing``() =
    let transport = new ThriftMemoryTransport()

    transport.Dispose()
    
    Assert.Throws<ObjectDisposedException>(fun () -> transport.ReadBytes([| 0uy; 1uy |], 0, 2))
    
[<Fact>]
let ``Cannot flush twice``() =
    let transport = new ThriftMemoryTransport()
    
    transport.FlushAndReadAsync().Wait()

    Assert.ThrowsAsync<InvalidOperationException>(fun () -> transport.FlushAndReadAsync())
    
[<Fact>]
let ``Cannot get the internal buffer after disposing``() =
    let transport = new ThriftMemoryTransport()

    transport.Dispose()
    
    Assert.Throws<ObjectDisposedException>(fun () -> transport.GetInternalBuffer() |> ignore)
    
#if !DEBUG
[<Fact>]
let ``Flushing loses the data``() =
    let bytes = [| 0uy; 1uy |]
    let bytesRef = WeakReference(bytes)
    let transport = new ThriftMemoryTransport(bytes)

    transport.Dispose()
    GC.Collect()
    bytesRef.IsAlive <=> false
#endif