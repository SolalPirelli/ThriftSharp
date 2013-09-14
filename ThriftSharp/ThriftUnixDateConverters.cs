using System;

namespace ThriftSharp
{
    /// <summary>
    /// Converts 32-bit integer timestamps to DateTime using the Unix format, 
    /// i.e. the number of seconds since Jan. 1, 1970 at midnight.
    /// </summary>
    public sealed class ThriftUnixDateConverter : ThriftValueConverter<int, DateTime>
    {
        private readonly ThriftUnixDate64Converter _converter = new ThriftUnixDate64Converter();

        /// <summary>
        /// Converts the specified 32-bit Unix timestamp to a DateTime.
        /// </summary>
        /// <param name="value">The timestamp.</param>
        /// <returns>The resulting DateTime.</returns>
        protected internal override DateTime Convert( int value )
        {
            return _converter.Convert( value );
        }

        /// <summary>
        /// Converts the specified DateTime to a 32-bit Unix timestamp.
        /// </summary>
        /// <param name="value">The DateTime.</param>
        /// <returns>The resulting timestamp.</returns>
        protected internal override int ConvertBack( DateTime value )
        {
            return (int) _converter.ConvertBack( value );
        }
    }

    /// <summary>
    /// Converts 64-bit integer timestamps to DateTime using the Unix format, 
    /// i.e. the number of seconds since Jan. 1, 1970 at midnight.
    /// </summary>
    public sealed class ThriftUnixDate64Converter : ThriftValueConverter<long, DateTime>
    {
        // The Unix time start: Jan. 1, 1970 at midnight.
        private static readonly DateTime UnixTimeStart = new DateTime( 1970, 1, 1 );

        /// <summary>
        /// Converts the specified 64-bit Unix timestamp to a DateTime.
        /// </summary>
        /// <param name="value">The timestamp.</param>
        /// <returns>The resulting DateTime.</returns>
        protected internal override DateTime Convert( long value )
        {
            return UnixTimeStart.AddSeconds( value );
        }

        /// <summary>
        /// Converts the specified DateTime to a 64-bit Unix timestamp.
        /// </summary>
        /// <param name="value">The DateTime.</param>
        /// <returns>The resulting timestamp.</returns>
        protected internal override long ConvertBack( DateTime value )
        {
            return (long) Math.Round( ( value - UnixTimeStart ).TotalSeconds );
        }
    }
}