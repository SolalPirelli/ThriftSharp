// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ThriftSharp.Protocols;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Writes Thrift messages from a client to a server.
    /// </summary>
    internal static class ThriftClientMessageWriter
    {
        private static class Cache
        {
            public static readonly ConstructorInfo StructHeaderConstructor =
                typeof( ThriftStructHeader ).GetTypeInfo().DeclaredConstructors.First();

            public static readonly ConstructorInfo MessageHeaderConstructor =
                typeof( ThriftMessageHeader ).GetTypeInfo().DeclaredConstructors.First();
        }

        private static Type[] EmptyTypes = new Type[0];

        // Cached compiled writers
        private static readonly Dictionary<ThriftMethod, Action<object[], IThriftProtocol>> _knownWriters =
            new Dictionary<ThriftMethod, Action<object[], IThriftProtocol>>();

        /// <summary>
        /// Creates a compiled writer for the specified method.
        /// </summary>
        private static Action<object[], IThriftProtocol> CreateCompiledWriter( ThriftMethod method )
        {
            var argsParam = Expression.Parameter( typeof( object[] ), "args" );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ), "protocol" );

            var methodContents = new List<Expression>
            {
                Expression.Call(
                    protocolParam,
                    "WriteMessageHeader",
                    EmptyTypes,
                    Expression.New(
                        Cache.MessageHeaderConstructor,
                        Expression.Constant( method.Name ),
                        Expression.Constant( method.IsOneWay ? ThriftMessageType.OneWay : ThriftMessageType.Call )
                    )
                ),

                Expression.Call(
                    protocolParam,
                    "WriteStructHeader",
                    EmptyTypes,
                    Expression.New(
                        Cache.StructHeaderConstructor,
                        Expression.Constant("")
                    )
                )
            };

            for ( int n = 0; n < method.Parameters.Count; n++ )
            {
                var getParamExpr =
                    Expression.Convert(
                        Expression.ArrayAccess(
                            argsParam,
                            Expression.Constant( n )
                        ),
                        method.Parameters[n].WireType
                    );

                methodContents.Add( ThriftStructWriter.ForField( protocolParam, method.Parameters[n], getParamExpr ) );
            }

            methodContents.Add( Expression.Call( protocolParam, "WriteFieldStop", EmptyTypes ) );
            methodContents.Add( Expression.Call( protocolParam, "WriteStructEnd", EmptyTypes ) );
            methodContents.Add( Expression.Call( protocolParam, "WriteMessageEnd", EmptyTypes ) );

            return Expression.Lambda<Action<object[], IThriftProtocol>>(
                Expression.Block( methodContents ),
                argsParam, protocolParam
            ).Compile();
        }

        /// <summary>
        /// Calls the specified ThriftMethod  with the specified arguments on the specified protocol.
        /// </summary>
        public static void Write( ThriftMethod method, object[] args, IThriftProtocol protocol )
        {
            if ( !_knownWriters.ContainsKey( method ) )
            {
                _knownWriters.Add( method, CreateCompiledWriter( method ) );
            }

            _knownWriters[method]( args, protocol );
        }
    }
}