// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift serializers.
    /// </summary>
    internal abstract class ThriftSerializer
    {
        // Order matters; FromType uses MatchesType on all, in order.
        private static readonly ThriftSerializer[] Serializers = new ThriftSerializer[]
        {
            new PrimitiveSerializer<bool>( p => p.ReadBooleanAsync, p => p.WriteBoolean , ThriftType.Bool ),
            new PrimitiveSerializer<sbyte>( p => p.ReadSByteAsync, p => p.WriteSByte , ThriftType.Byte ),
            new PrimitiveSerializer<double>( p => p.ReadDoubleAsync, p => p.WriteDouble , ThriftType.Double ),
            new PrimitiveSerializer<short>( p => p.ReadInt16Async, p => p.WriteInt16 , ThriftType.Int16 ),
            new PrimitiveSerializer<int>( p => p.ReadInt32Async, p => p.WriteInt32 , ThriftType.Int32 ),
            new PrimitiveSerializer<long>( p => p.ReadInt64Async, p => p.WriteInt64 , ThriftType.Int64 ),
            new PrimitiveSerializer<string>( p => p.ReadStringAsync, p => p.WriteString , ThriftType.String ),
            new PrimitiveSerializer<sbyte[]>( p => p.ReadBinaryAsync, p => p.WriteBinary , ThriftType.String ),
            new EnumSerializer(),
            new MapSerializer(),
            new CollectionSerializer( typeof( ISet<> ), 
                                      p => p.ReadSetHeaderAsync, p => p.ReadSetEndAsync, 
                                      p => p.WriteSetHeader, p => p.WriteSetEnd, 
                                      ThriftType.Set ),
            new CollectionSerializer( typeof( ICollection<> ), 
                                      p => p.ReadListHeaderAsync, p => p.ReadListEndAsync, 
                                      p => p.WriteListHeader, p => p.WriteListEnd, 
                                      ThriftType.List ),
            new ArraySerializer(),
            new NullableSerializer(),
            new StructSerializer()
        };

        /// <summary>
        /// Gets a Thrift type from a .NET type.
        /// </summary>
        public static ThriftSerializer FromTypeInfo( TypeInfo typeInfo )
        {
            return Serializers.First( tt => tt.MatchesType( typeInfo ) );
        }

        /// <summary>
        /// Gets the Thrift type with the specified ID.
        /// </summary>
        public static ThriftSerializer FromThriftType( ThriftType id )
        {
            return Serializers.First( s => s.GetThriftType( null ) == id );
        }


        /// <summary>
        /// Indicates whether the Thrift type matches the specified .NET TypeInfo.
        /// </summary>
        internal abstract bool MatchesType( TypeInfo typeInfo );

        /// <summary>
        /// Gets the Thrift type associated with the specified type, which may be null.
        /// The return value can be null if the argument is null.
        /// </summary>
        internal abstract ThriftType? GetThriftType( Type type );

        /// <summary>
        /// Asynchronously reads an instance of the specified .NET TypeInfo from the specified protocol.
        /// </summary>
        internal abstract Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo );

        /// <summary>
        /// Asynchronously skips an instance of the Thrift type from the specified protocol.
        /// </summary>
        internal abstract Task SkipAsync( IThriftProtocol protocol );

        /// <summary>
        /// Writes the specified object as an instance of the Thrift type to the specified protocol.
        /// </summary>
        internal abstract void Write( IThriftProtocol protocol, object obj );

        /// <summary>
        /// Reads the fields of an already created instance of the specified Thrift struct on the specified protocol.
        /// </summary>
        internal static async Task<object> ReadStructAsync( IThriftProtocol protocol, ThriftStruct st, object instance )
        {
            var readIds = new List<short>();

            await protocol.ReadStructHeaderAsync();
            while ( true )
            {
                var header = await protocol.ReadFieldHeaderAsync();
                if ( header == null )
                {
                    break;
                }

                var field = st.Fields.FirstOrDefault( f => f.Header.Id == header.Id );
                if ( field == null )
                {
                    await ThriftSerializer.FromThriftType( header.FieldType ).SkipAsync( protocol );
                }
                else
                {
                    var value = await ThriftSerializer.FromTypeInfo( field.TypeInfo )
                                                      .ReadAsync( protocol, field.TypeInfo );
                    field.Setter( instance, value );

                    readIds.Add( field.Header.Id );
                }
                await protocol.ReadFieldEndAsync();
            }
            await protocol.ReadStructEndAsync();

            foreach ( var field in st.Fields )
            {
                if ( !readIds.Contains( field.Header.Id ) )
                {
                    if ( field.IsRequired )
                    {
                        throw new ThriftProtocolException( ThriftProtocolExceptionType.MissingResult, string.Format( "Field '{0}' of type '{1}' is required but was not received.", field.Header.Name, st.Header.Name ) );
                    }
                    if ( field.DefaultValue.HasValue )
                    {
                        field.Setter( instance, field.DefaultValue.Value );
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Writes the fields of the specified instance of the specified Thrift struct on the specified protocol.
        /// </summary>
        internal static void WriteStruct( IThriftProtocol protocol, ThriftStruct st, object instance )
        {
            protocol.WriteStructHeader( st.Header );
            foreach ( var field in st.Fields )
            {
                var fieldValue = field.Getter( instance );
                if ( fieldValue != null )
                {
                    protocol.WriteFieldHeader( field.Header );
                    ThriftSerializer.FromTypeInfo( field.TypeInfo ).Write( protocol, fieldValue );
                    protocol.WriteFieldEnd();
                }
            }
            protocol.WriteFieldStop();
            protocol.WriteStructEnd();
        }


        /// <summary>
        /// A Thrift primitive parser. Super simple.
        /// </summary>
        private sealed class PrimitiveSerializer<T> : ThriftSerializer
        {
            private readonly Func<IThriftProtocol, Func<Task<T>>> _read;
            private readonly Func<IThriftProtocol, Action<T>> _write;
            private readonly ThriftType _thriftType;

            /// <summary>
            /// Creates a new instance of the PrimitiveType class from the specified reading and writing methods.
            /// </summary>
            public PrimitiveSerializer( Func<IThriftProtocol, Func<Task<T>>> read, Func<IThriftProtocol, Action<T>> write, ThriftType thriftType )
            {
                _read = read;
                _write = write;
                _thriftType = thriftType;
            }

            internal override bool MatchesType( TypeInfo typeInfo )
            {
                return typeInfo.AsType() == typeof( T );
            }

            internal override ThriftType? GetThriftType( Type type )
            {
                return _thriftType;
            }

            internal override async Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo )
            {
                return (object) await _read( protocol )();
            }

            internal override Task SkipAsync( IThriftProtocol protocol )
            {
                return _read( protocol )();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                _write( protocol )( (T) obj );
            }
        }

        /// <summary>
        /// The Thrift enum serializer, which converts Int32s to enums.
        /// </summary>
        private sealed class EnumSerializer : ThriftSerializer
        {
            internal override bool MatchesType( TypeInfo typeInfo )
            {
                return typeInfo.IsEnum;
            }

            internal override ThriftType? GetThriftType( Type type )
            {
                return ThriftType.Int32;
            }

            internal override async Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo )
            {
                var enm = ThriftAttributesParser.ParseEnum( targetTypeInfo );
                int value = await protocol.ReadInt32Async();

                var member = enm.Members.FirstOrDefault( f => f.Value == value );
                if ( member == null )
                {
                    return 0;
                }

                return member.UnderlyingField.GetValue( null );
            }

            internal override Task SkipAsync( IThriftProtocol protocol )
            {
                return protocol.ReadInt32Async();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var enm = ThriftAttributesParser.ParseEnum( obj.GetType().GetTypeInfo() );
                var member = enm.Members.FirstOrDefault( m => (int) m.UnderlyingField.GetValue( null ) == (int) obj );
                if ( member == null )
                {
                    throw new ThriftProtocolException( ThriftProtocolExceptionType.InternalError, "Cannot write an undeclared enum value." );
                }
                protocol.WriteInt32( member.Value );
            }
        }

        /// <summary>
        /// The Thrift array serializer, for lists as arrays.
        /// </summary>
        private sealed class ArraySerializer : ThriftSerializer
        {
            internal override bool MatchesType( TypeInfo typeInfo )
            {
                return typeInfo.IsArray;
            }

            internal override ThriftType? GetThriftType( Type type )
            {
                return ThriftType.List;
            }

            internal override async Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo )
            {
                var elemTypeInfo = targetTypeInfo.GetElementType().GetTypeInfo();
                var elemSerializer = ThriftSerializer.FromTypeInfo( elemTypeInfo );

                var header = await protocol.ReadListHeaderAsync();
                var array = Array.CreateInstance( elemTypeInfo.AsType(), header.Count );
                for ( int n = 0; n < array.Length; n++ )
                {
                    array.SetValue( await elemSerializer.ReadAsync( protocol, elemTypeInfo ), n );
                }
                await protocol.ReadListEndAsync();
                return array;
            }

            internal override async Task SkipAsync( IThriftProtocol protocol )
            {
                var header = await protocol.ReadListHeaderAsync();
                var ttype = ThriftSerializer.FromThriftType( header.ElementType );
                for ( int n = 0; n < header.Count; n++ )
                {
                    await ttype.SkipAsync( protocol );
                }
                await protocol.ReadListEndAsync();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var elemType = obj.GetType().GetElementType();
                var elemSerializer = ThriftSerializer.FromTypeInfo( elemType.GetTypeInfo() );

                var array = (Array) obj;
                protocol.WriteListHeader( new ThriftCollectionHeader( array.Length, elemSerializer.GetThriftType( elemType ).Value ) );
                for ( int n = 0; n < array.Length; n++ )
                {
                    elemSerializer.Write( protocol, array.GetValue( n ) );
                }
                protocol.WriteListEnd();
            }
        }

        /// <summary>
        /// The Thrift collections parameterized serializer.
        /// </summary>
        private sealed class CollectionSerializer : ThriftSerializer
        {
            private readonly Type _collectionGenericType;
            private readonly Func<IThriftProtocol, Func<Task<ThriftCollectionHeader>>> _readHeader;
            private readonly Func<IThriftProtocol, Func<Task>> _readEnd;
            private readonly Func<IThriftProtocol, Action<ThriftCollectionHeader>> _writeHeader;
            private readonly Func<IThriftProtocol, Action> _writeEnd;
            private readonly ThriftType _thriftType;

            /// <summary>
            /// Creates a new instance of the CollectionType class from the specified collection type and reading/writing methods.
            /// </summary>
            public CollectionSerializer( Type collectionGenericType,
                                         Func<IThriftProtocol, Func<Task<ThriftCollectionHeader>>> readHeader, Func<IThriftProtocol, Func<Task>> readEnd,
                                         Func<IThriftProtocol, Action<ThriftCollectionHeader>> writeHeader, Func<IThriftProtocol, Action> writeEnd,
                                         ThriftType thriftType )
            {
                _collectionGenericType = collectionGenericType;
                _readHeader = readHeader;
                _readEnd = readEnd;
                _writeHeader = writeHeader;
                _writeEnd = writeEnd;
                _thriftType = thriftType;
            }

            internal override bool MatchesType( TypeInfo typeInfo )
            {
                return !typeInfo.IsArray && typeInfo.GetGenericInterface( _collectionGenericType ) != null;
            }

            internal override ThriftType? GetThriftType( Type type )
            {
                return _thriftType;
            }

            internal override async Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo )
            {
                var elemType = targetTypeInfo.GetGenericInterface( _collectionGenericType ).GetTypeInfo().GenericTypeArguments[0];
                var elemSerializer = ThriftSerializer.FromTypeInfo( elemType.GetTypeInfo() );
                var addMethod = ReflectionEx.GetAddMethod( targetTypeInfo, _collectionGenericType );

                var inst = ReflectionEx.Create( targetTypeInfo );

                var header = await _readHeader( protocol )();
                for ( int n = 0; n < header.Count; n++ )
                {
                    addMethod.Invoke( inst, new[] { await elemSerializer.ReadAsync( protocol, elemType.GetTypeInfo() ) } );
                }
                await _readEnd( protocol )();

                return inst;
            }

            internal override async Task SkipAsync( IThriftProtocol protocol )
            {
                var header = await _readHeader( protocol )();
                var ttype = ThriftSerializer.FromThriftType( header.ElementType );
                for ( int n = 0; n < header.Count; n++ )
                {
                    await ttype.SkipAsync( protocol );
                }
                await _readEnd( protocol )();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var elemType = obj.GetType().GetTypeInfo().GetGenericInterface( _collectionGenericType ).GetTypeInfo().GenericTypeArguments[0];
                var elemSerializer = ThriftSerializer.FromTypeInfo( elemType.GetTypeInfo() );
                var arr = ( (IEnumerable) obj ).Cast<object>().ToArray();

                _writeHeader( protocol )( new ThriftCollectionHeader( arr.Length, elemSerializer.GetThriftType( elemType ).Value ) );
                foreach ( var elem in arr )
                {
                    elemSerializer.Write( protocol, elem );
                }
                _writeEnd( protocol )();
            }
        }

        /// <summary>
        /// The Thrift map serializer.
        /// </summary>
        private sealed class MapSerializer : ThriftSerializer
        {
            internal override bool MatchesType( TypeInfo typeInfo )
            {
                return typeInfo.GetGenericInterface( typeof( IDictionary<,> ) ) != null;
            }

            internal override ThriftType? GetThriftType( Type type )
            {
                return ThriftType.Map;
            }

            internal override async Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo )
            {
                var typeArgs = targetTypeInfo.GenericTypeArguments;
                var keyTypeInfo = typeArgs[0].GetTypeInfo();
                var valueTypeInfo = typeArgs[1].GetTypeInfo();
                var keySerializer = ThriftSerializer.FromTypeInfo( keyTypeInfo );
                var valueSerializer = ThriftSerializer.FromTypeInfo( valueTypeInfo );
                var addMethod = ReflectionEx.GetAddMethod( targetTypeInfo, typeof( IDictionary<,> ) );

                var inst = ReflectionEx.Create( targetTypeInfo );

                var header = await protocol.ReadMapHeaderAsync();
                for ( int n = 0; n < header.Count; n++ )
                {
                    addMethod.Invoke( inst, new[] { await keySerializer.ReadAsync( protocol, keyTypeInfo ), await valueSerializer.ReadAsync( protocol, valueTypeInfo ) } );
                }
                await protocol.ReadMapEndAsync();

                return inst;
            }

            internal override async Task SkipAsync( IThriftProtocol protocol )
            {
                var header = await protocol.ReadMapHeaderAsync();
                var keySerializer = ThriftSerializer.FromThriftType( header.KeyType );
                var valueSerializer = ThriftSerializer.FromThriftType( header.ValueType );
                for ( int n = 0; n < header.Count; n++ )
                {
                    await keySerializer.SkipAsync( protocol );
                    await valueSerializer.SkipAsync( protocol );
                }
                await protocol.ReadMapEndAsync();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var typeArgs = obj.GetType().GetTypeInfo().GenericTypeArguments;
                var keyType = typeArgs[0];
                var valueType = typeArgs[1];
                var keySerializer = ThriftSerializer.FromTypeInfo( keyType.GetTypeInfo() );
                var valueSerializer = ThriftSerializer.FromTypeInfo( valueType.GetTypeInfo() );
                var map = (IDictionary) obj;

                protocol.WriteMapHeader( new ThriftMapHeader( map.Count, keySerializer.GetThriftType( keyType ).Value, valueSerializer.GetThriftType( valueType ).Value ) );
                var enumerator = map.GetEnumerator();
                while ( enumerator.MoveNext() )
                {
                    keySerializer.Write( protocol, enumerator.Key );
                    valueSerializer.Write( protocol, enumerator.Value );
                }
                protocol.WriteMapEnd();
            }
        }

        /// <summary>
        /// The Thrift serializer for nullable value types, which converts "null" to and from an unset value.
        /// </summary>
        private sealed class NullableSerializer : ThriftSerializer
        {
            internal override bool MatchesType( TypeInfo typeInfo )
            {
                return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof( Nullable<> );
            }

            internal override ThriftType? GetThriftType( Type type )
            {
                if ( type == null )
                {
                    return null;
                }
                var wrappedType = type.GetTypeInfo().GenericTypeArguments[0];
                return ThriftSerializer.FromTypeInfo( wrappedType.GetTypeInfo() ).GetThriftType( wrappedType );
            }

            internal override async Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo )
            {
                var wrappedType = targetTypeInfo.GenericTypeArguments[0];
                var targetCtor = targetTypeInfo.DeclaredConstructors.First( c => c.GetParameters()[0].ParameterType == wrappedType );
                var value = await ThriftSerializer.FromTypeInfo( wrappedType.GetTypeInfo() )
                                                  .ReadAsync( protocol, wrappedType.GetTypeInfo() );
                return targetCtor.Invoke( new[] { value } );
            }

            internal override Task SkipAsync( IThriftProtocol protocol )
            {
                throw new InvalidOperationException( "NullableSerializer.Skip should never be called." );
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                ThriftSerializer.FromTypeInfo( obj.GetType().GetTypeInfo() ).Write( protocol, obj );
            }
        }

        /// <summary>
        /// The Thrift serializer for structs, which is the most complex one.
        /// </summary>
        private sealed class StructSerializer : ThriftSerializer
        {
            internal override bool MatchesType( TypeInfo typeInfo )
            {
                return true;
            }

            internal override ThriftType? GetThriftType( Type type )
            {
                return ThriftType.Struct;
            }

            internal override Task<object> ReadAsync( IThriftProtocol protocol, TypeInfo targetTypeInfo )
            {
                var st = ThriftAttributesParser.ParseStruct( targetTypeInfo );
                var inst = ReflectionEx.Create( targetTypeInfo );
                return ThriftSerializer.ReadStructAsync( protocol, st, inst );
            }

            internal override async Task SkipAsync( IThriftProtocol protocol )
            {
                await protocol.ReadStructHeaderAsync();
                while ( true )
                {
                    var header = await protocol.ReadFieldHeaderAsync();
                    if ( header == null )
                    {
                        break;
                    }

                    await ThriftSerializer.FromThriftType( header.FieldType ).SkipAsync( protocol );
                    await protocol.ReadFieldEndAsync();
                }
                await protocol.ReadStructEndAsync();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var st = ThriftAttributesParser.ParseStruct( obj.GetType().GetTypeInfo() );
                ThriftSerializer.WriteStruct( protocol, st, obj );
            }
        }
    }
}