using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Utility class for lists.
    /// </summary>
    internal static class ListExtensions
    {
        /// <summary>
        /// Creates a read-only copy (i.e. immutable version) of the specified list.
        /// </summary>
        public static ReadOnlyCollection<T> CopyAsReadOnly<T>( this IList<T> list )
        {
            T[] arr = new T[list.Count];
            list.CopyTo( arr, 0 );
            return new ReadOnlyCollection<T>( arr );
        }
    }
}