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
    /// Reads Thrift messages sent from a server to a client.
    /// </summary>
    internal static class ThriftClientMessageReader
    {
        private static class Cache
        {
            public static readonly MethodInfo EnumIsDefinedMethod =
                typeof( Enum ).GetTypeInfo().GetDeclaredMethod( "IsDefined" );

            public static readonly ConstructorInfo ThriftProtocolExceptionConstructor =
                typeof( ThriftProtocolException ).GetTypeInfo().DeclaredConstructors.First( c => c.GetParameters().Length == 1 );

            public static readonly MethodInfo ReadStructMethod =
                typeof( ThriftStructReader ).GetTypeInfo().GetDeclaredMethod( "Read" );

            public static readonly ThriftStruct ThriftProtocolExceptionStruct =
                ThriftAttributesParser.ParseStruct( typeof( ThriftProtocolException ).GetTypeInfo() );

            public static readonly TypeInfo VoidTypeInfo =
                typeof( void ).GetTypeInfo();

            public static readonly MethodInfo DisposeMethod =
                typeof( IDisposable ).GetTypeInfo().GetDeclaredMethod( "Dispose" );
        }

        private static Type[] EmptyTypes = new Type[0];

        private static readonly Dictionary<ThriftMethod, Func<IThriftProtocol, object>> _knownReaders =
            new Dictionary<ThriftMethod, Func<IThriftProtocol, object>>();


        /// <summary>
        /// Creates a compiled reader for the specified method.
        /// </summary>
        private static Func<IThriftProtocol, object> ForMethod( ThriftMethod method )
        {
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ), "protocol" );

            var headerVariable = Expression.Variable( typeof( ThriftMessageHeader ), "header" );
            ParameterExpression hasReturnVariable = null;
            ParameterExpression returnVariable = null;

            if ( method.ReturnValue.UnderlyingTypeInfo != Cache.VoidTypeInfo )
            {
                hasReturnVariable = Expression.Variable( typeof( bool ), "hasReturn" );
                returnVariable = Expression.Variable( method.ReturnValue.UnderlyingTypeInfo.AsType(), "returnValue" );
            }

            var fieldsAndSetters = new List<Tuple<ThriftField, Func<Expression, Expression>>>();

            if ( returnVariable != null )
            {
                // Field 0 is the return value
                fieldsAndSetters.Add( Tuple.Create(
                    method.ReturnValue,
                    (Func<Expression, Expression>) ( expr => Expression.Block(
                        Expression.Assign(
                            returnVariable,
                            expr
                        ),
                        Expression.Assign(
                            hasReturnVariable,
                            Expression.Constant( true )
                        )
                    ) )
                ) );
            }

            // All other fields are declared exceptions
            foreach ( var exception in method.Exceptions )
            {
                fieldsAndSetters.Add( Tuple.Create(
                    exception,
                    new Func<Expression, Expression>( Expression.Throw )
                ) );
            }

            var statements = new List<Expression>
            {
                Expression.Assign(
                    headerVariable,
                    Expression.Call(
                        protocolParam,
                        "ReadMessageHeader",
                        EmptyTypes
                    )
                ),

                Expression.IfThen(
                    Expression.IsFalse(
                        Expression.Call(
                            Cache.EnumIsDefinedMethod,
                            Expression.Constant( typeof( ThriftMessageType ) ),
                            Expression.Convert(
                                Expression.Field(
                                    headerVariable,
                                    "MessageType"
                                ),
                                typeof( object )
                            )
                        )
                    ),
                    Expression.Throw(
                        Expression.New(
                            Cache.ThriftProtocolExceptionConstructor,
                            Expression.Constant( ThriftProtocolExceptionType.InvalidMessageType )
                        )
                    )
                ),

                Expression.IfThen(
                    Expression.Equal(
                        Expression.Field(
                            headerVariable,
                            "MessageType"
                        ),
                        Expression.Constant( ThriftMessageType.Exception )
                    ),
                    Expression.Throw(
                        Expression.Call(
                            Cache.ReadStructMethod,
                            Expression.Constant( Cache.ThriftProtocolExceptionStruct ), protocolParam
                        )
                    )
                ),

                ThriftStructReader.ForFields( protocolParam, fieldsAndSetters ),

                Expression.Call(
                    protocolParam,
                    "ReadMessageEnd",
                    EmptyTypes
                ),

                // Dispose of it now that we have finished reading and writing
                // using() is dangerous in this case because of async stuff happening
                Expression.Call(
                    protocolParam,
                    Cache.DisposeMethod
                )
            };

            if ( returnVariable != null )
            {
                statements.Add(
                    Expression.IfThen(
                        Expression.Equal(
                            hasReturnVariable,
                            Expression.Constant( false )
                        ),
                        Expression.Throw(
                            Expression.New(
                                Cache.ThriftProtocolExceptionConstructor,
                                Expression.Constant( ThriftProtocolExceptionType.MissingResult )
                            )
                        )
                    )
                );
            }

            // return value
            if ( returnVariable == null )
            {
                statements.Add( Expression.Constant( null ) );
            }
            else
            {
                statements.Add( Expression.Convert( returnVariable, typeof( object ) ) );
            }

            return Expression.Lambda<Func<IThriftProtocol, object>>(
                Expression.Block(
                    typeof( object ),
                    returnVariable == null ? new[] { headerVariable } : new[] { headerVariable, hasReturnVariable, returnVariable },
                    statements
                ),
                new[] { protocolParam }
            ).Compile();
        }

        /// <summary>
        /// Reads a ThriftMessage returned by the specified ThriftMethod on the specified ThriftProtocol.
        /// </summary>
        public static object Read( ThriftMethod method, IThriftProtocol protocol )
        {
            if ( !_knownReaders.ContainsKey( method ) )
            {
                _knownReaders.Add( method, ForMethod( method ) );
            }

            return _knownReaders[method]( protocol );
        }
    }
}