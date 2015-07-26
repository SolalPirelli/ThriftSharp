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
        /// <summary>
        /// Gets the converter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a TypeInfo representing the interface implemented by the converter.
        /// </summary>
        public TypeInfo InterfaceTypeInfo { get; }

        /// <summary>
        /// Gets the type the converter converts from.
        /// </summary>
        public Type FromType
        {
            get { return InterfaceTypeInfo.GenericTypeArguments[0]; }
        }

        /// <summary>
        /// Gets the type the converter converts to.
        /// </summary>
        public Type ToType
        {
            get { return InterfaceTypeInfo.GenericTypeArguments[1]; }
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

            Type = type;
            InterfaceTypeInfo = ifaces[0];
        }


        public Expression CreateCall( string methodName, Expression target )
        {
            return Expression.Call(
                Expression.New( Type ),
                InterfaceTypeInfo.GetDeclaredMethod( methodName ),
                target
            );
        }
    }
}