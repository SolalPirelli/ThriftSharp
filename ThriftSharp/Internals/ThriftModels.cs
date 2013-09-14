using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift field.
    /// </summary>
    internal sealed class ThriftField
    {
        /// <summary>
        /// Gets the field's header.
        /// </summary>
        public ThriftFieldHeader Header { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the field is required.
        /// </summary>
        /// <remarks>
        /// If the field is required, an exception should be thrown if it is not present during deserialization.
        /// </remarks>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets the field's default value, if any.
        /// </summary>
        public Option<object> DefaultValue { get; private set; }

        /// <summary>
        /// Gets the field's underlying type.
        /// </summary>
        public Type UnderlyingType { get; private set; }

        /// <summary>
        /// Gets a get method that takes an instance and returns the field's value for that instance, if the field is not write-only.
        /// </summary>
        public Func<object, object> Getter { get; private set; }

        /// <summary>
        /// Gets a set method that takes an instance and a value and sets the field's value on that instance, if the field is not read-only.
        /// </summary>
        public Action<object, object> Setter { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftField class with the specified values.
        /// </summary>
        public ThriftField( ThriftFieldHeader header, bool isRequired, Option<object> defaultValue,
                            Type underlyingType, Func<object, object> getter, Action<object, object> setter )
        {
            Header = header;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            UnderlyingType = underlyingType;
            Getter = getter;
            Setter = setter;
        }
    }

    /// <summary>
    /// Thrift enum members.
    /// </summary>
    internal sealed class ThriftEnumMember
    {
        /// <summary>
        /// Gets the member's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the member's value.
        /// </summary>
        /// <remarks>
        /// Thrift enums are always Int32s.
        /// </remarks>
        public int Value { get; private set; }

        /// <summary>
        /// Gets the member's underlying field.
        /// </summary>
        public FieldInfo UnderlyingField { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftEnumMember class with the specified values.
        /// </summary>
        public ThriftEnumMember( string name, int value, FieldInfo underlyingField )
        {
            Name = name;
            Value = value;
            UnderlyingField = underlyingField;
        }
    }

    /// <summary>
    /// Thrift enums.
    /// </summary>
    internal sealed class ThriftEnum
    {
        /// <summary>
        /// Gets the enum's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the enum's members.
        /// </summary>
        public ReadOnlyCollection<ThriftEnumMember> Members { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftEnum class with the specified values.
        /// </summary>
        public ThriftEnum( string name, IList<ThriftEnumMember> members )
        {
            Name = name;
            Members = members.CopyAsReadOnly();
        }
    }

    /// <summary>
    /// Thrift struct.
    /// </summary>
    internal sealed class ThriftStruct
    {
        /// <summary>
        /// Gets the struct's header.
        /// </summary>
        public ThriftStructHeader Header { get; private set; }

        /// <summary>
        /// Gets the struct's fields.
        /// </summary>
        public ReadOnlyCollection<ThriftField> Fields { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftStructHeader class with the specified values.
        /// </summary>
        public ThriftStruct( ThriftStructHeader header, IList<ThriftField> fields )
        {
            Header = header;
            Fields = fields.CopyAsReadOnly();
        }
    }

    /// <summary>
    /// Thrift method "throws" clause.
    /// </summary>
    internal sealed class ThriftThrowsClause
    {
        /// <summary>
        /// Gets the clause's ID.
        /// </summary>
        public short Id { get; private set; }

        /// <summary>
        /// Gets the clause's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the clause's exception type.
        /// </summary>
        public Type ExceptionType { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftThrowsClause class with the specified values.
        /// </summary>
        public ThriftThrowsClause( short id, string name, Type exceptionType )
        {
            Id = id;
            Name = name;
            ExceptionType = exceptionType;
        }
    }

    /// <summary>
    /// Thrift method parameter.
    /// </summary>
    internal sealed class ThriftMethodParameter
    {
        /// <summary>
        /// Gets the parameter's ID.
        /// </summary>
        public short Id { get; private set; }

        /// <summary>
        /// Gets the parameter's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the parameter's underlying information.
        /// </summary>
        public Type UnderlyingType { get; private set; }

        /// <summary>
        /// Gets the parameter's converter, if any.
        /// </summary>
        public IThriftValueConverter Converter { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftMethodParameter class with the specified values.
        /// </summary>
        public ThriftMethodParameter( short id, string name, Type underlyingType, IThriftValueConverter converter )
        {
            Id = id;
            Name = name;
            UnderlyingType = underlyingType;
            Converter = converter;
        }
    }

    /// <summary>
    /// Thrift method.
    /// </summary>
    internal sealed class ThriftMethod
    {
        /// <summary>
        /// Gets the method's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the method's return type.
        /// </summary>
        public Type ReturnType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the method is asynchronous.
        /// </summary>
        public bool IsAsync { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the method is one-way.
        /// </summary>
        /// <remarks>
        /// If it is, no reply should be read.
        /// </remarks>
        public bool IsOneWay { get; private set; }

        /// <summary>
        /// Gets the converter used to convert the method's return type, if any.
        /// </summary>
        public IThriftValueConverter ReturnValueConverter { get; private set; }

        /// <summary>
        /// Gets the method's parameters.
        /// </summary>
        public ReadOnlyCollection<ThriftMethodParameter> Parameters { get; private set; }

        /// <summary>
        /// Gets the method's "throws" clauses.
        /// </summary>
        public ReadOnlyCollection<ThriftThrowsClause> Exceptions { get; private set; }

        /// <summary>
        /// Gets the method's underlying name.
        /// </summary>
        /// <remarks>
        /// This is only used to identify methods in client code.
        /// </remarks>
        public string UnderlyingName { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftMethod class with the specified values.
        /// </summary>
        public ThriftMethod( string name, Type returnType, bool isOneWay, bool isAsync,
                             IThriftValueConverter returnValueConverter,
                             IList<ThriftMethodParameter> parameters, IList<ThriftThrowsClause> exceptions,
                             string underlyingName )
        {
            Name = name;
            ReturnType = returnType;
            IsOneWay = isOneWay;
            IsAsync = isAsync;
            ReturnValueConverter = returnValueConverter;
            Parameters = parameters.CopyAsReadOnly();
            Exceptions = exceptions.CopyAsReadOnly();
            UnderlyingName = underlyingName;
        }
    }

    /// <summary>
    /// Thrift service.
    /// </summary>
    internal sealed class ThriftService
    {
        /// <summary>
        /// Gets the service's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the service's methods.
        /// </summary>
        public ReadOnlyCollection<ThriftMethod> Methods { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftService class with the specified values.
        /// </summary>
        public ThriftService( string name, IList<ThriftMethod> methods )
        {
            Name = name;
            Methods = methods.CopyAsReadOnly();
        }
    }
}