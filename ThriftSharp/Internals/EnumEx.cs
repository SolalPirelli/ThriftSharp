using System;
using System.Linq;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Utility class for enums.
    /// </summary>
    public static class EnumEx
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