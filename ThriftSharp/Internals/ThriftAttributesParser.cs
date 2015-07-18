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
        /// If the PropertyInfo is not declared as a Thrift field, returns null.
        /// </summary>
        private static ThriftField ParseField( PropertyInfo propertyInfo )
        {
            var attr = propertyInfo.GetCustomAttribute<ThriftFieldAttribute>();
            if ( attr == null )
            {
                return null;
            }

            var propertyTypeInfo = propertyInfo.PropertyType.GetTypeInfo();
            if ( propertyTypeInfo.IsValueType && !attr.IsRequired && Nullable.GetUnderlyingType( propertyInfo.PropertyType ) == null )
            {
                throw ThriftParsingException.OptionalValueField( propertyInfo );
            }

            var defaultValueAttr = propertyInfo.GetCustomAttribute<ThriftDefaultValueAttribute>();
            var defaultValue = defaultValueAttr == null ? null : defaultValueAttr.Value;

            var converterAttr = propertyInfo.GetCustomAttribute<ThriftConverterAttribute>();
            var converter = converterAttr == null ? null : converterAttr.Converter;

            return ThriftField.Field( attr.Id, attr.Name, attr.IsRequired, defaultValue, converter, propertyInfo );
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

                var attr = typeInfo.GetCustomAttribute<ThriftStructAttribute>();
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
        private static ThriftField ParseMethodParameter( ParameterInfo parameterInfo )
        {
            if ( parameterInfo.ParameterType == typeof( CancellationToken ) )
            {
                return null;
            }

            var attr = parameterInfo.GetCustomAttribute<ThriftParameterAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.ParameterWithoutAttribute( parameterInfo );
            }

            var converterAttr = parameterInfo.GetCustomAttribute<ThriftConverterAttribute>();
            var converter = converterAttr == null ? null : converterAttr.Converter;

            return ThriftField.Parameter( attr.Id, attr.Name, parameterInfo.ParameterType.GetTypeInfo(), converter );
        }

        /// <summary>
        /// Attempts to parse all "throws" clauses on the specified MethodInfo.
        /// </summary>
        private static ThriftField[] ParseThrowsClauses( MethodInfo methodInfo )
        {
            var clauses = methodInfo.GetCustomAttributes<ThriftThrowsAttribute>()
                              .Select( a => ThriftField.ThrowsClause( a.Id, a.Name, a.ExceptionTypeInfo ) )
                              .ToArray();

            var wrongClause = clauses.FirstOrDefault( c => !c.TypeInfo.Extends( typeof( Exception ) ) );
            if ( wrongClause != null )
            {
                throw ThriftParsingException.NotAnException( wrongClause.TypeInfo, methodInfo );
            }

            return clauses;
        }

        /// <summary>
        /// Parses a Thrift method from the specified MethodInfo.
        /// If the MethodInfo is not declared as a Thrift method, returns null.
        /// </summary>
        private static ThriftMethod ParseMethod( MethodInfo methodInfo )
        {
            var attr = methodInfo.GetCustomAttribute<ThriftMethodAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.MethodWithoutAttribute( methodInfo );
            }

            var throwsClauses = ParseThrowsClauses( methodInfo );
            if ( attr.IsOneWay && throwsClauses.Length != 0 )
            {
                throw ThriftParsingException.OneWayMethodWithExceptions( methodInfo );
            }

            var unwrapped = ReflectionExtensions.UnwrapTaskType( methodInfo.ReturnType );
            if ( unwrapped == null )
            {
                throw ThriftParsingException.SynchronousMethod( methodInfo );
            }
            if ( attr.IsOneWay && unwrapped != typeof( void ) )
            {
                throw ThriftParsingException.OneWayMethodWithResult( methodInfo );
            }

            var parameters = methodInfo.GetParameters()
                                 .Select( ParseMethodParameter )
                                 .Where( p => p != null )
                                 .ToArray();

            var converterAttr = methodInfo.ReturnParameter.GetCustomAttribute<ThriftConverterAttribute>();
            var converter = converterAttr == null ? null : converterAttr.Converter;


            return new ThriftMethod( attr.Name, attr.IsOneWay,
                                     ThriftField.ReturnValue( unwrapped.GetTypeInfo(), converter ),
                                     parameters,
                                     throwsClauses );
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

            var attr = typeInfo.GetCustomAttribute<ThriftServiceAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.ServiceWithoutAttribute( typeInfo );
            }

            var methods = typeInfo.DeclaredMethods.ToDictionary( m => m.Name, ParseMethod );
            if ( methods.Count == 0 )
            {
                throw ThriftParsingException.NoMethods( typeInfo );
            }

            return new ThriftService( attr.Name, methods );
        }
    }
}