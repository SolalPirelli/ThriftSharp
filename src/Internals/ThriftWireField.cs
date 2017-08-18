// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Component of a Thrift struct; can represent either a real field, or a virtual one.
    /// </summary>
    internal sealed class ThriftWireField
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
        /// Gets the field's type as used when serializing it to the wire.
        /// </summary>
        public readonly ThriftType WireType;

        /// <summary>
        /// Gets the field's type as specified in code.
        /// </summary>
        public readonly TypeInfo UnderlyingTypeInfo;

        /// <summary>
        /// Gets the field's presence state.
        /// </summary>
        public readonly ThriftWireFieldState Kind;

        /// <summary>
        /// Gets the field's default value, if any.
        /// </summary>
        public readonly object DefaultValue;

        /// <summary>
        /// Gets the field's converter, if any.
        /// </summary>
        public readonly ThriftConverter Converter;

        /// <summary>
        /// Gets an expression reading the field, if any.
        /// </summary>
        public readonly Expression Getter;

        /// <summary>
        /// Gets an expression writing its argument to the field, if any.
        /// </summary>
        public readonly Func<Expression, Expression> Setter;

        /// <summary>
        /// Gets an expression testing whether the field's value is null, if any.
        /// </summary>
        public readonly Expression NullChecker;


        /// <summary>
        /// Initializes a new instance of the ThriftWireField class with the specified values.
        /// </summary>
        private ThriftWireField( short id, string name,
                                 ThriftType wireType, TypeInfo underlyingTypeInfo,
                                 ThriftWireFieldState state, object defaultValue,
                                 ThriftConverter converter,
                                 Expression getter, Func<Expression, Expression> setter )
        {
            Id = id;
            Name = name;
            WireType = wireType;
            UnderlyingTypeInfo = underlyingTypeInfo;
            Kind = state;
            DefaultValue = defaultValue;
            Converter = converter;
            Getter = getter;
            Setter = setter;

            // NullChecker is required because UWP doesn't implement Equal/NotEqual on nullables.
            if( getter != null )
            {
                if( Nullable.GetUnderlyingType( underlyingTypeInfo.AsType() ) == null )
                {
                    if( !underlyingTypeInfo.IsValueType )
                    {
                        NullChecker = Expression.Equal(
                            getter,
                            Expression.Constant( null )
                        );
                    }
                }
                else
                {
                    // Can't use HasValue, not supported by UWP's expression interpreter
                    NullChecker = Expression.Call(
                        typeof( object ),
                        "ReferenceEquals",
                        Type.EmptyTypes,
                        Expression.Convert(
                            getter,
                            typeof( object )
                        ),
                        Expression.Constant( null )
                    );
                }
            }
        }


        /// <summary>
        /// Creates a wire field representing a struct field.
        /// </summary>
        public static ThriftWireField Field( ThriftField field, Expression structVar )
        {
            var propExpr = Expression.Property( structVar, field.BackingProperty );
            return new ThriftWireField( field.Id, field.Name,
                                        field.WireType, field.BackingProperty.PropertyType.GetTypeInfo(),
                                        field.IsRequired ? ThriftWireFieldState.Required : ThriftWireFieldState.Optional, field.DefaultValue,
                                        field.Converter,
                                        propExpr, e => Expression.Assign( propExpr, e ) );
        }

        /// <summary>
        /// Creates a virtual wire field representing a method parameter.
        /// </summary>
        public static ThriftWireField Parameter( ThriftParameter param, Expression parametersVar, int index )
        {
            var getterExpr = Expression.Convert(
                Expression.ArrayAccess( parametersVar, Expression.Constant( index ) ),
                param.UnderlyingTypeInfo.AsType()
            );
            return new ThriftWireField( param.Id, param.Name,
                                        param.WireType, param.UnderlyingTypeInfo,
                                        ThriftWireFieldState.AlwaysPresent, null,
                                        param.Converter,
                                        getterExpr, null );
        }

        /// <summary>
        /// Creates a virtual wire field representing a method "throws" clause.
        /// </summary>
        public static ThriftWireField ThrowsClause( ThriftThrowsClause clause )
        {
            return new ThriftWireField( clause.Id, clause.Name,
                                        clause.WireType, clause.UnderlyingTypeInfo,
                                        ThriftWireFieldState.Optional, null,
                                        clause.Converter,
                                        null, Expression.Throw );
        }

        /// <summary>
        /// Creates a virtual wire field representing a return value.
        /// </summary>
        public static ThriftWireField ReturnValue( ThriftReturnValue value, Expression returnValueVar, Expression hasReturnValueVar )
        {
            Func<Expression, Expression> setter = e => Expression.Block(
                Expression.Assign( returnValueVar, e ),
                Expression.Assign( hasReturnValueVar, Expression.Constant( true ) )
            );
            return new ThriftWireField( 0, null,
                                        value.WireType, value.UnderlyingTypeInfo,
                                        ThriftWireFieldState.Optional, null,
                                        value.Converter,
                                        returnValueVar, setter );
        }
    }
}