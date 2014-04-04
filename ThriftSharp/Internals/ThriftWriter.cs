// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

// TODO support enums

namespace ThriftSharp.Internals
{
    internal static class ThriftWriter
    {
        private static readonly IDictionary<ThriftStruct, dynamic> _knownWriters
            = new Dictionary<ThriftStruct, dynamic>();


        private static Expression CreateWriter( ParameterExpression protocolParam, ThriftType thriftType, Expression getter )
        {
            if ( thriftType.IsPrimitive )
            {
                string methodName = "Write" + ( thriftType.TypeInfo.AsType() == typeof( string ) ? "String" : thriftType.Id.ToString() );
                return Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( methodName ),
                                            getter );
            }

            if ( thriftType.CollectionTypeInfo != null )
            {
                string writeHeader, writeEnd;
                if ( thriftType.IsSet )
                {
                    writeHeader = "WriteSetHeader";
                    writeEnd = "WriteSetEnd";
                }
                else
                {
                    writeHeader = "WriteListHeader";
                    writeEnd = "WriteListEnd";
                }

                var countProp = thriftType.CollectionTypeInfo.GetInterfaceProperty( "Count" );

                var writeHeaderExpr =
                    Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( writeHeader ),
                                         Expression.New(
                                             TypeInfos.CollectionHeader.DeclaredConstructors.First(),
                                             Expression.Property( getter, countProp ),
                                             Expression.Constant( thriftType.ElementType )
                                         )
                    );

                var endOfLoop = Expression.Label();
                var enumerableInterface = thriftType.CollectionTypeInfo.GetGenericInterface( typeof( IEnumerable<> ) );
                var enumeratorVar = Expression.Variable( typeof( IEnumerator ), "enumerator" );
                var enumeratorAssign =
                    Expression.Assign( enumeratorVar,
                        Expression.Convert(
                            Expression.Call(
                                getter,
                                enumerableInterface.GetTypeInfo().GetDeclaredMethod( "GetEnumerator" )
                            ),
                            typeof( IEnumerator )
                        )
                    );
                var loopExpr = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsTrue(
                            Expression.Call( enumeratorVar, TypeInfos.IEnumerator.GetDeclaredMethod( "MoveNext" ) )
                        ),
                        CreateWriter( protocolParam, thriftType.ElementType,
                                          Expression.Convert(
                                              Expression.Property( enumeratorVar, TypeInfos.IEnumerator.GetDeclaredProperty( "Current" ) ),
                                              thriftType.ElementType.TypeInfo.AsType()
                                          )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                );
                var writeEndExpr = Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( writeEnd ) );

                return Expression.Block( new[] { enumeratorVar }, writeHeaderExpr, enumeratorAssign, loopExpr, writeEndExpr );
            }

            if ( thriftType.MapTypeInfo != null )
            {
                var enumerableInterface = thriftType.MapTypeInfo.GetGenericInterface( typeof( IEnumerable<> ) );
                var countProp = thriftType.MapTypeInfo.GetInterfaceProperty( "Count" );

                var pairTypeInfo = enumerableInterface.GenericTypeArguments[0].GetTypeInfo();

                var writeHeaderExpr =
                    Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "WriteMapHeader" ),
                                         Expression.New(
                                            TypeInfos.MapHeader.DeclaredConstructors.First(),
                                            Expression.Property( getter, countProp ),
                                            Expression.Constant( thriftType.KeyType ),
                                            Expression.Constant( thriftType.ValueType )
                                         )
                    );

                var endOfLoop = Expression.Label();
                var enumeratorVar = Expression.Variable( typeof( IEnumerator ), "enumerator" );
                var enumeratorCurrentExpr =
                    Expression.Convert(
                        Expression.Property( enumeratorVar, TypeInfos.IEnumerator.GetDeclaredProperty( "Current" ) ),
                        pairTypeInfo.AsType()
                    );

                var enumeratorAssign =
                    Expression.Assign( enumeratorVar,
                        Expression.Call( getter, TypeInfos.IEnumerable.GetDeclaredMethod( "GetEnumerator" ) )
                    );

                var loopExpr = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsTrue(
                            Expression.Call( enumeratorVar, TypeInfos.IEnumerator.GetDeclaredMethod( "MoveNext" ) )
                        ),
                        Expression.Block(
                            CreateWriter( protocolParam, thriftType.KeyType,
                                              Expression.Property( enumeratorCurrentExpr, pairTypeInfo.GetDeclaredProperty( "Key" ) )
                            ),
                            CreateWriter( protocolParam, thriftType.ValueType,
                                              Expression.Property( enumeratorCurrentExpr, pairTypeInfo.GetDeclaredProperty( "Value" ) )
                            )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                );
                var writeEndExpr = Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "WriteMapEnd" ) );

                return Expression.Block( new[] { enumeratorVar }, writeHeaderExpr, enumeratorAssign, loopExpr, writeEndExpr );
            }

            return Expression.Call( TypeInfos.Writer.GetDeclaredMethod( "Write2" ),
                                        getter, protocolParam
                   );
        }

        private static dynamic CreateCompiledWriter( ThriftStruct thriftStruct )
        {
            var structParam = Expression.Parameter( typeof( ThriftStruct ), "struct" );
            var valueParam = Expression.Parameter( thriftStruct.TypeInfo.AsType(), "value" );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ), "param" );

            var methodContents = new List<Expression>();

            methodContents.Add(
                Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "WriteStructHeader" ),
                                     Expression.Constant( thriftStruct.Header ) )
            );

            for ( int n = 0; n < thriftStruct.Fields.Count; n++ )
            {
                var field = thriftStruct.Fields[n];
                var fieldExpr = Expression.MakeIndex( Expression.Field( structParam, TypeInfos.Struct.GetDeclaredField( "Fields" ) ),
                                                      TypeInfos.FieldCollection.GetDeclaredProperty( "Item" ),
                                                      new[] { Expression.Constant( n ) } );
                var getFieldExpr =
                    Expression.Convert(
                        Expression.Call( fieldExpr, TypeInfos.Field.GetDeclaredMethod( "GetValue" ), valueParam ),
                        field.Header.FieldType.TypeInfo.AsType()
                    );

                var writingExpr = Expression.Block(
                    Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "WriteFieldHeader" ),
                                         Expression.Constant( field.Header ) ),
                    CreateWriter( protocolParam, field.Header.FieldType,
                                      getFieldExpr
                    ),
                    Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "WriteFieldEnd" ) )
                );

                if ( field.DefaultValue.HasValue )
                {
                    var isDefaultExpr =
                        Expression.Equal(
                            Expression.Constant( field.DefaultValue.Value ),
                            getFieldExpr
                        );

                    methodContents.Add( Expression.IfThenElse( isDefaultExpr, Expression.Empty(), writingExpr ) );
                }
                else if ( field.IsRequired && !field.Header.FieldType.TypeInfo.IsValueType )
                {
                    var isDefaultExpr =
                        Expression.Equal(
                            Expression.Constant( null ),
                            getFieldExpr
                        );

                    var exceptionExpr =
                        Expression.Throw(
                            Expression.Call( TypeInfos.SerializationError.GetDeclaredMethod( "CannotWriteNull" ),
                                                 Expression.Constant( thriftStruct.Header.Name ),
                                                 Expression.Constant( field.Header.Name )
                            )
                        );

                    methodContents.Add( Expression.IfThenElse( isDefaultExpr, exceptionExpr, writingExpr ) );
                }
                else
                {
                    methodContents.Add( writingExpr );
                }
            }

            methodContents.Add(
                Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "WriteFieldStop" ) )
            );

            methodContents.Add(
                Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "WriteStructEnd" ) )
            );

            var methodBlock = Expression.Block( methodContents );

            return Expression.Lambda( methodBlock, structParam, valueParam, protocolParam ).Compile();
        }

        public static void Write( ThriftStruct thriftStruct, object value, IThriftProtocol protocol )
        {
            if ( !_knownWriters.ContainsKey( thriftStruct ) )
            {
                _knownWriters.Add( thriftStruct, CreateCompiledWriter( thriftStruct ) );
            }

            _knownWriters[thriftStruct].DynamicInvoke( thriftStruct, value, protocol );
        }

        private static void Write2( object value, IThriftProtocol protocol )
        {
            var thriftStruct = ThriftAttributesParser.ParseStruct( value.GetType().GetTypeInfo() );
            Write( thriftStruct, value, protocol );
        }
    }
}