// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Parses attributes to build a Thrift interface definition.
    /// </summary>
    internal static class ThriftAttributesParser
    {
        private static readonly Dictionary<TypeInfo, ThriftStruct> _knownStructs = new Dictionary<TypeInfo, ThriftStruct>();


        /// <summary>
        /// Parses a Thrift field from the specified PropertyInfo.
        /// </summary>
        /// <remarks>
        /// If the PropertyInfo is not declared as a Thrift field, returns null.
        /// </remarks>
        private static ThriftField ParseField( PropertyInfo info )
        {
            var attr = info.GetAttribute<ThriftFieldAttribute>();
            if ( attr == null )
            {
                return null;
            }

            var fieldTypeInfo = info.PropertyType.GetTypeInfo();

            if ( fieldTypeInfo.IsValueType && !attr.IsRequired && Nullable.GetUnderlyingType( info.PropertyType ) == null )
            {
                throw ThriftParsingException.OptionalValueField( info );
            }

            var defaultValueAttr = info.GetAttribute<ThriftDefaultValueAttribute>();
            var defaultValue = defaultValueAttr == null ? new Option() : new Option( defaultValueAttr.Value );
            var converterAttr = info.GetAttribute<ThriftConverterAttribute>();

            if ( converterAttr == null )
            {
                return new ThriftField( attr.Id, attr.Name, attr.IsRequired, defaultValue,
                                        fieldTypeInfo,
                                        o => info.GetValue( o, null ),
                                        ( o, v ) => info.SetValue( o, v, null ) );
            }

            var converter = converterAttr.Converter;
            return new ThriftField( attr.Id, attr.Name, attr.IsRequired, defaultValue,
                                    converter.FromType.GetTypeInfo(),
                                    o => converter.ConvertBack( info.GetValue( o, null ) ),
                                    ( o, v ) => info.SetValue( o, converter.Convert( v ), null ) );
        }

        /// <summary>
        /// Attempts to parse a Thrift struct from the specified TypeInfo.
        /// </summary>
        public static ThriftStruct ParseStruct( TypeInfo typeInfo )
        {
            if ( !_knownStructs.ContainsKey( typeInfo ) )
            {
                if ( typeInfo.IsInterface || typeInfo.IsAbstract )
                {
                    throw ThriftParsingException.NotAConcreteType( typeInfo );
                }

                var attr = typeInfo.GetAttribute<ThriftStructAttribute>();
                if ( attr == null )
                {
                    throw ThriftParsingException.StructWithoutAttribute( typeInfo );
                }

                var fields = typeInfo.DeclaredProperties
                                     .Select( ParseField )
                                     .Where( f => f != null )
                                     .ToArray();

                if ( fields.Length == 0 )
                {
                    throw ThriftParsingException.NoFields( typeInfo );
                }

                // The type may have been added during fields parsing
                if ( !_knownStructs.ContainsKey( typeInfo ) )
                {
                    _knownStructs.Add( typeInfo, new ThriftStruct( new ThriftStructHeader( attr.Name ), fields, typeInfo ) );
                }
            }

            return _knownStructs[typeInfo];
        }


        /// <summary>
        /// Attempts to parse a Thrift method parameter from the specified ParameterInfo.
        /// </summary>
        private static ThriftMethodParameter ParseMethodParameter( ParameterInfo info )
        {
            if ( info.ParameterType == typeof( CancellationToken ) )
            {
                return null;
            }

            var attr = info.GetAttribute<ThriftParameterAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.ParameterWithoutAttribute( info );
            }

            var converterAttr = info.GetAttribute<ThriftConverterAttribute>();
            var converter = converterAttr == null ? null : converterAttr.Converter;

            return new ThriftMethodParameter( attr.Id, attr.Name, info.ParameterType.GetTypeInfo(), converter );
        }

        /// <summary>
        /// Attempts to parse all "throws" clauses on the specified MethodInfo.
        /// </summary>
        private static ThriftThrowsClause[] ParseThrowsClauses( MethodInfo info )
        {
            var clauses = info.GetAttributes<ThriftThrowsAttribute>()
                              .Select( a => new ThriftThrowsClause( a.Id, a.Name, a.ExceptionType.GetTypeInfo() ) )
                              .ToArray();

            var wrongClause = clauses.FirstOrDefault( c => !typeof( Exception ).GetTypeInfo().IsAssignableFrom( c.ExceptionTypeInfo ) );
            if ( wrongClause != null )
            {
                throw ThriftParsingException.NotAnException( wrongClause.ExceptionTypeInfo );
            }

            return clauses;
        }

        /// <summary>
        /// Parses a Thrift method from the specified MethodInfo.
        /// </summary>
        /// <remarks>
        /// If the MethodInfo is not declared as a Thrift method, returns null.
        /// </remarks>
        private static ThriftMethod ParseMethod( MethodInfo info )
        {
            var attr = info.GetAttribute<ThriftMethodAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.MethodWithoutAttribute( info );
            }

            var converterAttr = info.ReturnParameter.GetAttribute<ThriftConverterAttribute>();
            var converter = converterAttr == null ? null : converterAttr.Converter;

            var throwsClauses = ParseThrowsClauses( info );

            if ( attr.IsOneWay && throwsClauses.Length != 0 )
            {
                throw ThriftParsingException.OneWayMethodWithExceptions( info );
            }

            var parameters = info.GetParameters()
                                 .Select( ParseMethodParameter )
                                 .Where( p => p != null )
                                 .ToArray();

            var unwrapped = ReflectionEx.UnwrapTask( info.ReturnType );
            if ( unwrapped == null )
            {
                throw ThriftParsingException.SynchronousMethod( info );
            }
            if ( attr.IsOneWay && unwrapped != typeof( void ) )
            {
                throw ThriftParsingException.OneWayMethodWithResult( info );
            }

            return new ThriftMethod( attr.Name, unwrapped, attr.IsOneWay,
                                     converter, parameters, throwsClauses, info.Name );
        }

        /// <summary>
        /// Attempts to parse a Thrift service from the specified TypeInfo.
        /// </summary>
        public static ThriftService ParseService( TypeInfo typeInfo )
        {
            if ( !typeInfo.IsInterface )
            {
                throw ThriftParsingException.NotAService( typeInfo );
            }

            var attr = typeInfo.GetAttribute<ThriftServiceAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.ServiceWithoutAttribute( typeInfo );
            }

            var methods = typeInfo.DeclaredMethods
                                  .Select( ParseMethod )
                                  .ToArray();

            if ( methods.Length == 0 )
            {
                throw ThriftParsingException.NoMethods( typeInfo );
            }

            return new ThriftService( attr.Name, methods );
        }
    }
}