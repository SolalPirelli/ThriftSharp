// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp
{
    /// <summary>
    /// Converter between types, used to serialize and deserialize complex objects into Thrift types.
    /// </summary>
    public interface IThriftValueConverter<TFrom, TTo>
    {
        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        TTo Convert( TFrom value );

        /// <summary>
        /// Converts the specified object.
        /// </summary>
        /// <param name="value">The object.</param>
        /// <remarks>A converted form of the object.</remarks>
        TFrom ConvertBack( TTo value );
    }
}