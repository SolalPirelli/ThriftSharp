using System;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;
using ThriftSharp.Transport;

namespace ThriftSharp
{
    /// <summary>
    /// Utility class to serialize and deserialize objects to Thrift binary data.
    /// </summary>
    public static class ThriftSerializer
    {
        /// <summary>
        /// Serializes the specified object to Thrift binary data.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Thrift binary data representing the object.</returns>
        public static byte[] Serialize<T>( T obj )
        {
            if( obj == null )
            {
                throw new ArgumentNullException( nameof( obj ) );
            }

            var transport = new ThriftMemoryTransport();
            var protocol = new ThriftBinaryProtocol( transport );
            ThriftStructWriter.Write( obj, protocol );
            return transport.GetInternalBuffer();
        }

        /// <summary>
        /// Deserializes the specified Thrift binary data into an object.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="bytes">The Thrift binary data representing the object.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>( byte[] bytes )
        {
            if( bytes == null )
            {
                throw new ArgumentNullException( nameof( bytes ) );
            }

            var transport = new ThriftMemoryTransport( bytes );
            var protocol = new ThriftBinaryProtocol( transport );
            return ThriftStructReader.Read<T>( protocol );
        }
    }
}