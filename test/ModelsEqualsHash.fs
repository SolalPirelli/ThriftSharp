module ThriftSharp.Tests.``Models: Equals and HashCode``

open System.Reflection
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
 

let op obj1  obj2 name =
    obj1.GetType().GetMethod(name, BindingFlags.Static ||| BindingFlags.Public).Invoke(null, [| obj1; obj2 |]) :?> bool


// Usually, asserting for equality of a condition and true is bad, but we are testing the methods themselves here

[<Theory;
  MemberData("PositiveData")>]
let ``Equal objects: Equals()``(obj: obj) =
    obj.Equals(obj) <=> true // bad assert usually, but Equals() is what we're testing here

[<Theory;
  MemberData("PositiveData")>]
let ``Equal objects: ==``(obj: obj) =
    op obj obj "op_Equality" <=> true
    
[<Theory;
  MemberData("PositiveData")>]
let ``Equal objects: !=``(obj: obj) =
    op obj obj "op_Inequality" <=> false
    
[<Theory;
  MemberData("PositiveData")>]
let ``Equal objects: GetHashCode()``(obj: obj) =
    obj.GetHashCode() <=> obj.GetHashCode()
    
[<Theory;
  MemberData("NegativeData")>]
let ``Unequal objects: Equals()``(obj1: obj, obj2: obj) =
    obj1.Equals(obj2) <=> false
    
[<Theory;
  MemberData("NegativeData")>]
let ``Unequal objects: ==``(obj1: obj, obj2: obj) =
    op obj1 obj2 "op_Equality" <=> false

[<Theory;
  MemberData("NegativeData")>]
let ``Unequal objects: !=``(obj1: obj, obj2: obj) =
    op obj1 obj2 "op_Inequality" <=> true

[<Theory;
  MemberData("PositiveData")>]
let ``Unrelated objects: Equals()``(obj: obj) =
    obj.Equals(42) <=> false