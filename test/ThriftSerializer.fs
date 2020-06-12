module ThriftSharp.Tests.``Thrift serializer``

open System
open Xunit
open ThriftSharp

[<ThriftStruct("SimpleStruct")>]
type SimpleStruct() =
    [<ThriftField(1s, true, "Field")>]
    member val Field = 0 with get, set

[<Fact>]
let ``Roundtrip``() =
    let obj = SimpleStruct(Field = 42)
    let arr = ThriftSerializer.Serialize(obj)
    let bytes = arr.ToArray();
    let obj2 = ThriftSerializer.Deserialize<SimpleStruct>(bytes)
    obj.Field <=> obj2.Field
    bytes.Length <=> 8

[<Fact>]
let ``Serializing a null object fails``() =
    Assert.Throws<ArgumentNullException>(fun () -> ThriftSerializer.Serialize(null) |> ignore) |> ignore

[<Fact>]
let ``Deserializing null bytes fails``() =
    Assert.Throws<ArgumentNullException>(fun () -> ThriftSerializer.Deserialize(null) |> ignore) |> ignore