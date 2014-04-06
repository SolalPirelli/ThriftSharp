// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Linq.Expressions;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Utility class for validating method parameters, using expressions to avoid hardcoded parameter names.
    /// </summary>
    internal static class Validation
    {
        /// <summary>
        /// Ensures the specified object is not null.
        /// </summary>
        public static void IsNotNull<T>( T obj, Expression<Func<T>> paramGet )
            where T : class
        {
            if ( obj == null )
            {
                throw new ArgumentException( string.Format( "Parameter {0} must not be null.", GetName( paramGet ) ) );
            }
        }

        /// <summary>
        /// Ensures the specified string is not null, empty or only composed of whitespace.
        /// </summary>
        public static void IsNeitherNullNorWhitespace( string s, Expression<Func<string>> paramGet )
        {
            if ( string.IsNullOrWhiteSpace( s ) )
            {
                throw new ArgumentException( string.Format( "Parameter {0} must not be null, empty or contain only whitespace. It was '{1}'.", GetName( paramGet ), s ?? "null" ) );
            }
        }

        /// <summary>
        /// Utility method to get the name of a value returned by a Func expression, e.g. () => abc.
        /// </summary>
        private static string GetName<T>( Expression<Func<T>> paramGet )
        {
            return ( (MemberExpression) paramGet.Body ).Member.Name;
        }
    }
}