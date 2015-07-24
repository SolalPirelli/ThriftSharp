module ThriftSharp.Tests.``TaskEx: TimeoutAfter``

open System
open System.Threading
open System.Threading.Tasks
open Xunit
open ThriftSharp.Utilities

let MaxTimeout = TimeSpan.FromMilliseconds(40.0)

[<Fact>]
let ``Already-completed task``() = asTask <| async {
    let! result = TaskEx.TimeoutAfter(Task.FromResult(42), MaxTimeout) |> Async.AwaitTask
    result <=> 42
}    

[<Fact>]
let ``Already-faulted task``() = asTask <| async {
    let! exn = Assert.ThrowsAsync<Exception>(fun () -> 
            TaskEx.TimeoutAfter(Task.FromException<int>(Exception("Oops")), MaxTimeout) :> Task) |> Async.AwaitTask
    exn.Message <=> "Oops"
}    

[<Fact>]
let ``Already-canceled task``() = asTask <| async {
    do! Assert.ThrowsAnyAsync<OperationCanceledException>(fun () ->
            TaskEx.TimeoutAfter(Task.FromCanceled<int>(CancellationToken(true)), MaxTimeout) :> Task) |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Quick task``() = asTask <| async {
    let source = TaskCompletionSource<int>()
    let task = Task.Delay(10).ContinueWith(fun _ -> source.TrySetResult(42))
    let! result = TaskEx.TimeoutAfter(source.Task, MaxTimeout) |> Async.AwaitTask
    result <=> 42
    do! task |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Quickly faulted task``() = asTask <| async {
    let source = TaskCompletionSource<int>()
    let task = Task.Delay(10).ContinueWith(fun _ -> source.TrySetException(Exception("Oops")))
    let! exn = Assert.ThrowsAsync<Exception>(fun () -> TaskEx.TimeoutAfter(source.Task, MaxTimeout) :> Task) |> Async.AwaitTask
    exn.Message <=> "Oops"
    do! task |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Quickly canceled task``() = asTask <| async {
    let source = TaskCompletionSource<int>()
    let task = Task.Delay(10).ContinueWith(fun _ -> source.TrySetCanceled(CancellationToken(false)))
    do! Assert.ThrowsAnyAsync<OperationCanceledException>(fun () ->
            TaskEx.TimeoutAfter(source.Task, MaxTimeout) :> Task) |> Async.AwaitTask |> Async.Ignore
    do! task |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Never-completing task``() = asTask <| async {
    let source = TaskCompletionSource<int>()
    do! Assert.ThrowsAnyAsync<OperationCanceledException>(fun () ->
            TaskEx.TimeoutAfter(source.Task, MaxTimeout) :> Task) |> Async.AwaitTask |> Async.Ignore
}

[<Fact>]
let ``Very lengthy task``() = asTask <| async {
    let source = TaskCompletionSource<int>()
    Task.Delay(100).ContinueWith(fun _ -> source.TrySetResult(0)) |> ignore
    do! Assert.ThrowsAnyAsync<OperationCanceledException>(fun () ->
            TaskEx.TimeoutAfter(source.Task, MaxTimeout) :> Task) |> Async.AwaitTask |> Async.Ignore
}