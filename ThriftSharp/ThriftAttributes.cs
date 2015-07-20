// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Specifies a converter to be used when serializing the value.
    /// </summary>
    public abstract class ThriftConvertibleAttribute : Attribute
    {
        private static readonly Dictionary<Type, object> _knownConverters = new Dictionary<Type, object>();

        private Type _converter;

        /// <summary>
        /// Gets the converter's type.
        /// </summary>
        public Type Converter
        {
            get { return _converter; }
            set
            {
                Validation.IsNotNull( value, nameof( value ) );

                if ( !_knownConverters.ContainsKey( value ) )
                {
                    var typeInfo = value.GetTypeInfo();
                    var iface = typeInfo.GetGenericInterface( typeof( IThriftValueConverter<,> ) );
                    if ( iface == null )
                    {
                        throw new ArgumentException( "The type must implement IThriftValueConverter." );
                    }

                    var ctor = typeInfo.DeclaredConstructors.FirstOrDefault( c => c.GetParameters().Length == 0 );
                    if ( ctor == null )
                    {
                        throw new ArgumentException( "The type must have a parameterless constructor." );
                    }

                    _knownConverters.Add( value, ctor.Invoke( null ) );
                }

                _converter = value;
                ConverterInstance = _knownConverters[value];
            }
        }

        /// <summary>
        /// Gets the converter.
        /// </summary>
        internal object ConverterInstance { get; private set; }
    }

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
        /// <param name="id">The ID of the field the attribute is applied to. Must be positive.</param>
        /// <param name="isRequired">Whether the field the attribute is applied to is required.</param>
        /// <param name="name">The name of the field the attribute is applied to.</param>
        public ThriftFieldAttribute( short id, bool isRequired, string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Id = id;
            IsRequired = isRequired;
            Name = name;
        }
    }

    /// <summary>
    /// Required attribute for Thrift structs.
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
        /// <param name="name">The name of the struct the attribute is applied to.</param>
        public ThriftStructAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Name = name;
        }
    }

    /// <summary>
    /// Required attribute for Thrift method parameters.
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
        /// <param name="id">The ID of the parameter the attribute is applied to. Must be positive.</param>
        /// <param name="name">The name of the parameter the attribute is applied to.</param>
        public ThriftParameterAttribute( short id, string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// Optional attribute for Thrift methods specifying a "throws" clause.
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
        /// <param name="id">The ID of the clause defined by the attribute.</param>
        /// <param name="name">The name of the clause defined by the attribute.</param>
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
    /// Required attribute for Thrift methods.
    /// </summary>
    /// <remarks>
    /// Methods without this attribute will be ignored.
    /// </remarks>
    [AttributeUsage( AttributeTargets.Method )]
    public sealed class ThriftMethodAttribute : ThriftConvertibleAttribute
    {
        /// <summary>
        /// Gets the method's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the method is a one-way method.
        /// </summary>
        /// <remarks>
        /// One-way methods are sent by the client but not replied to by the server.
        /// </remarks>
        public bool IsOneWay { get; set; }


        /// <summary>
        /// Initializes a new instance of the ThriftMethodAttribute class with the specified values.
        /// </summary>
        /// <param name="name">The name of the method the attribute is applied to.</param>
        public ThriftMethodAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Name = name;
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
        public string Name { get; }


        /// <summary>
        /// Initializes a new instance of the ThriftServiceAttribute class with the specified name.
        /// </summary>
        /// <param name="name">The name of the service the attribute is applied to.</param>
        public ThriftServiceAttribute( string name )
        {
            Validation.IsNeitherNullNorWhitespace( name, nameof( name ) );

            Name = name;
        }
    }
}