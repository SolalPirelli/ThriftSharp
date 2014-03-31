// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Linq;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Utility class for enums.
    /// </summary>
    internal static class EnumEx
    {
        /// <summary>
        /// Gets the values of an enum in a type-safe way.
        /// </summary>
        public static T[] GetValues<T>()
            where T : struct // where T : enum is not valid C#, unfortunately
        {
            return Enum.GetValues( typeof( T ) ).Cast<T>().ToArray();
        }
    }
}