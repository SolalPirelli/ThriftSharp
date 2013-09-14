// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

[<AutoOpen>]
module ThriftSharp.Tests.Utils

open System
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open ThriftSharp

type TestAttribute = TestMethodAttribute

let tid = LanguagePrimitives.EnumOfValue

let (<=>) (act:'a) (exp:'a) =
    if act <> exp then Assert.Fail(sprintf "Expected: %A%sActual: %A" exp Environment.NewLine act)

let (<===>) (act:'a seq) (exp:'a seq) =
    CollectionAssert.AreEqual(List(act), List(exp))

let throws<'T> func =
    let e = ref null
    try func() |> ignore with x -> e := x

    if !e = null then
        Assert.Fail(sprintf "Expected an exception of type %A, but no exception was thrown" typeof<'T>)

    let e = !e
    if not (typeof<'T>.IsAssignableFrom(e.GetType())) then
        Assert.Fail(sprintf "Expected an exception of type %A, but got one of type %A" typeof<'T> (e.GetType()))
    box e :?> 'T