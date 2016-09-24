// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.ComponentModel;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Interface used to build fluent APIs by hiding methods declared by <see cref="Object"/> from IntelliSense.
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