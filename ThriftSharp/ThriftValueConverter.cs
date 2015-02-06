// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;

namespace ThriftSharp
{
    /// <summary>
    /// Converter between types, used to serialize and deserialize complex objects into Thrift types.
    /// </summary>
    /// <remarks>
    /// Consumers should implement the <see cref="ThriftValueConverter{TFrom, TTo}" /> abstract class instead of directly inheriting from this interface.
    /// </remarks>
    public interface IThriftValueConverter
    {
        /// <summary>
        /// Gets the type from which conversions are performed.
        /// </summary>
        Type FromType { get; }

        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        object Convert( object value );

        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        object ConvertBack( object value );
    }

    /// <summary>
    /// Generic version of <see cref="IThriftValueConverter" /> that provides type-safe conversions.
    /// </summary>
    /// <typeparam name="TFrom">The type to convert from.</typeparam>
    /// <typeparam name="TTo">The type to convert to.</typeparam>
    public abstract class ThriftValueConverter<TFrom, TTo> : IThriftValueConverter
    {
        /// <summary>
        /// Gets the type from which conversions are performed.
        /// </summary>
        public Type FromType
        {
            get { return typeof( TFrom ); }
        }

        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        public object Convert( object value )
        {
            return Convert( (TFrom) value );
        }

        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        public object ConvertBack( object value )
        {
            return ConvertBack( (TTo) value );
        }

        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        protected internal abstract TTo Convert( TFrom value );

        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        protected internal abstract TFrom ConvertBack( TTo value );
    }
}