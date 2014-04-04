// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Reflection;

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
        /// Creates an exception indicating a type was expected to be an enum but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAnEnum( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The type '{0}' is not an enum.", typeInfo.FullName );
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
        /// Creates an exception indicating a type was expected to be a class or struct but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAStruct( TypeInfo typeInfo )
        {
            return new ThriftParsingException( "The type '{0}' is not a class or a struct.", typeInfo.FullName );
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
        /// Creates an exception indicating a method was not async but should have been.
        /// </summary>
        internal static ThriftParsingException NotAsync( MethodInfo methodInfo )
        {
            return new ThriftParsingException( "The method '{0}' does not return a Task or Task<T>."
                                             + Environment.NewLine
                                             + "Only asynchronous methods are supported. Please wrap the return type in a Task.",
                                               methodInfo.Name );
        }
    }

    /// <summary>
    /// Raised when a serialization error occurs.
    /// </summary>
    public sealed class ThriftSerializationException : Exception
    {
        private ThriftSerializationException( string message, params string[] args ) : base( string.Format( message, args ) ) { }

        public static ThriftSerializationException CannotWriteNull( string structName, string fieldName )
        {
            return new ThriftSerializationException( "Field '{0}' of struct '{1}' is a required field and cannot be null when writing it.", structName, fieldName );
        }
    }

    /// <summary>
    /// Raised when a problem occurs during the transport of Thrift data.
    /// </summary>
    public sealed class ThriftTransportException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ThriftTransportException class with the specified message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        internal ThriftTransportException( string message ) : base( message ) { }
    }
}