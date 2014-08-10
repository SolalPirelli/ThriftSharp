// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

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
        /// This is meant to force customers to use one of the static factory methods.
        /// </summary>
        private ThriftParsingException( string message, params object[] args )
            : base( string.Format( message, args ) ) { }


        /// <summary>
        /// Creates an exception indicating a Thrift struct lacked Thrift fields.
        /// </summary>
        internal static ThriftParsingException NoFields( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The type '{0}' does not have any Thrift fields and is therefore useless."
                                             + Environment.NewLine
                                             + "Please either remove this type and all references to it, mark existing properties with the ThriftFieldAttribute attribute, or add new properties marked with the ThriftFieldAttribute attribute.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating an enum type lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException EnumWithoutAttribute( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The enum type '{0}' is not part of a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftEnumAttribute attribute.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating an enum type was of the wrong underlying type.
        /// </summary>
        internal static ThriftParsingException NonInt32Enum( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The enum type '{0}' has an underlying type different from Int32."
                                             + Environment.NewLine
                                             + "Only enums whose underlying type is Int32 are supported.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating that a type for a Thrift struct was expected to be concrete but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAConcreteType( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The type '{0}' is not a concrete type."
                                             + Environment.NewLine
                                             + "Only concrete types can be used in Thrift interfaces.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a class or struct type lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException StructWithoutAttribute( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The class or struct type '{0}' is not part of a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftStructAttribute attribute.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating an optional field has a value type field type.
        /// </summary>
        internal static ThriftParsingException OptionalValueField( PropertyInfo propertyInfo )
        {
            return new ThriftParsingException( "The Thrift field '{0}' is optional, but its type is a value type."
                                             + Environment.NewLine
                                             + "This is not supported. Please use Nullable<T> for optional value fields.",
                                               propertyInfo.Name );
        }

        /// <summary>
        /// Creates an exception indicating an unknown value type was encountered.
        /// </summary>
        internal static ThriftParsingException UnknownValueType( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The type '{0}' is an user-defined value type."
                                             + Environment.NewLine
                                             + "The only available value types are Thrift primitive types and nullable types."
                                             + Environment.NewLine
                                             + "If this is unintentional, change the type to a reference type.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a map type is not supported.
        /// </summary>
        internal static ThriftParsingException UnsupportedMap( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The map type '{0}' is not supported."
                                             + Environment.NewLine
                                             + "Supported types are IDictionary<K,V> and any concrete implementation with a parameterless constructor.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a set type is not supported.
        /// </summary>
        internal static ThriftParsingException UnsupportedSet( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The set type '{0}' is not supported."
                                             + Environment.NewLine
                                             + "Supported types are ISet<T> and any concrete implementation with a parameterless constructor.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a list type is not supported.
        /// </summary>
        internal static ThriftParsingException UnsupportedList( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The list type '{0}' is not supported."
                                             + Environment.NewLine
                                             + "Supported types are arrays, ICollection<T>, IList<T>, and any concrete implementation of ICollection<T> or IList<T> with a parameterless constructor.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a parameter lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException ParameterWithoutAttribute( ParameterInfo info )
        {
            return new ThriftParsingException( "Parameter '{0}' of method '{1}' of type '{2}' does not have a Thrift interface definition."
                                             + Environment.NewLine
                                             + "It must be decorated with the ThriftParameterAttribute attribute.",
                                               info.Name, info.Member.Name, info.Member.DeclaringType );
        }

        /// <summary>
        /// Creates an exception indicating a type was expected to be an exception but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAnException( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "Type '{0}' was used in a ThriftThrowsClauseAttribute but does not inherit from Exception."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftStructAttribute.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a type was expected to be an interface but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAService( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The type '{0}' is not an interface.", typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a service type lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException ServiceWithoutAttribute( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The interface '{0}' does not have a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftServiceAttribute attribute.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a service lacked methods.
        /// </summary>
        internal static ThriftParsingException NoMethods( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The interface '{0}' does not have any methods, and is therefore useless."
                                             + Environment.NewLine
                                             + "If this is unintentional, add some methods to it.",
                                               typeInfo.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a method lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException MethodWithoutAttribute( MethodInfo methodInfo )
        {
            return new ThriftParsingException( "The method '{0}' is not part of the Thrift interface."
                                             + Environment.NewLine
                                             + "All methods in a Thrift service definition must be part of the Thrift interface."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftMethodAttribute attribute.",
                                               methodInfo.Name );
        }

        /// <summary>
        /// Creates an exception indicating a method was not async but should have been.
        /// </summary>
        internal static ThriftParsingException SynchronousMethod( MethodInfo methodInfo )
        {
            return new ThriftParsingException( "The method '{0}' is two-way, but does not return a Task or Task<T>."
                                             + Environment.NewLine
                                             + "Only asynchronous calls are supported for two-way methods. Please wrap the return type in a Task.",
                                               methodInfo.Name );
        }

        /// <summary>
        /// Creates an exception indicating a one-way method had a result.
        /// </summary>
        internal static ThriftParsingException OneWayMethodWithResult( MethodInfo methodInfo )
        {
            return new ThriftParsingException( "The method '{0}' is a one-way method, but returns a value."
                                             + Environment.NewLine
                                             + "One-way methods cannot return a value. Please either make it two-way, or remove the return value (make it a Task).",
                                               methodInfo.Name );
        }

        /// <summary>
        /// Creates an exception indicating a one-way method had declared exceptions.
        /// </summary>
        internal static ThriftParsingException OneWayMethodWithExceptions( MethodInfo methodInfo )
        {
            return new ThriftParsingException( "The method '{0}' is a one-way method, but has declared thrown exceptions."
                                             + Environment.NewLine
                                             + "One-way methods cannot throw exceptions since they do not wait for a server response. Please either make it two-way, or remove the declared exceptions.",
                                               methodInfo.Name );
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
        private ThriftSerializationException( string message, params object[] args ) : base( string.Format( message, args ) ) { }

        /// <summary>
        /// Creates a ThriftSerializationException indicating a required field was null during serialization.
        /// </summary>
        internal static ThriftSerializationException RequiredFieldIsNull( string structName, string fieldName )
        {
            return new ThriftSerializationException( "Field '{0}' of struct '{1}' is a required field and cannot be null when writing it.", structName, fieldName );
        }

        /// <summary>
        /// Creates a ThriftSerializationException indicating a required field was not present during deserialization.
        /// </summary>
        internal static ThriftSerializationException MissingRequiredField( string structName, string fieldName )
        {
            return new ThriftSerializationException( "Field '{0}' of struct '{1}' is a required field, but was not present.", structName, fieldName );
        }

        /// <summary>
        /// Creates a ThriftSerializationException indicating a read type ID did not match the expected type ID.
        /// </summary>
        internal static ThriftSerializationException TypeIdMismatch( ThriftTypeId expectedId, ThriftTypeId actualId )
        {
            return new ThriftSerializationException( "Deserialization error: Expected type {0}, but type {1} was read.", expectedId, actualId );
        }
    }
}