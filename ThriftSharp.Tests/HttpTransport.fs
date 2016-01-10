module ThriftSharp.Tests.``HTTP transport``

open System
open System.Collections.Generic
open System.Net
open System.Threading
open Xunit
open ThriftSharp.Transport

type TestParams(request: int list, response: int list) =
    let toArray =
        List.map (fun i -> i % 256) >> List.map byte >> List.toArray

    member val Timeout = Timeout.InfiniteTimeSpan with get, set
    member val Headers = Dictionary<string, string>() with get, set
    member __.Request with get() = toArray request
    member __.Response with get() = toArray response

let mutable PortCounter = 4000

let test (ps: TestParams) = async {
    let url = sprintf "http://localhost:%d/" (Interlocked.Increment(&PortCounter))
    let transport = HttpThriftTransport(url, CancellationToken.None, ps.Headers, ps.Timeout)

    let listener = HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()

    transport.WriteBytes(ps.Request, 0, ps.Request.Length)
    let flushTask = transport.FlushAndReadAsync()

    let! context = listener.GetContextAsync() |> Async.AwaitTask

    context.Request.HttpMethod <=> "POST"
    context.Request.ContentType <=> "application/x-thrift"
    context.Request.AcceptTypes <=> [| "application/x-thrift" |]

    let! received = context.Request.InputStream.AsyncRead(ps.Request.Length)
    received <=> ps.Request

    let receivedHeaders = context.Request.Headers.AllKeys 
                       |> Seq.filter (fun k -> ps.Headers.ContainsKey(k))
                       |> Seq.map (fun k -> k, context.Request.Headers.[k])
                       |> dict
    receivedHeaders <=> ps.Headers

    do! context.Response.OutputStream.AsyncWrite(ps.Response, 0, ps.Response.Length)
    context.Response.OutputStream.Close()

    do! flushTask |> Async.AwaitTask

    let clientReceived = Array.zeroCreate ps.Response.Length
    transport.ReadBytes(clientReceived, 0, clientReceived.Length)
    clientReceived <=> ps.Response
}

[<Fact>]
let ``Client sends 0 bytes and receives 0 bytes``() = asTask <| async {
    do! test (TestParams([], []))
}

[<Fact>]
let ``Client sends 2 bytes and receives 4 bytes``() = asTask <| async {
    do! test (TestParams([0; 1], [10; 30; 60; 100]))
}

[<Fact>]
let ``Client sends 5000 bytes and receives 10000 bytes``() = asTask <| async {
    let request = List.init 5000 id
    let response = List.init 10000 id
    do! test (TestParams(request, response))
}
[<Fact>]
let ``Client sends headers along with the request``() = asTask <| async {
    let headers = dict ["X-Hello", "World"; "X-Test", "42"]
    do! test (TestParams([0], [0], Headers = headers))
}

[<Fact>]
let ``Server takes too long to respond``() = asTask <| async {
    let url = sprintf "http://localhost:%d/" (Interlocked.Increment(&PortCounter))
    let transport = HttpThriftTransport(url, CancellationToken.None, dict [], TimeSpan.FromMilliseconds(30.0))

    let listener = HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()

    transport.WriteBytes([| |], 0, 0)
    let flushTask = transport.FlushAndReadAsync()

    let! context = listener.GetContextAsync() |> Async.AwaitTask

    do! Async.Sleep(60)

    do! context.Response.OutputStream.AsyncWrite([| 10uy |], 0, 1)
    context.Response.OutputStream.Close()

    do! Assert.ThrowsAnyAsync<OperationCanceledException>(fun () -> flushTask) |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Client's token is canceled before even sending data``() = asTask <| async {
    let url = sprintf "http://localhost:%d/" (Interlocked.Increment(&PortCounter))
    let transport = HttpThriftTransport(url, CancellationToken(true), dict [], Timeout.InfiniteTimeSpan)

    let listener = HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()

    let flushTask = transport.FlushAndReadAsync()

    let! isConnected = Async.AwaitIAsyncResult(listener.GetContextAsync(), 50)
    isConnected <=> false

    do! Assert.ThrowsAsync<OperationCanceledException>(fun () -> flushTask) |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Client's token is canceled after sending data but before receiving it``() = asTask <| async {
    let source = CancellationTokenSource()
    let url = sprintf "http://localhost:%d/" (Interlocked.Increment(&PortCounter))
    let transport = HttpThriftTransport(url, source.Token, dict [], Timeout.InfiniteTimeSpan)

    let listener = HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()

    transport.WriteBytes([| 42uy |], 0, 1)
    let flushTask = transport.FlushAndReadAsync()

    source.Cancel()

    let! context = listener.GetContextAsync() |> Async.AwaitTask

    let! received = context.Request.InputStream.AsyncRead(1)
    received <=> [| 42uy |]

    do! context.Response.OutputStream.AsyncWrite([| 10uy |], 0, 1)
    context.Response.OutputStream.Close()

    do! Assert.ThrowsAsync<OperationCanceledException>(fun () -> flushTask) |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Client's token is canceled after receiving data``() = asTask <| async {
    let source = CancellationTokenSource()
    let url = sprintf "http://localhost:%d/" (Interlocked.Increment(&PortCounter))
    let transport = HttpThriftTransport(url, source.Token, dict [], Timeout.InfiniteTimeSpan)

    let listener = HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()

    transport.WriteBytes([| |], 0, 0)
    let flushTask = transport.FlushAndReadAsync()

    let! context = listener.GetContextAsync() |> Async.AwaitTask

    do! context.Response.OutputStream.AsyncWrite([| 10uy |], 0, 1)

    source.Cancel()

    context.Response.OutputStream.Close()

    do! Assert.ThrowsAsync<OperationCanceledException>(fun () -> flushTask) |> Async.AwaitTask |> Async.Ignore
}