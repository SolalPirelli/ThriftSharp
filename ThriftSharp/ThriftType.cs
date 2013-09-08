using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThriftSharp.Models;
using ThriftSharp.Protocols;
using ThriftSharp.Reflection;

namespace ThriftSharp
{
    /// <summary>
    /// Thrift types.
    /// </summary>
    public abstract class ThriftType
    {
        // Order matters; FromType uses MatchesType on all, in order.
        private static readonly Dictionary<ThriftType, ThriftTypeInfo> Types = new Dictionary<ThriftType, ThriftTypeInfo>
        {
            { new PrimitiveType<bool>( p => p.ReadBoolean, p => p.WriteBoolean ), new ThriftTypeInfo( "Bool", 2 ) },
            { new PrimitiveType<sbyte>( p => p.ReadSByte, p => p.WriteSByte ), new ThriftTypeInfo( "Byte", 3 ) },
            { new PrimitiveType<double>( p => p.ReadDouble, p => p.WriteDouble ), new ThriftTypeInfo( "Double", 4 ) },
            { new PrimitiveType<short>( p => p.ReadInt16, p => p.WriteInt16 ), new ThriftTypeInfo( "Int16", 6 ) },
            { new PrimitiveType<int>( p => p.ReadInt32, p => p.WriteInt32 ), new ThriftTypeInfo( "Int32", 8 ) },
            { new PrimitiveType<long>( p => p.ReadInt64, p => p.WriteInt64 ), new ThriftTypeInfo( "Int64", 10 ) },
            { new PrimitiveType<string>( p => p.ReadString, p => p.WriteString ), new ThriftTypeInfo( "String", 11 ) },
            { new PrimitiveType<sbyte[]>( p => p.ReadBinary, p => p.WriteBinary ), new ThriftTypeInfo( "Binary (String)", 11 ) },
            { new EnumType(), new ThriftTypeInfo( "Enum (Int32)", 8) },
            { new ArrayType(), new ThriftTypeInfo( "Array (List)", 15 ) },
            { new MapType(), new ThriftTypeInfo( "Map", 13 ) },
            { new CollectionType( typeof( ISet<> ), 
                                  p => p.ReadSetHeader, p => p.ReadSetEnd, 
                                  p => p.WriteSetHeader, p => p.WriteSetEnd ), 
                                new ThriftTypeInfo( "Set", 14 ) },
            { new CollectionType( typeof( ICollection<> ), 
                                  p => p.ReadListHeader, p => p.ReadListEnd, 
                                  p => p.WriteListHeader, p => p.WriteListEnd ), 
                                new ThriftTypeInfo( "List", 15 ) },
            { new StructType(), new ThriftTypeInfo( "Struct", 12 ) }
        };

        /// <summary>
        /// Statically initializes the ThriftType class.
        /// </summary>
        static ThriftType()
        {
            Struct = ThriftType.FromId( 12 );
        }

        /// <summary>
        /// Gets the type's ID.
        /// </summary>
        public byte Id
        {
            get { return Types[this].Id; }
        }

        /// <summary>
        /// Gets a string representation of the type.
        /// </summary>
        public override string ToString()
        {
            return Types[this].Name;
        }

        /// <summary>
        /// Gets a Thrift type from a .NET type.
        /// </summary>
        /// <param name="type">The .NET type.</param>
        /// <returns>The Thrift type corresponding to the .NET type.</returns>
        public static ThriftType FromType( Type type )
        {
            return Types.Keys.First( tt => tt.MatchesType( type ) );
        }

        /// <summary>
        /// Gets the Thrift type with the specified ID.
        /// </summary>
        /// <param name="id">The Thrift type ID.</param>
        /// <returns>The Thrift type with the specified ID.</returns>
        public static ThriftType FromId( byte id )
        {
            return Types.First( p => p.Value.Id == id ).Key;
        }


        /// <summary>
        /// The "Struct" Thrift type.
        /// </summary>
        internal static ThriftType Struct { get; private set; }

        /// <summary>
        /// Indicates whether the Thrift type matches the specified .NET type.
        /// </summary>
        internal abstract bool MatchesType( Type type );

        /// <summary>
        /// Reads an instance of the specified .NET type from the specified protocol.
        /// </summary>
        internal abstract object Read( IThriftProtocol protocol, Type targetType );

        /// <summary>
        /// Skips an instance of the Thrift type from the specified protocol.
        /// </summary>
        internal abstract void Skip( IThriftProtocol protocol );

        /// <summary>
        /// Writes the specified object as an instance of the Thrift type to the specified protocol.
        /// </summary>
        internal abstract void Write( IThriftProtocol protocol, object obj );

        /// <summary>
        /// Reads the fields of an already created instance of the specified Thrift struct on the specified protocol.
        /// </summary>
        internal static object ReadStruct( IThriftProtocol protocol, ThriftStruct st, object instance )
        {
            var readIds = new List<short>();

            protocol.ReadStructHeader();
            while ( true )
            {
                var header = protocol.ReadFieldHeader();
                if ( header == null )
                {
                    break;
                }

                var field = st.Fields.FirstOrDefault( f => f.Header.Id == header.Id );
                if ( field == null )
                {
                    header.FieldType.Skip( protocol );
                }
                else
                {
                    var value = field.Header.FieldType.Read( protocol, field.UnderlyingType );
                    field.Setter( instance, value );

                    readIds.Add( field.Header.Id );
                }
                protocol.ReadFieldEnd();
            }
            protocol.ReadStructEnd();

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
                var value = field.Getter( instance );
                bool isDefault = !field.IsRequired && field.DefaultValue.HasValue && value == field.DefaultValue.Value;
                if ( !isDefault )
                {
                    protocol.WriteFieldHeader( field.Header );
                    field.Header.FieldType.Write( protocol, value );
                    protocol.WriteFieldEnd();
                }
            }
            protocol.WriteFieldStop();
            protocol.WriteStructEnd();
        }


        /// <summary>
        /// Contains information about a Thrift type.
        /// </summary>
        private sealed class ThriftTypeInfo
        {
            /// <summary>
            /// Gets the Thrift type's name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the Thrift type's ID.
            /// </summary>
            public byte Id { get; private set; }


            /// <summary>
            /// Initializes an instance of the ThriftTypeInfo class with the specified name and ID.
            /// </summary>
            /// <param name="name">The Thrift type name.</param>
            /// <param name="id">The Thrift type ID.</param>
            public ThriftTypeInfo( string name, byte id )
            {
                Name = name;
                Id = id;
            }
        }

        /// <summary>
        /// A Thrift primitive type.
        /// </summary>
        private sealed class PrimitiveType<T> : ThriftType
        {
            private readonly Func<IThriftProtocol, Func<T>> _read;
            private readonly Func<IThriftProtocol, Action<T>> _write;

            /// <summary>
            /// Creates a new instance of the PrimitiveType class from the specified reading and writing methods.
            /// </summary>
            public PrimitiveType( Func<IThriftProtocol, Func<T>> read, Func<IThriftProtocol, Action<T>> write )
            {
                _read = read;
                _write = write;
            }

            internal override bool MatchesType( Type type )
            {
                return type == typeof( T );
            }

            internal override object Read( IThriftProtocol protocol, Type targetType )
            {
                return _read( protocol )();
            }

            internal override void Skip( IThriftProtocol protocol )
            {
                _read( protocol )();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                _write( protocol )( (T) obj );
            }
        }

        /// <summary>
        /// The Thrift enum type, which is actually an Int32 in disguise.
        /// </summary>
        private sealed class EnumType : ThriftType
        {
            internal override bool MatchesType( Type type )
            {
                return type.IsEnum;
            }

            internal override object Read( IThriftProtocol protocol, Type targetType )
            {
                var enm = ThriftAttributesParser.ParseEnum( targetType );
                int value = protocol.ReadInt32();

                var member = enm.Members.FirstOrDefault( f => f.Value == value );
                if ( member == null )
                {
                    return 0;
                }

                return member.UnderlyingField.GetValue( null );
            }

            internal override void Skip( IThriftProtocol protocol )
            {
                protocol.ReadInt32();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var enm = ThriftAttributesParser.ParseEnum( obj.GetType() );
                var member = enm.Members.FirstOrDefault( m => (int) m.UnderlyingField.GetValue( null ) == (int) obj );
                if ( member == null )
                {
                    throw new ThriftProtocolException( ThriftProtocolExceptionType.InternalError, "Cannot write an undeclared enum value." );
                }
                protocol.WriteInt32( member.Value );
            }
        }

        /// <summary>
        /// The Thrift array type, which is actually a list in disguise.
        /// </summary>
        private sealed class ArrayType : ThriftType
        {
            internal override bool MatchesType( Type type )
            {
                return type.IsArray;
            }

            internal override object Read( IThriftProtocol protocol, Type targetType )
            {
                var elemType = targetType.GetElementType();
                var elemTType = ThriftType.FromType( elemType );

                var header = protocol.ReadListHeader();
                var array = Array.CreateInstance( elemType, header.Count );
                for ( int n = 0; n < array.Length; n++ )
                {
                    array.SetValue( elemTType.Read( protocol, elemType ), n );
                }
                protocol.ReadListEnd();
                return array;
            }

            internal override void Skip( IThriftProtocol protocol )
            {
                var header = protocol.ReadListHeader();
                for ( int n = 0; n < header.Count; n++ )
                {
                    header.ElementType.Skip( protocol );
                }
                protocol.ReadListEnd();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var elemType = obj.GetType().GetElementType();
                var elemTType = ThriftType.FromType( elemType );

                var array = (Array) obj;
                protocol.WriteListHeader( new ThriftCollectionHeader( array.Length, elemTType ) );
                for ( int n = 0; n < array.Length; n++ )
                {
                    elemTType.Write( protocol, array.GetValue( n ) );
                }
                protocol.WriteListEnd();
            }
        }

        /// <summary>
        /// A Thrift collection type, containing one kind of element.
        /// </summary>
        private sealed class CollectionType : ThriftType
        {
            private readonly Type _collectionGenericType;
            private readonly Func<IThriftProtocol, Func<ThriftCollectionHeader>> _readHeader;
            private readonly Func<IThriftProtocol, Action> _readEnd;
            private readonly Func<IThriftProtocol, Action<ThriftCollectionHeader>> _writeHeader;
            private readonly Func<IThriftProtocol, Action> _writeEnd;

            /// <summary>
            /// Creates a new instance of the CollectionType class from the specified collection type and reading/writing methods.
            /// </summary>
            public CollectionType( Type collectionGenericType,
                                   Func<IThriftProtocol, Func<ThriftCollectionHeader>> readHeader, Func<IThriftProtocol, Action> readEnd,
                                   Func<IThriftProtocol, Action<ThriftCollectionHeader>> writeHeader, Func<IThriftProtocol, Action> writeEnd )
            {
                _collectionGenericType = collectionGenericType;
                _readHeader = readHeader;
                _readEnd = readEnd;
                _writeHeader = writeHeader;
                _writeEnd = writeEnd;
            }

            internal override bool MatchesType( Type type )
            {
                return !type.IsArray && type.GetGenericInterface( _collectionGenericType ) != null;
            }

            internal override object Read( IThriftProtocol protocol, Type targetType )
            {
                var elemType = targetType.GetGenericArguments()[0];
                var elemTType = ThriftType.FromType( elemType );
                var addMethod = ReflectionExtensions.GetAddMethod( targetType, _collectionGenericType );

                var inst = Activator.CreateInstance( targetType );

                var header = _readHeader( protocol )();
                for ( int n = 0; n < header.Count; n++ )
                {
                    addMethod.Invoke( inst, new[] { elemTType.Read( protocol, elemType ) } );
                }
                _readEnd( protocol )();

                return inst;
            }

            internal override void Skip( IThriftProtocol protocol )
            {
                var header = _readHeader( protocol )();
                for ( int n = 0; n < header.Count; n++ )
                {
                    header.ElementType.Skip( protocol );
                }
                _readEnd( protocol )();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var elemType = obj.GetType().GetGenericInterface( _collectionGenericType ).GetGenericArguments()[0];
                var elemTType = ThriftType.FromType( elemType );
                var arr = ( (IEnumerable) obj ).Cast<object>().ToArray();

                _writeHeader( protocol )( new ThriftCollectionHeader( arr.Length, elemTType ) );
                foreach ( var elem in arr )
                {
                    elemTType.Write( protocol, elem );
                }
                _writeEnd( protocol )();
            }
        }

        /// <summary>
        /// The Thrift map type, which is a map of two kinds of elements.
        /// </summary>
        private sealed class MapType : ThriftType
        {
            internal override bool MatchesType( Type type )
            {
                return type.GetGenericInterface( typeof( IDictionary<,> ) ) != null;
            }

            internal override object Read( IThriftProtocol protocol, Type targetType )
            {
                var typeArgs = targetType.GetGenericArguments();
                var keyType = typeArgs[0];
                var valType = typeArgs[1];
                var keyTType = ThriftType.FromType( keyType );
                var valTType = ThriftType.FromType( valType );
                var addMethod = ReflectionExtensions.GetAddMethod( targetType, typeof( IDictionary<,> ) );

                var inst = Activator.CreateInstance( targetType );

                var header = protocol.ReadMapHeader();
                for ( int n = 0; n < header.Count; n++ )
                {
                    addMethod.Invoke( inst, new[] { keyTType.Read( protocol, keyType ), valTType.Read( protocol, valType ) } );
                }
                protocol.ReadMapEnd();

                return inst;
            }

            internal override void Skip( IThriftProtocol protocol )
            {
                var header = protocol.ReadMapHeader();
                for ( int n = 0; n < header.Count; n++ )
                {
                    header.KeyType.Skip( protocol );
                    header.ValueType.Skip( protocol );
                }
                protocol.ReadMapEnd();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var typeArgs = obj.GetType().GetGenericArguments();
                var keyType = typeArgs[0];
                var valType = typeArgs[1];
                var keyTType = ThriftType.FromType( keyType );
                var valTType = ThriftType.FromType( valType );
                var map = (IDictionary) obj;

                protocol.WriteMapHeader( new ThriftMapHeader( map.Count, keyTType, valTType ) );
                var enumerator = map.GetEnumerator();
                while ( enumerator.MoveNext() )
                {
                    keyTType.Write( protocol, enumerator.Key );
                    valTType.Write( protocol, enumerator.Value );
                }
                protocol.WriteMapEnd();
            }
        }

        /// <summary>
        /// The Thrift struct types, containing fields.
        /// </summary>
        private sealed class StructType : ThriftType
        {
            internal override bool MatchesType( Type type )
            {
                return true;
            }

            internal override object Read( IThriftProtocol protocol, Type targetType )
            {
                var st = ThriftAttributesParser.ParseStruct( targetType );
                var inst = Activator.CreateInstance( targetType );
                return ThriftType.ReadStruct( protocol, st, inst );
            }

            internal override void Skip( IThriftProtocol protocol )
            {
                protocol.ReadStructHeader();
                while ( true )
                {
                    var header = protocol.ReadFieldHeader();
                    if ( header == null )
                    {
                        break;
                    }

                    header.FieldType.Skip( protocol );
                }
                protocol.ReadStructEnd();
            }

            internal override void Write( IThriftProtocol protocol, object obj )
            {
                var st = ThriftAttributesParser.ParseStruct( obj.GetType() );
                ThriftType.WriteStruct( protocol, st, obj );
            }
        }
    }
}