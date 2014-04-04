// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Internals
{
    internal enum ThriftTypeId : byte
    {
        Boolean = 2,
        SByte = 3,
        Double = 4,
        Int16 = 6,
        Int32 = 8,
        Int64 = 10,
        Binary = 11,
        Struct = 12,
        Map = 13,
        Set = 14,
        List = 15
    }
}