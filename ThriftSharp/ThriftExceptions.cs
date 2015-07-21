// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Reflection;
using ThriftSharp.Internals;

namespace ThriftSharp
{
    /// <summary>
    /// Raised when a parsing error occurs.
    /// </summary>
    public sealed class ThriftParsingException : Exception
    {
        /// <summary>
        /// Prevents a default instance of the ThriftParsingException class from being created.
        /// This is meant to force the use one of the static factory methods.
        /// </summary>
        private ThriftParsingException( string message ) : base( message ) { }


        internal static ThriftParsingException EnumWithoutAttribute( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The enum type '{typeInfo.Name}' is not part of a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftEnumAttribute attribute." );
        }

        internal static ThriftParsingException NonInt32Enum( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The enum type '{typeInfo.Name}' has an underlying type different from Int32."
                                             + Environment.NewLine
                                             + "Only enums whose underlying type is Int32 are supported." );
        }

        internal static ThriftParsingException NotAConcreteType( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The type '{typeInfo.Name}' is not a concrete type."
                                             + Environment.NewLine
                                             + "Only concrete types can be used in Thrift interfaces." );
        }

        internal static ThriftParsingException StructWithoutAttribute( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The class or struct type '{typeInfo.Name}' is not part of a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftStructAttribute attribute." );
        }

        internal static ThriftParsingException RequiredNullableField( PropertyInfo propertyInfo )
        {
            return new ThriftParsingException( $"The Thrift field '{propertyInfo.Name}' is required, but its type is nullable."
                                             + Environment.NewLine
                                             + "This is not supported. Please use non-nullable types for required value fields." );
        }

        internal static ThriftParsingException OptionalValueField( PropertyInfo propertyInfo )
        {
            return new ThriftParsingException( $"The Thrift field '{propertyInfo.Name}' is optional, but its type is a value type."
                                             + Environment.NewLine
                                             + "This is not supported. Please use Nullable<T> for optional value fields." );
        }

        internal static ThriftParsingException UnknownValueType( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The type '{typeInfo.Name}' is an user-defined value type."
                                             + Environment.NewLine
                                             + "The only available value types are Thrift primitive types and nullable types."
                                             + Environment.NewLine
                                             + "If this is unintentional, change the type to a reference type." );
        }

        internal static ThriftParsingException UnsupportedMap( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The map type '{typeInfo.Name}' is not supported."
                                             + Environment.NewLine
                                             + "Supported map types are IDictionary<TKey, TValue> "
                                             + "and any concrete implementation with a parameterless constructor." );
        }

        internal static ThriftParsingException UnsupportedSet( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The set type '{typeInfo.Name}' is not supported."
                                             + Environment.NewLine
                                             + "Supported set types are ISet<T> and any concrete implementation with a parameterless constructor." );
        }

        internal static ThriftParsingException UnsupportedList( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The list type '{typeInfo.Name}' is not supported."
                                             + Environment.NewLine
                                             + "Supported list types are arrays, IList<T> "
                                             + "and any concrete implementation of that interface with a parameterless constructor." );
        }

        internal static ThriftParsingException CollectionWithOrthogonalInterfaces( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The collection type '{typeInfo.Name}' implements more than one of IDictionary<TKey, TValue>, ISet<T> and IList<T>."
                                             + Environment.NewLine
                                             + "This is not supported. Please only use collections implementing exactly one of these interfaces, or an array." );
        }

        internal static ThriftParsingException NotAnException( TypeInfo typeInfo, MethodInfo methodInfo )
        {
            return new ThriftParsingException( $"Type '{typeInfo.Name}' was used in a ThriftThrowsAttribute for method '{methodInfo.Name}', "
                                             + "but does not inherit from Exception."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftStructAttribute." );
        }

        internal static ThriftParsingException NotAService( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The type '{typeInfo.Name}' is not an interface." );
        }

        internal static ThriftParsingException ServiceWithoutAttribute( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The interface '{typeInfo.Name}' does not have a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftServiceAttribute attribute." );
        }

        internal static ThriftParsingException NoMethods( TypeInfo typeInfo )
        {
            return new ThriftParsingException( $"The interface '{typeInfo.Name}' does not have any methods, and is therefore useless."
                                             + Environment.NewLine
                                             + "If this is unintentional, add some methods to it." );
        }

        internal static ThriftParsingException MethodWithoutAttribute( MethodInfo methodInfo )
        {
            return new ThriftParsingException( $"The method '{methodInfo.Name}' (in type '{methodInfo.DeclaringType.Name}') is not part of the Thrift interface."
                                             + Environment.NewLine
                                             + "All methods in a Thrift service definition must be part of the Thrift interface."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftMethodAttribute attribute." );
        }

        internal static ThriftParsingException ParameterWithoutAttribute( ParameterInfo info )
        {
            return new ThriftParsingException( $"Parameter '{info.Name}' of method '{info.Member.Name}' (in type '{info.Member.DeclaringType.Name}') does not have a Thrift interface definition."
                                             + Environment.NewLine
                                             + "It must be decorated with the ThriftParameterAttribute attribute." );
        }

        internal static ThriftParsingException MoreThanOneCancellationToken( MethodInfo methodInfo )
        {
            return new ThriftParsingException( $"The method '{methodInfo.Name}' (in type '{methodInfo.DeclaringType.Name}') has more than one CancellationToken parameter."
                                             + Environment.NewLine
                                             + "A Thrift method can only have at most one such parameter." );
        }

        internal static ThriftParsingException SynchronousMethod( MethodInfo methodInfo )
        {
            return new ThriftParsingException( $"The method '{methodInfo.Name}' does not return a Task or Task<T>."
                                             + Environment.NewLine
                                             + "Only asynchronous calls are supported. Please wrap the return type in a Task." );
        }

        internal static ThriftParsingException OneWayMethodWithResult( MethodInfo methodInfo )
        {
            return new ThriftParsingException( $"The method '{methodInfo.Name}' is a one-way method, but returns a value."
                                             + Environment.NewLine
                                             + "One-way methods cannot return a value. Please either make it two-way, or remove the return value (make it a Task)." );
        }

        internal static ThriftParsingException OneWayMethodWithExceptions( MethodInfo methodInfo )
        {
            return new ThriftParsingException( $"The method '{methodInfo.Name}' is a one-way method, but has declared thrown exceptions."
                                             + Environment.NewLine
                                             + "One-way methods cannot throw exceptions since they do not wait for a server response. Please either make it two-way, or remove the declared exceptions." );
        }
    }

    /// <summary>
    /// Raised when a serialization error occurs.
    /// </summary>
    public sealed class ThriftSerializationException : Exception
    {
        /// <summary>
        /// Prevents a default instance of the ThriftSerializationException class from being created.
        /// This is meant to force customers to use one of the static factory methods.
        /// </summary>
        private ThriftSerializationException( string message ) : base( message ) { }

        internal static ThriftSerializationException RequiredFieldIsNull( string fieldName )
        {
            return new ThriftSerializationException( $"Field '{fieldName}' is a required field and cannot be null when writing it." );
        }

        internal static ThriftSerializationException NullParameter( string parameterName )
        {
            return new ThriftSerializationException( $"Parameter '{parameterName}' was null." );
        }

        internal static ThriftSerializationException MissingRequiredField( string fieldName )
        {
            return new ThriftSerializationException( $"Field '{fieldName}' is a required field, but was not present." );
        }

        internal static ThriftSerializationException TypeIdMismatch( ThriftTypeId expectedId, ThriftTypeId actualId )
        {
            return new ThriftSerializationException( $"Deserialization error: Expected type {expectedId}, but type {actualId} was read." );
        }
    }
}