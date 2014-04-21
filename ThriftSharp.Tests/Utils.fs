// Copyright (c) 2014 Solal Pirelli
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

let throwsAsync<'T when 'T :> Exception> func = async {
    try
        // BUG: This should be asynchronous, but I can't find a way to catch all exceptions it throws...
        func() |> Async.RunSynchronously |> ignore
        Assert.Fail("Expected an exception, but none was thrown.")
        return Unchecked.defaultof<'T>
    with
    | ex -> match (match ex with :? AggregateException -> ex.InnerException | _ -> ex) with
            | ex when typeof<'T>.IsAssignableFrom(ex.GetType()) -> 
                return ex :?> 'T
            | ex -> 
                Assert.Fail(sprintf "Expected an exception of type %A, but got one of type %A (message: %s)" typeof<'T> (ex.GetType()) ex.Message)
                return Unchecked.defaultof<'T>
}

let run x = x |> Async.Ignore |> Async.RunSynchronously

let readAsync<'T> prot = async {
    let thriftStruct = ThriftAttributesParser.ParseStruct(typeof<'T>.GetTypeInfo())
    let! retVal = ThriftReader.ReadAsync(thriftStruct, prot)
               |> Async.AwaitTask
    return retVal :?> 'T
}

let write prot obj =
    let thriftStruct = ThriftAttributesParser.ParseStruct(obj.GetType().GetTypeInfo())
    ThriftWriter.Write(thriftStruct, obj, prot)

let readMsgAsync<'T> prot name = async {
    let svc = ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())
    return! Thrift.CallMethodAsync(ThriftCommunication(prot), svc, name, [| |]) |> Async.AwaitTask
}

let writeMsgAsync<'T> methodName args = async {
    let m = MemoryProtocol([MessageHeader (0, "", ThriftMessageType.Reply)
                            StructHeader ""
                            FieldStop
                            StructEnd
                            MessageEnd])
    let svc = ThriftAttributesParser.ParseService(typeof<'T>.GetTypeInfo())
    let! res = Thrift.CallMethodAsync(ThriftCommunication(m), svc, methodName, args) |> Async.AwaitTask
    return m
}