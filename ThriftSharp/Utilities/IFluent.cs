using System;
using System.ComponentModel;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Interface that is used to build fluent interfaces and hides methods declared by <see cref="object"/> from IntelliSense.
    /// </summary>
    /// <remarks>
    /// See http://bit.ly/ifluentinterface for more information.
    /// </remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public interface IFluent
    {
        /// <summary>
        /// Redeclaration that hides the <see cref="object.GetType()" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        Type GetType();

        /// <summary>
        /// Redeclaration that hides the <see cref="object.GetHashCode()" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        int GetHashCode();

        /// <summary>
        /// Redeclaration that hides the <see cref="object.ToString()" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        string ToString();

        /// <summary>
        /// Redeclaration that hides the <see cref="object.Equals(object)" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        bool Equals( object obj );
    }
}