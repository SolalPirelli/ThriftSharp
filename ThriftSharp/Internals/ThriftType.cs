// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Internals
{
    /// <summary>
    /// The types supported by Thrift.
    /// </summary>
    internal enum ThriftType : byte
    {
        /// <summary>
        /// Boolean.
        /// </summary>
        Bool = 2,

        /// <summary>
        /// Signed byte.
        /// </summary>
        Byte = 3,

        /// <summary>
        /// IEEE754 double-precision floating-point number.
        /// </summary>
        Double = 4,

        /// <summary>
        /// 16-bit signed integer.
        /// </summary>
        Int16 = 6,

        /// <summary>
        /// 32-bit signed integer.
        /// </summary>
        Int32 = 8,

        /// <summary>
        /// 64-bit signed integer.
        /// </summary>
        Int64 = 10,

        /// <summary>
        /// String.
        /// </summary>
        String = 11,

        /// <summary>
        /// User-defined class.
        /// </summary>
        Struct = 12,

        /// <summary>
        /// Key-value map.
        /// </summary>
        Map = 13,

        /// <summary>
        /// Unordered set.
        /// </summary>
        Set = 14,

        /// <summary>
        /// Ordered list.
        /// </summary>
        List = 15
    }
}