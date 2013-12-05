// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.ComponentModel;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Interface that is used to build fluent interfaces by hiding methods declared by <see cref="Object"/> from IntelliSense.
    /// </summary>
    /// <remarks>
    /// See http://bit.ly/ifluentinterface for more information.
    /// </remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public interface IFluent
    {
        /// <summary>
        /// Redeclaration that hides the <see cref="Object.GetType()" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        Type GetType();

        /// <summary>
        /// Redeclaration that hides the <see cref="Object.GetHashCode()" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        int GetHashCode();

        /// <summary>
        /// Redeclaration that hides the <see cref="Object.ToString()" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        string ToString();

        /// <summary>
        /// Redeclaration that hides the <see cref="Object.Equals(object)" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        bool Equals( object obj );
    }
}