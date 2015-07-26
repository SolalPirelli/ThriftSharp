module ThriftSharp.Tests.``Models: Equals and HashCode``

open Xunit
open ThriftSharp.Models

let PositiveData =
    [ThriftCollectionHeader(42, ThriftTypeId.Boolean) :> obj
     ThriftMapHeader(5, ThriftTypeId.Boolean, ThriftTypeId.Double) :> obj
     ThriftFieldHeader(3s, "field", ThriftTypeId.Double) :> obj
     ThriftStructHeader("struct") :> obj
     ThriftMessageHeader("message", ThriftMessageType.Call) :> obj]
 |> List.map Array.singleton

let NegativeData =
    [ThriftCollectionHeader(42, ThriftTypeId.Boolean) :> obj, ThriftCollectionHeader(10, ThriftTypeId.Boolean) :> obj
     ThriftCollectionHeader(42, ThriftTypeId.Boolean) :> obj, ThriftCollectionHeader(42, ThriftTypeId.Int16) :> obj
     ThriftMapHeader(5, ThriftTypeId.Boolean, ThriftTypeId.Double) :> obj, ThriftMapHeader(7, ThriftTypeId.Boolean, ThriftTypeId.Double) :> obj
     ThriftMapHeader(5, ThriftTypeId.Boolean, ThriftTypeId.Double) :> obj, ThriftMapHeader(5, ThriftTypeId.Int64, ThriftTypeId.Double) :> obj
     ThriftMapHeader(5, ThriftTypeId.Boolean, ThriftTypeId.Double) :> obj, ThriftMapHeader(5, ThriftTypeId.Boolean, ThriftTypeId.Binary) :> obj
     ThriftFieldHeader(3s, "field", ThriftTypeId.Double) :> obj, ThriftFieldHeader(2s, "field", ThriftTypeId.Double) :> obj
     ThriftFieldHeader(3s, "field", ThriftTypeId.Double) :> obj, ThriftFieldHeader(3s, "field2", ThriftTypeId.Double) :> obj
     ThriftFieldHeader(3s, "field", ThriftTypeId.Double) :> obj, ThriftFieldHeader(3s, "field", ThriftTypeId.Struct) :> obj
     ThriftStructHeader("struct") :> obj, ThriftStructHeader("other") :> obj
     ThriftMessageHeader("message", ThriftMessageType.Call) :> obj, ThriftMessageHeader("otherMessage", ThriftMessageType.Call) :> obj
     ThriftMessageHeader("message", ThriftMessageType.Call) :> obj, ThriftMessageHeader("message", ThriftMessageType.OneWay) :> obj]
 |> List.map (fun (a, b) -> [| a; b |])

[<Theory;
  MemberData("PositiveData")>]
let ``Equal objects: Equals()``(obj: obj) =
    obj.Equals(obj) <=> true // bad assert usually, but Equals() is what we're testing here
    
[<Theory;
  MemberData("PositiveData")>]
let ``Equal objects: GetHashCode()``(obj: obj) =
    obj.GetHashCode() <=> obj.GetHashCode()

[<Theory;
  MemberData("NegativeData")>]
let ``Unequal objects: Equals()``(obj1: obj, obj2: obj) =
    obj1.Equals(obj2) <=> false

[<Theory;
  MemberData("PositiveData")>]
let ``Unrelated objects: Equals()``(obj: obj) =
    obj.Equals(42) <=> false