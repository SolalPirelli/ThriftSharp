// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Specific tests``

open ThriftSharp
open ThriftSharp.Internals

[<ThriftStruct("Simple")>]
type Simple() =
    [<ThriftField(1s, true, "field")>]
    member val Field = "" with get, set

[<TestClass>]
type MemoryLeakTests() =
    // Regression test
    [<Test>]
    member x.``No reference is kept to returned objects``() =
        let prot = MemoryProtocol([MessageHeader ("Test", ThriftMessageType.Reply)
                                   StructHeader ""
                                   FieldHeader (0s, "", tid 12)
                                   StructHeader "Simple"
                                   FieldHeader (1s, "field", tid 11)
                                   String "Hello"
                                   FieldEnd
                                   FieldStop
                                   StructEnd
                                   FieldEnd
                                   FieldStop
                                   StructEnd
                                   MessageEnd])
        let meth = ThriftMethod("test", typeof<Simple>, false, null, [| |], [| |], "Test")
        let resultRef = System.WeakReference(ThriftMessageReader.Read(prot, meth))
        System.GC.Collect()
        resultRef.IsAlive <=> false