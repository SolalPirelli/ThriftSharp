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
            if ( !attr.IsRequired && propertyTypeInfo.IsValueType && Nullable.GetUnderlyingType( propertyInfo.PropertyType ) == null )
            {
                throw ThriftParsingException.OptionalValueField( propertyInfo );
            }
            if ( attr.IsRequired && Nullable.GetUnderlyingType( propertyInfo.PropertyType ) != null )
            {
                throw ThriftParsingException.RequiredNullableField( propertyInfo );
            }

            return new ThriftField( attr.Id, attr.Name, attr.IsRequired, attr.DefaultValue, attr.ConverterInstance, propertyInfo );
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
        private static ThriftParameter ParseMethodParameter( ParameterInfo parameterInfo )
        {
            var attr = parameterInfo.GetCustomAttribute<ThriftParameterAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.ParameterWithoutAttribute( parameterInfo );
            }

            return new ThriftParameter( attr.Id, attr.Name, parameterInfo.ParameterType.GetTypeInfo(), attr.ConverterInstance );
        }

        /// <summary>
        /// Attempts to parse all "throws" clauses on the specified MethodInfo.
        /// </summary>
        private static ThriftThrowsClause[] ParseThrowsClauses( MethodInfo methodInfo )
        {
            var clauses = methodInfo.GetCustomAttributes<ThriftThrowsAttribute>()
                                    .Select( a => new ThriftThrowsClause( a.Id, a.Name, a.ExceptionTypeInfo, a.ConverterInstance ) )
                                    .ToArray();

            var wrongClause = clauses.FirstOrDefault( c => !c.UnderlyingTypeInfo.Extends( typeof( Exception ) ) );
            if ( wrongClause != null )
            {
                throw ThriftParsingException.NotAnException( wrongClause.UnderlyingTypeInfo, methodInfo );
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

            var unwrapped = methodInfo.ReturnType.UnwrapTask();
            if ( unwrapped == null )
            {
                throw ThriftParsingException.SynchronousMethod( methodInfo );
            }
            if ( attr.IsOneWay && unwrapped != typeof( void ) )
            {
                throw ThriftParsingException.OneWayMethodWithResult( methodInfo );
            }

            var methodParameters = methodInfo.GetParameters();
            var parameters = methodParameters.Where( p => p.ParameterType != typeof( CancellationToken ) )
                                             .Select( ParseMethodParameter )
                                             .ToArray();
            if ( methodParameters.Length - parameters.Length > 1 )
            {
                throw ThriftParsingException.MoreThanOneCancellationToken( methodInfo );
            }

            return new ThriftMethod( attr.Name, attr.IsOneWay, new ThriftReturnValue( unwrapped.GetTypeInfo(), attr.ConverterInstance ), throwsClauses, parameters );
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