using System;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Represents an optional value.
    /// </summary>
    /// <remarks>
    /// Similar to Nullable, but works with reference types too.
    /// </remarks>
    internal sealed class Option<T>
    {
        private readonly T _value;

        /// <summary>
        /// Gets a value indicating whether the option has a value.
        /// </summary>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Gets the option value, or throws an exception if there is none.
        /// </summary>
        public T Value
        {
            get
            {
                if ( HasValue )
                {
                    return _value;
                }
                throw new InvalidOperationException( "Cannot get the value of an empty option" );
            }
        }

        /// <summary>
        /// Initializes a new instance of the Option class with no value.
        /// </summary>
        public Option() { }

        /// <summary>
        /// Initializes a new instance of the Option class with the specified value.
        /// </summary>
        public Option( T value )
        {
            _value = value;
            HasValue = true;
        }
    }

    /// <summary>
    /// Utility class to help with type inference when creating Options.
    /// </summary>
    internal static class Option
    {
        /// <summary>
        /// Returns the optional value of a field, depending on whether the first parameter is null.
        /// </summary>
        public static Option<U> Get<T, U>( T obj, Func<T, U> selector )
            where T : class
        {
            if ( obj == null )
            {
                return new Option<U>();
            }
            return new Option<U>( selector( obj ) );
        }
    }
}