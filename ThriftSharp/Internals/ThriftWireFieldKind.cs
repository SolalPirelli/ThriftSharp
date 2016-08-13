// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Possible kinds for a wire field.
    /// </summary>
    internal enum ThriftWireFieldState
    {
        /// <summary>
        /// The field is guaranteed to always exist.
        /// </summary>
        AlwaysPresent,

        /// <summary>
        /// The field is required, but its existence must be validated.
        /// </summary>
        Required,

        /// <summary>
        /// The field is not required.
        /// </summary>
        Optional
    }
}