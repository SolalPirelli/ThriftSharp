// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System.Reflection;
using ThriftSharp.Internals;

namespace ThriftSharp
{
    /// <summary>
    /// Dynamically generates proxies to Thrift services from their definitions.
    /// </summary>
    public static class ThriftProxy
    {
        /// <summary>
        /// Creates a proxy for the specified interface type using the specified protocol.
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <param name="communication">The means of communication with the server.</param>
        /// <returns>A dynamically generated proxy to the Thrift service.</returns>
        public static T Create<T>( ThriftCommunication communication )
        {
            var service = ThriftAttributesParser.ParseService( typeof( T ).GetTypeInfo() );
            return TypeCreator.CreateImplementation<T>( m => args => Thrift.CallMethodAsync<object>( communication, service, m.Name, args ) );
        }
    }
}