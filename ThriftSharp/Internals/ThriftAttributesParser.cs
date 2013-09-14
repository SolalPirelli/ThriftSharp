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
        /// Attempts to parse a Thrift enum from the specified type.
        /// </summary>
        public static ThriftEnum ParseEnum( Type type )
        {
            if ( !type.IsEnum )
            {
                throw ThriftParsingException.NotAnEnum( type );
            }

            var attr = type.GetAttribute<ThriftEnumAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.EnumWithoutAttribute( type );
            }

            var members = type.GetFields( BindingFlags.Public | BindingFlags.Static )
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
            var defaultVal = Option.Get( defaultValAttr, a => a.Value );

            var header = new ThriftFieldHeader( attr.Id, attr.Name, ThriftSerializer.FromType( info.PropertyType ).ThriftType );
            return new ThriftField( header, attr.IsRequired, defaultVal, info.PropertyType, o => info.GetValue( o, null ), ( o, v ) => info.SetValue( o, v, null ) );
        }

        /// <summary>
        /// Attempts to parse a Thrift struct from the specified Type.
        /// </summary>
        public static ThriftStruct ParseStruct( Type type )
        {
            if ( !type.IsClass && !type.IsValueType )
            {
                throw ThriftParsingException.NotAStruct( type );
            }

            var attr = type.GetAttribute<ThriftStructAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.StructWithoutAttribute( type );
            }

            var fields = type.GetProperties( BindingFlags.Public | BindingFlags.Instance )
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

            return new ThriftMethodParameter( attr.Id, attr.Name, info );
        }

        /// <summary>
        /// Attempts to parse all "throws" clauses on the specified MethodInfo.
        /// </summary>
        private static ThriftThrowsClause[] ParseThrowsClauses( MethodInfo info )
        {
            var clauses = info.GetAttributes<ThriftThrowsAttribute>()
                              .Select( a => new ThriftThrowsClause( a.Id, a.Name, a.ExceptionType ) )
                              .ToArray();

            var wrongClause = clauses.FirstOrDefault( c => !typeof( Exception ).IsAssignableFrom( c.ExceptionType ) );
            if ( wrongClause != null )
            {
                throw ThriftParsingException.NotAnException( wrongClause.ExceptionType );
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

            var throwsClauses = ParseThrowsClauses( info );
            var parameters = info.GetParameters()
                                 .Select( ParseMethodParameter )
                                 .ToArray();
            var unwrapped = ReflectionEx.UnwrapTaskIfNeeded( info.ReturnType );
            return new ThriftMethod( attr.Name, unwrapped ?? info.ReturnType, attr.IsOneWay, unwrapped != null, parameters, throwsClauses, info.Name );
        }

        /// <summary>
        /// Attempts to parse a Thrift service from the specified Type.
        /// </summary>
        public static ThriftService ParseService( Type type )
        {
            if ( !type.IsInterface )
            {
                throw ThriftParsingException.NotAService( type );
            }

            var attr = type.GetAttribute<ThriftServiceAttribute>();
            if ( attr == null )
            {
                throw ThriftParsingException.ServiceWithoutAttribute( type );
            }

            var methods = type.GetMethods( BindingFlags.Public | BindingFlags.Instance )
                              .Select( ParseMethod )
                              .ToArray();
            return new ThriftService( attr.Name, methods );
        }
    }
}