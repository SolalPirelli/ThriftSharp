// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System.Collections.Generic;
using System.Reflection;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift field.
    /// </summary>
    internal sealed class ThriftField
    {
        /// <summary>
        /// Gets the field's ID.
        /// </summary>
        public readonly short Id;

        /// <summary>
        /// Gets the field's name.
        /// </summary>
        public readonly string Name;

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
        public readonly object DefaultValue;

        /// <summary>
        /// Gets the field's underlying type.
        /// </summary>
        public readonly TypeInfo TypeInfo;

        /// <summary>
        /// Gets the field's type as used on the wire.
        /// </summary>
        public readonly TypeInfo WireTypeInfo;

        /// <summary>
        /// Gets the converter associated with the field, if any.
        /// </summary>
        public readonly IThriftValueConverter Converter;

        /// <summary>
        /// Gets the property associated with the field.
        /// </summary>
        public readonly PropertyInfo BackingProperty;


        private ThriftField( short id, string name, bool isRequired, object defaultValue, TypeInfo typeInfo, IThriftValueConverter converter, PropertyInfo backingProperty )
        {
            Id = id;
            Name = name;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            TypeInfo = typeInfo;
            WireTypeInfo = converter == null ? typeInfo : converter.FromType.GetTypeInfo();
            Converter = converter;
            BackingProperty = backingProperty;
        }

        public static ThriftField Field( short id, string name, bool isRequired, object defaultValue, IThriftValueConverter converter, PropertyInfo backingProperty )
        {
            return new ThriftField( id, name, isRequired, defaultValue, backingProperty.PropertyType.GetTypeInfo(), converter, backingProperty );
        }

        public static ThriftField Parameter( short id, string name, TypeInfo typeInfo, IThriftValueConverter converter )
        {
            return new ThriftField( id, name, true, null, typeInfo, converter, null );
        }

        public static ThriftField ThrowsClause( short id, string name, TypeInfo typeInfo )
        {
            return new ThriftField( id, name, false, null, typeInfo, null, null );
        }

        public static ThriftField ReturnValue( TypeInfo typeInfo, IThriftValueConverter converter )
        {
            return new ThriftField( 0, null, false, null, typeInfo, converter, null );
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
    /// Thrift method.
    /// </summary>
    internal sealed class ThriftMethod
    {
        /// <summary>
        /// Gets the method's name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Gets a value indicating whether the method is one-way.
        /// </summary>
        /// <remarks>
        /// If it is, no reply should be read.
        /// </remarks>
        public readonly bool IsOneWay;

        /// <summary>
        /// Gets the method's return value.
        /// </summary>
        public readonly ThriftField ReturnValue;

        /// <summary>
        /// Gets the method's parameters.
        /// </summary>
        public readonly IReadOnlyList<ThriftField> Parameters;

        /// <summary>
        /// Gets the method's "throws" clauses.
        /// </summary>
        public readonly IReadOnlyList<ThriftField> Exceptions;


        /// <summary>
        /// Initializes a new instance of the ThriftMethod class with the specified values.
        /// </summary>
        public ThriftMethod( string name, bool isOneWay,
                             ThriftField returnValue,
                             IReadOnlyList<ThriftField> parameters, IReadOnlyList<ThriftField> exceptions )
        {
            Name = name;
            ReturnValue = returnValue;
            IsOneWay = isOneWay;
            Parameters = parameters;
            Exceptions = exceptions;
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
        /// Gets the service's Thrift methods, mapped by their .NET name.
        /// </summary>
        public readonly IReadOnlyDictionary<string, ThriftMethod> Methods;


        /// <summary>
        /// Initializes a new instance of the ThriftService class with the specified values.
        /// </summary>
        public ThriftService( string name, IReadOnlyDictionary<string, ThriftMethod> methods )
        {
            Name = name;
            Methods = methods;
        }
    }
}