// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Utility class for validating method parameters.
    /// </summary>
    internal static class Validation
    {
        /// <summary>
        /// Ensures the specified object is not null.
        /// </summary>
        public static void IsNotNull<T>( T obj, string parameterName )
            where T : class
        {
            if( obj == null )
            {
                throw new ArgumentNullException( $"Parameter {parameterName} must not be null." );
            }
        }

        /// <summary>
        /// Ensures the specified string is not null, empty or entirely composed of whitespace.
        /// </summary>
        public static void IsNeitherNullNorWhitespace( string s, string parameterName )
        {
            if( string.IsNullOrWhiteSpace( s ) )
            {
                throw new ArgumentException( $"Parameter {parameterName} must not be null, empty or contain only whitespace. It was '{s ?? "null"}'." );
            }
        }
    }
}
