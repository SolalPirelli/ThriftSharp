// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Required attribute for Thrift enums.
    /// </summary>
    /// <remarks>
    /// This is only a marker attribute that ensures users do not involuntarily use wrong enums.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Enum )]
    public sealed class ThriftEnumAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ThriftEnumAttribute class.
        /// </summary>
        public ThriftEnumAttribute() { }
    }

    /// <summary>
    /// Required attribute for Thrift fields.
    /// </summary>
    /// <remarks>
    /// Properties without this attribute will be ignored.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Property )]
    public sealed class ThriftFieldAttribute : Attribute
    {
        /// <summary>
        /// Gets the Thrift field's ID.
        /// </summary>
        public short Id { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the field is required.
        /// </summary>
        /// <remarks>
        /// If the Thrift field is required, an exception will be thrown if it is not set during serialization.
        /// </remarks>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets the Thrift field's name.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftFieldAttribute class with the specified values.
        /// </summary>
        /// <param name="id">The ID of the field the attribute is applied to. Must be positive.</param>
        /// <param name="isRequired">Whether the field the attribute is applied to is required.</param>
        /// <param name="name">The name of the field the attribute is applied to.</param>
        public ThriftFieldAttribute( short id, bool isRequired, string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, () => name );

            Id = id;
            IsRequired = isRequired;
            Name = name;
        }
    }

    /// <summary>
    /// Optional attribute for Thrift fields specifying their default value.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property )]
    public sealed class ThriftDefaultValueAttribute : Attribute
    {
        /// <summary>
        /// Gets the Thrift field's default value.
        /// </summary>
        public object Value { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftDefaultValueAttribute class with the specified value.
        /// </summary>
        /// <param name="value">The default value of the field the attribute is applied to.</param>
        public ThriftDefaultValueAttribute( object value )
        {
            Value = value;
        }
    }

    /// <summary>
    /// Required attribute for Thrift structs.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct )]
    public sealed class ThriftStructAttribute : Attribute
    {
        /// <summary>
        /// Gets the Thrift struct's name.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftStructAttribute class with the specified name.
        /// </summary>
        /// <param name="name">The name of the struct the attribute is applied to.</param>
        public ThriftStructAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, () => name );

            Name = name;
        }
    }

    /// <summary>
    /// Required attribute for Thrift method parameters.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    public sealed class ThriftParameterAttribute : Attribute
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
        /// Initializes a new instance of the ThriftParameterAttribute class with the specified ID and name.
        /// </summary>
        /// <param name="id">The ID of the parameter the attribute is applied to. Must be positive.</param>
        /// <param name="name">The name of the parameter the attribute is applied to.</param>
        public ThriftParameterAttribute( short id, string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, () => name );

            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// Optional attribute for Thrift methods specifying a "throws" clause.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class ThriftThrowsAttribute : Attribute
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
        /// Gets the type of the exception specified by the clause.
        /// </summary>
        public Type ExceptionType { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftThrowsAttribute class with the specified values.
        /// </summary>
        /// <param name="id">The ID of the clause defined by the attribute.</param>
        /// <param name="name">The name of the clause defined by the attribute.</param>
        /// <param name="exceptionType">The type of the exception whose clause is defined by the attribute.</param>
        public ThriftThrowsAttribute( short id, string name, Type exceptionType )
        {
            Validation.IsNeitherNullNorWhitespace( name, () => name );
            Validation.IsNotNull( exceptionType, () => exceptionType );

            Id = id;
            Name = name;
            ExceptionType = exceptionType;
        }
    }

    /// <summary>
    /// Required attribute for Thrift methods.
    /// </summary>
    /// <remarks>
    /// Methods without this attribute will be ignored.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class ThriftMethodAttribute : Attribute
    {
        /// <summary>
        /// Gets the method's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the method is a one-way method.
        /// </summary>
        /// <remarks>
        /// One-way methods do not expect or wait for a server reply.
        /// </remarks>
        public bool IsOneWay { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftMethodAttribute class with the specified values.
        /// </summary>
        /// <param name="name">The name of the method the attribute is applied to.</param>
        /// <param name="isOneWay">Whether the method the attribute is applied to is one-way.</param>
        public ThriftMethodAttribute( string name, bool isOneWay = false )
        {
            Validation.IsNeitherNullNorWhitespace( name, () => name );

            Name = name;
            IsOneWay = isOneWay;
        }
    }

    /// <summary>
    /// Required attribute for Thrift services.
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface )]
    public sealed class ThriftServiceAttribute : Attribute
    {
        /// <summary>
        /// Gets the service's name.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftServiceAttribute class with the specified name.
        /// </summary>
        /// <param name="name">The name of the service the attribute is applied to.</param>
        public ThriftServiceAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, () => name );

            Name = name;
        }
    }


    /// <summary>
    /// Optional attribute for Thrift fields, method return values and method parameters.
    /// Specifies a converter to be used when serializing the value.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue )]
    public sealed class ThriftConverterAttribute : Attribute
    {
        /// <summary>
        /// Gets the converter.
        /// </summary>
        public IThriftValueConverter Converter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ThriftConverterAttribute class with the specified converter type.
        /// </summary>
        /// <param name="converterType">The converter type.</param>
        public ThriftConverterAttribute( Type converterType )
        {
            Validation.IsNotNull( converterType, () => converterType );

            var typeInfo = converterType.GetTypeInfo();

            if ( !typeof( IThriftValueConverter ).GetTypeInfo().IsAssignableFrom( typeInfo ) )
            {
                throw new ArgumentException( "The type must inherit from IThriftValueConverter." );
            }

            Converter = (IThriftValueConverter) ReflectionEx.Create( typeInfo );
        }
    }
}