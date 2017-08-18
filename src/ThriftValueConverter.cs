// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

namespace ThriftSharp
{
    /// <summary>
    /// Converter between types, used to serialize and deserialize complex objects into Thrift types.
    /// </summary>
    public interface IThriftValueConverter<TFrom, TTo>
    {
        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>A converted form of the value.</remarks>
        TTo Convert( TFrom value );

        /// <summary>
        /// Converts back the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>A converted form of the value.</remarks>
        TFrom ConvertBack( TTo value );
    }
}