using System;

namespace ThriftSharp
{
    /// <summary>
    /// Converts 32-bit integer timestamps to DateTime using the Unix format, 
    /// i.e. the number of seconds since Jan. 1, 1970 at midnight.
    /// </summary>
    public sealed class ThriftUnixDateConverter : ThriftValueConverter<int, DateTime>
    {
        // The Unix time start: Jan. 1, 1970 at midnight.
        private static readonly DateTime UnixTimeStart = new DateTime( 1970, 1, 1 );

        /// <summary>
        /// Converts the specified Unix timestamp to a DateTime.
        /// </summary>
        /// <param name="value">The timestamp.</param>
        /// <returns>The resulting DateTime.</returns>
        protected override DateTime Convert( int value )
        {
            return UnixTimeStart.AddSeconds( value );
        }

        /// <summary>
        /// Converts the specified DateTime to a Unix timestamp.
        /// </summary>
        /// <param name="value">The DateTime.</param>
        /// <returns>The resulting timestamp.</returns>
        protected override int ConvertBack( DateTime value )
        {
            return (int) Math.Round( ( value - UnixTimeStart ).TotalSeconds );
        }
    }
}