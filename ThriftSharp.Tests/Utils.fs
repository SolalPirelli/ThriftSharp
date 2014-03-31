// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

[<AutoOpen>]
module ThriftSharp.Tests.Utils

open System
open System.Collections.Generic
open System.Reflection
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp
open ThriftSharp.Internals

type TestAttribute = TestMethodAttribute
type TestContainerAttribute = TestClassAttribute

let tid = LanguagePrimitives.EnumOfValue

let utcDate(dd, mm, yyyy) = DateTime(yyyy, mm, dd, 00, 00, 00, DateTimeKind.Utc)

let nullable x = System.Nullable(x)

let dict (vals: ('a * 'b) seq) =
    let dic = Dictionary()
    for (k,v) in vals do
        dic.Add(k, v)
    dic

let (<=>) (act:'a) (exp:'a) =
    if act <> exp then Assert.Fail(sprintf "Expected: %A%sActual: %A" exp Environment.NewLine act)

let (<===>) (act:'a seq) (exp:'a seq) =
    CollectionAssert.AreEqual(List(act), List(exp))

let throws<'T when 'T :> Exception> func =
    let e = ref None

    try func()
    with :? AggregateException as ex -> e := Some(ex.InnerException)
       | ex -> e := Some(ex)


    match !e with
    | None -> 
        Assert.Fail(sprintf "Expected an exception of type %A, but no exception was thrown" typeof<'T>)
        Unchecked.defaultof<'T>
    | Some ex when typeof<'T>.IsAssignableFrom(ex.GetType()) -> 
        ex :?> 'T
    | Some ex -> 
        Assert.Fail(sprintf "Expected an exception of type %A, but got one of type %A" typeof<'T> (ex.GetType()))
        Unchecked.defaultof<'T>

let throwsAsync<'T when 'T :> Exception> func = async {
    let e = ref None

    try do! func()
    with ex -> e := Some(ex.InnerException)

    match !e with
    | None -> 
        Assert.Fail(sprintf "Expected an exception of type %A, but no exception was thrown" typeof<'T>)
        return Unchecked.defaultof<'T>
    | Some ex when typeof<'T>.IsAssignableFrom(ex.GetType()) -> 
        return ex :?> 'T
    | Some ex -> 
        Assert.Fail(sprintf "Expected an exception of type %A, but got one of type %A" typeof<'T> (ex.GetType()))
        return Unchecked.defaultof<'T>
}

let run x = x |> Async.Ignore |> Async.RunSynchronously

let readAsync<'T> prot = async {
    let! retVal = ThriftSerializer.FromTypeInfo(typeof<'T>.GetTypeInfo())
                                  .ReadAsync(prot, typeof<'T>.GetTypeInfo())
               |> Async.AwaitTask
    return retVal :?> 'T
}

let write prot obj = ThriftSerializer.FromTypeInfo(obj.GetType().GetTypeInfo()).Write(prot, obj)

let readMsgAsync<'T> prot name = async {
    let svc = ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())
    return! Thrift.CallMethodAsync(ThriftCommunication(prot), svc, name, [| |]) |> Async.AwaitTask
}

let writeMsg<'T> methodName args = 
    let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                            StructHeader ""
                            FieldStop
                            StructEnd
                            MessageEnd])
    let svc = ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())
    let meth = svc.Methods |> Seq.find (fun me -> me.UnderlyingName = methodName)
    Thrift.CallMethod(m, meth, args)
    m