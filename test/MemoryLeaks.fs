// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

module ThriftSharp.Tests.``Memory leak tests``

open System.Reflection
open Xunit
open ThriftSharp
open ThriftSharp.Models
open ThriftSharp.Internals

[<ThriftStruct("Simple")>]
type Simple() =
    [<ThriftField(1s, true, "field")>]
    member val Field = "" with get, set

// In Debug mode, this test always fails because the GC doesn't collect local variables
#if !DEBUG
[<Fact>]
let ``[Regression] No reference is kept to returned objects``() =
    let prot = new MemoryProtocol([MessageHeader ("Test", ThriftMessageType.Reply)
                                   StructHeader ""
                                   FieldHeader (0s, "", ThriftTypeId.Struct)
                                   StructHeader "Simple"
                                   FieldHeader (1s, "field", ThriftTypeId.Binary)
                                   String "Hello"
                                   FieldEnd
                                   FieldStop
                                   StructEnd
                                   FieldEnd
                                   FieldStop
                                   StructEnd
                                   MessageEnd])
    let meth = ThriftMethod("test", false, ThriftReturnValue(typeof<Simple>.GetTypeInfo(), null), [| |], [| |])
    let resultRef = System.WeakReference(ThriftClientMessageReader.Read<Simple>(meth, prot))
    System.GC.Collect()
    resultRef.IsAlive <=> false
#endif