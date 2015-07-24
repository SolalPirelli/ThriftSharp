// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift type IDs.
    /// </summary>
    internal enum ThriftTypeId : byte
    {
        /// <summary>
        /// Marks the lack of a field.
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Boolean.
        /// </summary>
        Boolean = 2,

        /// <summary>
        /// Signed byte.
        /// </summary>
        SByte = 3,

        /// <summary>
        /// 64-bit IEEE-754 floating-point number.
        /// </summary>
        Double = 4,

        /// <summary>
        /// 16-bit integer.
        /// </summary>
        Int16 = 6,

        /// <summary>
        /// 32-bit integer.
        /// </summary>
        Int32 = 8,

        /// <summary>
        /// 64-bit integer.
        /// </summary>
        Int64 = 10,

        /// <summary>
        /// Array of signed bytes, or UTF-8 string.
        /// </summary>
        Binary = 11,

        /// <summary>
        /// User-defined struct.
        /// </summary>
        Struct = 12,

        /// <summary>
        /// Key -> value map.
        /// </summary>
        Map = 13,

        /// <summary>
        /// Set. (list with no duplicates)
        /// </summary>
        Set = 14,

        /// <summary>
        /// List.
        /// </summary>
        List = 15
    }
}