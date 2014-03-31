// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

module ThriftSharp.Tests.``Serialized and deserialized``

open System.Collections.Generic
open System.Linq
open System.Reflection
open ThriftSharp
open ThriftSharp.Internals
open ThriftSharp.Protocols

[<ThriftStruct("ComplexStruct")>]
type ComplexStruct() =
    [<ThriftField(12s, true, "Bool")>]
    member val Boolean = false with get, set
    [<ThriftField(45s, false, "Sbyte")>]
    member val SByte = 0y with get, set
    [<ThriftField(23445s, false, "float")>]
    member val Double = 0.0 with get, set
    [<ThriftField(2s, false, "i16")>]
    member val Int16 = 0s with get, set
    [<ThriftField(666s, true, "I")>]
    member val Int32 = 0 with get, set
    [<ThriftField(777s, true, "long")>]
    member val Int64 = 0L with get, set
    [<ThriftField(1s, false, "s_t_r")>]
    member val String = "" with get, set
    [<ThriftField(6969s, true, "BiNaRy")>]
    member val Binary = [| 0y |] with get, set
    [<ThriftField(3s, true, "i32arr")>]
    member val Int32Array = [| 0 |] with get, set
    [<ThriftField(445s, false, "str___________________lst")>]
    member val StringList = List([ "" ]) with get, set
    [<ThriftField(343s, true, "HASHset")>]
    member val BoolSet = HashSet([ false ]) with get, set
    [<ThriftField(2222s, true, "DoubleSByteMapWhichIsVeryCoolAndSuperAwesomeSoYouShouldTotallyUseItBecauseOfHowCoolAndAwesomeItIsDontForgetAboutIt")>]
    member val DoubleSByteMap = Dictionary() with get, set

    override x.Equals(other) =
        let o = box other :?> ComplexStruct
        let eq x y = Enumerable.SequenceEqual(x,y)
        x.Boolean = o.Boolean
     && x.SByte = o.SByte
     && x.Double = o.Double
     && x.Int16 = o.Int16
     && x.Int32 = o.Int32
     && x.Int64 = o.Int64
     && x.String = o.String
     && eq x.Binary o.Binary
     && eq x.Int32Array o.Int32Array
     && eq x.StringList o.StringList
     && eq x.BoolSet o.BoolSet
     && eq x.DoubleSByteMap o.DoubleSByteMap

    override x.GetHashCode() = 0 // not used

[<TestContainer>]
type __() =
    [<Test>]
    member __.``Complex struct``() = run <| async {
        let o = ComplexStruct( Boolean = false, 
                               SByte = 1y,
                               Double = System.Double.PositiveInfinity,
                               Int16 = System.Int16.MinValue,
                               Int32 = System.Int32.MaxValue,
                               Int64 = System.Int64.MaxValue - 1L,
                               String = "ﻌ❺₤Ớᶳუٱ֍зʩǲŒâ௩რ",
                               Binary = [| 1y; 2y; 3y; 100y |],
                               Int32Array = [| |],
                               StringList = List(["abc"; "def"; "೧೨೩"]),
                               BoolSet = HashSet([ true; false ]))
        o.DoubleSByteMap.Add(1.234, 45y)
        o.DoubleSByteMap.Add(4567.0, 112y)
        o.DoubleSByteMap.Add(0.000123, 0y)

        let trans = CircularTransport()
        let prot = ThriftBinaryProtocol(trans)
        let ti = typeof<ComplexStruct>.GetTypeInfo()
        ThriftSerializer.FromTypeInfo(ti).Write(prot, o)
        let! o2 = ThriftSerializer.FromTypeInfo(ti).ReadAsync(prot, ti) |> Async.AwaitTask

        (o2 :?> ComplexStruct) <=> o
    }