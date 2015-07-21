// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

module ThriftSharp.Tests.``Converters``

open System
open Xunit
open ThriftSharp

[<Theory>]
[<InlineData(0,           01, 01, 1970, 00, 00, 00)>]
[<InlineData(1408976627,  25, 08, 2014, 14, 23, 47)>]
let ``Unix date 32``(stamp, dd, mm, yyyy, hh, MM, ss) =
    let conv = ThriftUnixDateConverter()
    let mutable date = DateTime(yyyy, mm, dd, hh, MM, ss, DateTimeKind.Utc)
    conv.Convert(stamp) <=> date
    conv.ConvertBack(date) <=> stamp

[<Theory>]
[<InlineData(0,            01, 01, 1970, 00, 00, 00)>]
[<InlineData(1408976627L,  25, 08, 2014, 14, 23, 47)>]
[<InlineData(2524608000L,  01, 01, 2050, 00, 00, 00)>]
let ``Unix date 64``(stamp, dd, mm, yyyy, hh, MM, ss) =
    let conv = ThriftUnixDate64Converter()
    let mutable date = DateTime(yyyy, mm, dd, hh, MM, ss, DateTimeKind.Utc)
    conv.Convert(stamp) <=> date
    conv.ConvertBack(date) <=> stamp

[<Theory>]
[<InlineData(0,               01, 01, 1970, 00, 00, 00, 000)>]
[<InlineData(1408976627123L,  25, 08, 2014, 14, 23, 47, 123)>]
[<InlineData(2524608000000L,  01, 01, 2050, 00, 00, 00, 000)>]
let ``Java date``(stamp, dd, mm, yyyy, hh, MM, ss, ms) =
    let conv = ThriftJavaDateConverter()
    let mutable date = DateTime(yyyy, mm, dd, hh, MM, ss, ms, DateTimeKind.Utc)
    conv.Convert(stamp) <=> date
    conv.ConvertBack(date) <=> stamp

[<Theory>]
[<InlineData(0,           01, 01, 1970, 00, 00, 00)>]
[<InlineData(1408976627,  25, 08, 2014, 14, 23, 47)>]
let ``Unix date offset 32``(stamp, dd, mm, yyyy, hh, MM, ss) =
    let conv = ThriftUnixDateOffsetConverter()
    let mutable date = DateTimeOffset(yyyy, mm, dd, hh, MM, ss, TimeSpan.Zero)
    conv.Convert(stamp) <=> date
    conv.ConvertBack(date) <=> stamp

[<Theory>]
[<InlineData(0,            01, 01, 1970, 00, 00, 00)>]
[<InlineData(1408976627L,  25, 08, 2014, 14, 23, 47)>]
[<InlineData(2524608000L,  01, 01, 2050, 00, 00, 00)>]
let ``Unix date offset 64``(stamp, dd, mm, yyyy, hh, MM, ss) =
    let conv = ThriftUnixLongDateOffsetConverter()
    let mutable date = DateTimeOffset(yyyy, mm, dd, hh, MM, ss, TimeSpan.Zero)
    conv.Convert(stamp) <=> date
    conv.ConvertBack(date) <=> stamp

[<Theory>]
[<InlineData(0,               01, 01, 1970, 00, 00, 00, 000)>]
[<InlineData(1408976627123L,  25, 08, 2014, 14, 23, 47, 123)>]
[<InlineData(2524608000000L,  01, 01, 2050, 00, 00, 00, 000)>]
let ``Java date offset``(stamp, dd, mm, yyyy, hh, MM, ss, ms) =
    let conv = ThriftJavaDateOffsetConverter()
    let mutable date = DateTimeOffset(yyyy, mm, dd, hh, MM, ss, ms, TimeSpan.Zero)
    conv.Convert(stamp) <=> date
    conv.ConvertBack(date) <=> stamp