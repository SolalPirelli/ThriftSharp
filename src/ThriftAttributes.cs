// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.Collections.Generic;
using System.Reflection;
using ThriftSharp.Internals;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Base class for attributes that can specify a converter to be used when serializing values.
    /// </summary>
    public abstract class ThriftConvertibleAttribute : Attribute
    {
        private static readonly Dictionary<Type, ThriftConverter> _knownConverters = new Dictionary<Type, ThriftConverter>();

        private Type _converter;

        /// <summary>
        /// Gets the converter's type.
        /// </summary>
        public Type Converter
        {
            get { return _converter; }
            set
            {
                if( value == null )
                {
                    _converter = null;
                    return;
                }

                if( !_knownConverters.ContainsKey( value ) )
                {
                    _knownConverters.Add( value, new ThriftConverter( value ) );
                }

                _converter = value;
            }
        }

        /// <summary>
        /// Gets the converter.
        /// </summary>
        internal ThriftConverter ThriftConverter
        {
            get { return Converter == null ? null : _knownConverters[Converter]; }
        }


        /// <summary>
        /// Initializes a new instance of the ThriftConvertibleAttribute class, ensuring only Thrift# classes can inherit from it.
        /// </summary>
        internal ThriftConvertibleAttribute() { }
    }

    /// <summary>
    /// Required attribute marking enums as Thrift enums.
    /// </summary>
    /// <remarks>
    /// This is only a marker attribute that ensures the wrong enums are not involuntarily used.
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
    /// Required attribute marking properties as Thrift fields.
    /// Properties without this attribute will be ignored.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property )]
    public sealed class ThriftFieldAttribute : ThriftConvertibleAttribute
    {
        /// <summary>
        /// Gets the Thrift field's ID.
        /// </summary>
        public short Id { get; }

        /// <summary>
        /// Gets a value indicating whether the field is required.
        /// </summary>
        /// <remarks>
        /// If the Thrift field is required, an exception will be thrown if it is not set during serialization.
        /// </remarks>
        public bool IsRequired { get; }

        /// <summary>
        /// Gets the Thrift field's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the field's default value.
        /// </summary>
        public object DefaultValue { get; set; }


        /// <summary>
        /// Initializes a new instance of the ThriftFieldAttribute class with the specified values.
        /// </summary>
        /// <param name="id">The Thrift ID of the field the attribute is applied to.</param>
        /// <param name="isRequired">Whether the field the attribute is applied to is required.</param>
        /// <param name="name">The Thrift name of the field the attribute is applied to.</param>
        public ThriftFieldAttribute( short id, bool isRequired, string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Id = id;
            IsRequired = isRequired;
            Name = name;
        }
    }

    /// <summary>
    /// Required attribute marking classes as Thrift structs.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class ThriftStructAttribute : Attribute
    {
        /// <summary>
        /// Gets the Thrift struct's name.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Initializes a new instance of the ThriftStructAttribute class with the specified name.
        /// </summary>
        /// <param name="name">The Thrift name of the struct the attribute is applied to.</param>
        public ThriftStructAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Name = name;
        }
    }

    /// <summary>
    /// Required attribute marking Thrift method parameters.
    /// </summary>
    [AttributeUsage( AttributeTargets.Parameter )]
    public sealed class ThriftParameterAttribute : ThriftConvertibleAttribute
    {
        /// <summary>
        /// Gets the parameter's ID.
        /// </summary>
        public short Id { get; }

        /// <summary>
        /// Gets the parameter's name.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Initializes a new instance of the ThriftParameterAttribute class with the specified ID and name.
        /// </summary>
        /// <param name="id">The Thrift ID of the parameter the attribute is applied to.</param>
        /// <param name="name">The Thrift name of the parameter the attribute is applied to.</param>
        public ThriftParameterAttribute( short id, string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// Optional attribute marking methods to specify a Thrift "throws" clause.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class ThriftThrowsAttribute : ThriftConvertibleAttribute
    {
        /// <summary>
        /// Gets the clause's ID.
        /// </summary>
        public short Id { get; }

        /// <summary>
        /// Gets the clause's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the exception specified by the clause.
        /// </summary>
        public TypeInfo ExceptionTypeInfo { get; }


        /// <summary>
        /// Initializes a new instance of the ThriftThrowsAttribute class with the specified values.
        /// </summary>
        /// <param name="id">The Thrift ID of the clause defined by the attribute.</param>
        /// <param name="name">The Thrift name of the clause defined by the attribute.</param>
        /// <param name="exceptionType">The type of the exception whose clause is defined by the attribute.</param>
        public ThriftThrowsAttribute( short id, string name, Type exceptionType )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );
            Validation.IsNotNull( exceptionType, nameof( exceptionType ) );

            Id = id;
            Name = name;
            ExceptionTypeInfo = exceptionType.GetTypeInfo();
        }
    }

    /// <summary>
    /// Required attribute marking methods as Thrift methods.
    /// Methods without this attribute will be ignored.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class ThriftMethodAttribute : ThriftConvertibleAttribute
    {
        /// <summary>
        /// Gets the method's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the method is a one-way method.
        /// One-way methods are sent by the client but not replied to by the server.
        /// </summary>
        public bool IsOneWay { get; set; }


        /// <summary>
        /// Initializes a new instance of the ThriftMethodAttribute class with the specified name.
        /// </summary>
        /// <param name="name">The Thrift name of the method the attribute is applied to.</param>
        public ThriftMethodAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Name = name;
        }
    }

    /// <summary>
    /// Required attribute marking interfaces as Thrift services.
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface )]
    public sealed class ThriftServiceAttribute : Attribute
    {
        /// <summary>
        /// Gets the service's name.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Initializes a new instance of the ThriftServiceAttribute class with the specified name.
        /// </summary>
        /// <param name="name">The Thrift name of the service the attribute is applied to.</param>
        public ThriftServiceAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Name = name;
        }
    }
}