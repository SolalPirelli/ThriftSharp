// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.Converters

open System
open ThriftSharp

[<TestClass>]
type ``Unix date (32-bit)``() =
    let (<===>) stamp (dd, mm, yyyy, hh, MM, ss) =
        let conv = ThriftUnixDateConverter()
        let mutable date = DateTime(yyyy, mm, dd, hh, MM, ss, DateTimeKind.Utc)
        date <- date.ToLocalTime()
        conv.Convert(stamp) <=> date
        conv.ConvertBack(date) <=> stamp

    [<Test>]
    member x.``Epoch (1.1.1970 00:00:00)``() =
        0 <===> (1, 1, 1970, 00, 00, 00)

    [<Test>]
    member x.``25.08.2014 14:23:47``() =
        1408976627 <===> (25, 08, 2014, 14, 23, 47)

[<TestClass>]
type ``Unix date (64-bit)``() =
    let (<===>) stamp (dd, mm, yyyy, hh, MM, ss) =
        let conv = ThriftUnixDate64Converter()
        let mutable date = DateTime(yyyy, mm, dd, hh, MM, ss, DateTimeKind.Utc)
        date <- date.ToLocalTime()
        conv.Convert(stamp) <=> date
        conv.ConvertBack(date) <=> stamp

    [<Test>]
    member x.``Epoch (01.01.1970 00:00:00)``() =
        0L <===> (01, 01, 1970, 00, 00, 00)

    [<Test>]
    member x.``25.08.2014 14:23:47``() =
        1408976627L <===> (25, 08, 2014, 14, 23, 47)

    [<Test>]
    member x.``01.01.2050 00:00:00 (outside of 32-bit range)``() =
        2524608000L <===> (01, 01, 2050, 00, 00, 00)

[<TestClass>]
type ``Java date (64-bit)``() =
    let (<===>) stamp (dd, mm, yyyy, hh, MM, ss, ms) =
        let conv = ThriftJavaDateConverter()
        let mutable date = DateTime(yyyy, mm, dd, hh, MM, ss, ms, DateTimeKind.Utc)
        date <- date.ToLocalTime()
        conv.Convert(stamp) <=> date
        conv.ConvertBack(date) <=> stamp

    [<Test>]
    member x.``Epoch (01.01.1970 00:00:00.000)``() =
        0L <===> (01, 01, 1970, 00, 00, 00, 000)

    [<Test>]
    member x.``25.08.2014 14:23:47.123``() =
        1408976627123L <===> (25, 08, 2014, 14, 23, 47, 123)

    [<Test>]
    member x.``01.01.2050 00:00:00.000 (outside of 32-bit range)``() =
        2524608000000L <===> (01, 01, 2050, 00, 00, 00, 000)


[<TestClass>]
type ``Unix date offset (32-bit)``() =
    let (<===>) stamp (dd, mm, yyyy, hh, MM, ss) =
        let conv = ThriftUnixDateOffsetConverter()
        let mutable date = DateTimeOffset(yyyy, mm, dd, hh, MM, ss, TimeSpan.Zero)
        date <- date.ToLocalTime()
        conv.Convert(stamp) <=> date
        conv.ConvertBack(date) <=> stamp

    [<Test>]
    member x.``Epoch (1.1.1970 00:00:00)``() =
        0 <===> (1, 1, 1970, 00, 00, 00)

    [<Test>]
    member x.``25.08.2014 14:23:47``() =
        1408976627 <===> (25, 08, 2014, 14, 23, 47)

[<TestClass>]
type ``Unix date offset (64-bit)``() =
    let (<===>) stamp (dd, mm, yyyy, hh, MM, ss) =
        let conv = ThriftUnixLongDateOffsetConverter()
        let mutable date = DateTimeOffset(yyyy, mm, dd, hh, MM, ss, TimeSpan.Zero)
        date <- date.ToLocalTime()
        conv.Convert(stamp) <=> date
        conv.ConvertBack(date) <=> stamp

    [<Test>]
    member x.``Epoch (01.01.1970 00:00:00)``() =
        0L <===> (01, 01, 1970, 00, 00, 00)

    [<Test>]
    member x.``25.08.2014 14:23:47``() =
        1408976627L <===> (25, 08, 2014, 14, 23, 47)

    [<Test>]
    member x.``01.01.2050 00:00:00 (outside of 32-bit range)``() =
        2524608000L <===> (01, 01, 2050, 00, 00, 00)

[<TestClass>]
type ``Java date offset (64-bit)``() =
    let (<===>) stamp (dd, mm, yyyy, hh, MM, ss, ms) =
        let conv = ThriftJavaDateOffsetConverter()
        let mutable date = DateTimeOffset(yyyy, mm, dd, hh, MM, ss, ms, TimeSpan.Zero)
        date <- date.ToLocalTime()
        conv.Convert(stamp) <=> date
        conv.ConvertBack(date) <=> stamp

    [<Test>]
    member x.``Epoch (01.01.1970 00:00:00.000)``() =
        0L <===> (01, 01, 1970, 00, 00, 00, 000)

    [<Test>]
    member x.``25.08.2014 14:23:47.123``() =
        1408976627123L <===> (25, 08, 2014, 14, 23, 47, 123)

    [<Test>]
    member x.``01.01.2050 00:00:00.000 (outside of 32-bit range)``() =
        2524608000000L <===> (01, 01, 2050, 00, 00, 00, 000)