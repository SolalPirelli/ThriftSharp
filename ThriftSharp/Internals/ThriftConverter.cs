using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift converter, including commonly-needed properties.
    /// </summary>
    internal sealed class ThriftConverter
    {
        private Type _type;
        private TypeInfo _interfaceTypeInfo;

        /// <summary>
        /// Gets the type the converter converts from.
        /// </summary>
        public Type FromType
        {
            get { return _interfaceTypeInfo.GenericTypeArguments[0]; }
        }

        /// <summary>
        /// Initializes a new instance of the ThriftConverter class with the specified type.
        /// </summary>
        public ThriftConverter( Type type )
        {
            var typeInfo = type.GetTypeInfo();
            var ifaces = typeInfo.GetGenericInterfaces( typeof( IThriftValueConverter<,> ) );
            if ( ifaces.Length == 0 )
            {
                throw new ArgumentException( $"The type '{type.Name}' does not IThriftValueConverter<TFrom, TTo>." );
            }
            if ( ifaces.Length > 1 )
            {
                throw new ArgumentException( $"The type '{type.Name}' implements IThriftValueConverter<TFrom, TTo> more than once." );
            }

            var ctor = typeInfo.DeclaredConstructors.FirstOrDefault( c => c.GetParameters().Length == 0 );
            if ( ctor == null )
            {
                throw new ArgumentException( $"The type '{type.Name}' does not have a parameterless constructor." );
            }

            _type = type;
            _interfaceTypeInfo = ifaces[0];
        }


        /// <summary>
        /// Creates an expression calling the specified method on the converter, with the specified argument.
        /// </summary>
        public Expression CreateCall( string methodName, Expression arg )
        {
            return Expression.Call(
                Expression.New( _type ),
                _interfaceTypeInfo.GetDeclaredMethod( methodName ),
                arg
            );
        }
    }
}