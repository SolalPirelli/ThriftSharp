// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Infrastructure.
    /// Do not use this type.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static class SpecialProxy
    {
        /// <summary>
        /// Infrastructure.
        /// Do not call this method.
        /// </summary>
        /// <typeparam name="T">Undocumented.</typeparam>
        /// <param name="communication">Undocumented.</param>
        /// <param name="thriftService">Undocumented.</param>
        /// <param name="methodName">Undocumented.</param>
        /// <param name="args">Undocumented.</param>
        /// <returns>Undocumented.</returns>
        [EditorBrowsable( EditorBrowsableState.Never )]
        [Obsolete( "Do not use this method.", true )]
        public static Task<T> CallMethodAsync<T>( ThriftCommunication communication, object thriftService, string methodName, object[] args )
        {
            return Thrift.CallMethodAsync<T>( communication, (ThriftService) thriftService, methodName, args );
        }
    }
}