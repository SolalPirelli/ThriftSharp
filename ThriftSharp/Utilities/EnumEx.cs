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