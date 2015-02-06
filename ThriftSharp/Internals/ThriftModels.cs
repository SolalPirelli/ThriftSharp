// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift field.
    /// </summary>
    internal sealed class ThriftField
    {
        private readonly Func<object, object> _getter;
        private readonly Action<object, object> _setter;

        /// <summary>
        /// Gets the field's header.
        /// </summary>
        public readonly ThriftFieldHeader Header;

        /// <summary>
        /// Gets a value indicating whether the field is required.
        /// </summary>
        /// <remarks>
        /// If the field is required, an exception should be thrown if it is not present during deserialization.
        /// </remarks>
        public readonly bool IsRequired;

        /// <summary>
        /// Gets the field's default value, if any.
        /// </summary>
        public readonly Option DefaultValue;



        /// <summary>
        /// Initializes a new instance of the ThriftField class with the specified values.
        /// </summary>
        public ThriftField( short id, string name, bool isRequired, Option defaultValue,
                            TypeInfo typeInfo, Func<object, object> getter, Action<object, object> setter )
        {
            _getter = getter;
            _setter = setter;

            Header = new ThriftFieldHeader( id, name, ThriftType.Get( typeInfo.AsType() ) );
            IsRequired = isRequired;
            DefaultValue = defaultValue;
        }


        /// <summary>
        /// Takes an instance and returns the field's value for that instance.
        /// </summary>
        public object GetValue( object instance )
        {
            return _getter( instance );
        }

        /// <summary>
        /// Takes an instance and a value and sets the field's value on that instance.
        /// </summary>
        public void SetValue( object instance, object value )
        {
            _setter( instance, value );
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
        public readonly ThriftStructHeader Header;

        /// <summary>
        /// Gets the struct's fields.
        /// </summary>
        public readonly IReadOnlyList<ThriftField> Fields;

        /// <summary>
        /// Gets the struct's underlying TypeInfo.
        /// </summary>
        public readonly TypeInfo TypeInfo;


        /// <summary>
        /// Initializes a new instance of the ThriftStructHeader class with the specified values.
        /// </summary>
        public ThriftStruct( ThriftStructHeader header, IReadOnlyList<ThriftField> fields, TypeInfo typeInfo )
        {
            Header = header;
            Fields = fields;
            TypeInfo = typeInfo;
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
        public readonly short Id;

        /// <summary>
        /// Gets the clause's name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Gets the clause's exception TypeInfo.
        /// </summary>
        public readonly TypeInfo ExceptionTypeInfo;


        /// <summary>
        /// Initializes a new instance of the ThriftThrowsClause class with the specified values.
        /// </summary>
        public ThriftThrowsClause( short id, string name, TypeInfo exceptionTypeInfo )
        {
            Id = id;
            Name = name;
            ExceptionTypeInfo = exceptionTypeInfo;
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
        public readonly short Id;

        /// <summary>
        /// Gets the parameter's name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Gets the parameter's underlying TypeInfo.
        /// </summary>
        public readonly TypeInfo TypeInfo;

        /// <summary>
        /// Gets the parameter's converter, if any.
        /// </summary>
        public readonly IThriftValueConverter Converter;


        /// <summary>
        /// Initializes a new instance of the ThriftMethodParameter class with the specified values.
        /// </summary>
        public ThriftMethodParameter( short id, string name, TypeInfo typeInfo, IThriftValueConverter converter )
        {
            Id = id;
            Name = name;
            TypeInfo = typeInfo;
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
        public readonly string Name;

        /// <summary>
        /// Gets the method's return type.
        /// </summary>
        public readonly Type ReturnType;

        /// <summary>
        /// Gets a value indicating whether the method is one-way.
        /// </summary>
        /// <remarks>
        /// If it is, no reply should be read.
        /// </remarks>
        public readonly bool IsOneWay;

        /// <summary>
        /// Gets the converter used to convert the method's return type, if any.
        /// </summary>
        public readonly IThriftValueConverter ReturnValueConverter;

        /// <summary>
        /// Gets the method's parameters.
        /// </summary>
        public readonly IReadOnlyList<ThriftMethodParameter> Parameters;

        /// <summary>
        /// Gets the method's "throws" clauses.
        /// </summary>
        public readonly IReadOnlyList<ThriftThrowsClause> Exceptions;

        /// <summary>
        /// Gets the method's underlying name.
        /// </summary>
        /// <remarks>
        /// This is only used to identify methods in client code.
        /// </remarks>
        public readonly string UnderlyingName;


        /// <summary>
        /// Initializes a new instance of the ThriftMethod class with the specified values.
        /// </summary>
        public ThriftMethod( string name, Type returnType, bool isOneWay,
                             IThriftValueConverter returnValueConverter,
                             IReadOnlyList<ThriftMethodParameter> parameters, IReadOnlyList<ThriftThrowsClause> exceptions,
                             string underlyingName )
        {
            Name = name;
            ReturnType = returnType;
            IsOneWay = isOneWay;
            ReturnValueConverter = returnValueConverter;
            Parameters = parameters;
            Exceptions = exceptions;
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
        public readonly string Name;

        /// <summary>
        /// Gets the service's methods.
        /// </summary>
        public readonly IReadOnlyList<ThriftMethod> Methods;


        /// <summary>
        /// Initializes a new instance of the ThriftService class with the specified values.
        /// </summary>
        public ThriftService( string name, IReadOnlyList<ThriftMethod> methods )
        {
            Name = name;
            Methods = methods;
        }
    }
}