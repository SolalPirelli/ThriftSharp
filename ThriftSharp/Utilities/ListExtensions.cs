// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThriftSharp.Utilities
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