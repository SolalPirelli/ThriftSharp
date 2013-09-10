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
        /// Prevents a default instance of the ThriftParsingExceptionClass from being called.
        /// </summary>
        private ThriftParsingException( string message, params object[] args )
            : base( string.Format( message, args ) ) { }

        /// <summary>
        /// Creates an exception indicating a type was expected to be an enum but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAnEnum( Type type )
        {
            return new ThriftParsingException( "The specified type, '{0}', is not an enum.", type.FullName );
        }

        /// <summary>
        /// Creates an exception indicating an enum type lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException EnumWithoutAttribute( Type type )
        {
            return new ThriftParsingException( "The enum type '{0}' is not part of a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftEnumAttribute attribute.",
                                               type.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a type was expected to be a class or struct but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAStruct( Type type )
        {
            return new ThriftParsingException( "The specified type, '{0}', is not a class or a struct.", type.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a class or struct type lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException StructWithoutAttribute( Type type )
        {
            return new ThriftParsingException( "The class or struct type '{0}' is not part of a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftStructAttribute attribute.",
                                               type.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a parameter lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException ParameterWithoutAttribute( ParameterInfo info )
        {
            return new ThriftParsingException( "Parameter '{0}' of method '{1}' of type '{2}' does not have a Thrift interface definition."
                                             + Environment.NewLine
                                             + "It must be decorated with the ThriftParameterAttribute attribute.",
                                               info.Name, info.Member.Name, info.Member.DeclaringType.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a type was expected to be an exception but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAnException( Type type )
        {
            return new ThriftParsingException( "Type '{0}' was used in a ThriftThrowsClauseAttribute but does not inherit from Exception."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftStructAttribute.",
                                               type.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a type was expected to be an interface but wasn't.
        /// </summary>
        internal static ThriftParsingException NotAService( Type type )
        {
            return new ThriftParsingException( "The specified type, '{0}', is not an interface.", type.FullName );
        }

        /// <summary>
        /// Creates an exception indicating a service type lacked the appropriate attribute.
        /// </summary>
        internal static ThriftParsingException ServiceWithoutAttribute( Type type )
        {
            return new ThriftParsingException( "The interface '{0}' does not have a Thrift interface definition."
                                             + Environment.NewLine
                                             + "If this is unintentional, mark it with the ThriftServiceAttribute attribute.",
                                               type.FullName );
        }
    }

    /// <summary>
    /// Occurs when a problem occurs during the transport of Thrift data.
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