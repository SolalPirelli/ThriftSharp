// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Linq;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Parses attributes to build a Thrift interface definition.
    /// </summary>
    internal static class ThriftAttributesParser
    {
        /// <summary>
        /// Parses a Thrift enum member from the specified FieldInfo.
        /// </summary>
        private static ThriftEnumMember ParseEnumMember( FieldInfo info )
        {
            var attr = info.GetAttribute<ThriftEnumMemberAttribute>();
            if ( attr == null )
            {
                return new ThriftEnumMember( info.Name, info.GetEnumMemberValue(), info );
            }
            return new ThriftEnumMember( attr.Name, attr.Value, info );
        }

        /// <summary>
        /// Attempts to parse a Thrift enum from the specified TypeInfo.
        /// </summary>
        public static ThriftEnum ParseEnum( TypeInfo typeInfo )
        {
            if ( !typeInfo.IsEnum )
            {
                throw ThriftParsingException.NotAnEnum( typeInfo );
            }

            var attr = typeInfo.GetAttribute<ThriftEnumAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.EnumWithoutAttribute( typeInfo );
            }

            var members = typeInfo.DeclaredFields
                                  .Where( f => f.IsStatic )
                                  .Select( ParseEnumMember )
                                  .ToArray();
            return new ThriftEnum( attr.Name, members );
        }


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

            var defaultValAttr = info.GetAttribute<ThriftDefaultValueAttribute>();
            var defaultValue = Option.Get( defaultValAttr, a => a.Value );

            var converterAttr = info.GetAttribute<ThriftConverterAttribute>();
            if ( converterAttr == null )
            {
                return new ThriftField( attr.Id, attr.Name, attr.IsRequired, defaultValue, info.PropertyType.GetTypeInfo(),
                                        o => info.GetValue( o, null ),
                                        ( o, v ) => info.SetValue( o, v, null ) );
            }
            else
            {
                var converter = converterAttr.Converter;
                return new ThriftField( attr.Id, attr.Name, attr.IsRequired, defaultValue, converter.FromType.GetTypeInfo(),
                                        o => converter.ConvertBack( info.GetValue( o, null ) ),
                                        ( o, v ) => info.SetValue( o, converter.Convert( v ), null ) );
            }
        }

        /// <summary>
        /// Attempts to parse a Thrift struct from the specified TypeInfo.
        /// </summary>
        public static ThriftStruct ParseStruct( TypeInfo typeInfo )
        {
            if ( !typeInfo.IsClass && !typeInfo.IsValueType )
            {
                throw ThriftParsingException.NotAStruct( typeInfo );
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
            return new ThriftStruct( new ThriftStructHeader( attr.Name ), fields );
        }


        /// <summary>
        /// Attempts to parse a Thrift method parameter from the specified ParameterInfo.
        /// </summary>
        private static ThriftMethodParameter ParseMethodParameter( ParameterInfo info )
        {
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
                return null;
            }

            var converterAttr = info.GetAttribute<ThriftConverterAttribute>();
            var converter = converterAttr == null ? null : converterAttr.Converter;

            var throwsClauses = ParseThrowsClauses( info );
            var parameters = info.GetParameters()
                                 .Select( ParseMethodParameter )
                                 .ToArray();

            var unwrapped = ReflectionEx.UnwrapTaskIfNeeded( info.ReturnType );
            if ( unwrapped == null )
            {
                throw ThriftParsingException.NotAsync( info );
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
            return new ThriftService( attr.Name, methods );
        }
    }
}