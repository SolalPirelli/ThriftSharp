using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Possible statuses for a wire field.
    /// </summary>
    internal enum ThriftFieldPresenseState
    {
        /// <summary>
        /// The field is guaranteed to always be present.
        /// </summary>
        AlwaysPresent,

        /// <summary>
        /// The field is required, but its presence must be validated.
        /// </summary>
        Required,

        /// <summary>
        /// The field is not required to be present.
        /// </summary>
        Optional
    }

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
        public readonly Type UnderlyingType;

        /// <summary>
        /// Gets the field's presence state.
        /// </summary>
        public readonly ThriftFieldPresenseState State;

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
        /// Initializes a new instance of the ThriftWireField class with the specified values.
        /// </summary>
        private ThriftWireField( short id, string name,
                                 ThriftType wireType, TypeInfo underlyingTypeInfo,
                                 ThriftFieldPresenseState state, object defaultValue,
                                 ThriftConverter converter,
                                 Expression getter, Func<Expression, Expression> setter )
        {
            Id = id;
            Name = name;
            WireType = wireType;
            UnderlyingType = underlyingTypeInfo.AsType();
            State = state;
            DefaultValue = defaultValue;
            Converter = converter;
            Getter = getter;
            Setter = setter;
        }


        /// <summary>
        /// Creates a wire field representing a struct field.
        /// </summary>
        public static ThriftWireField Field( ThriftField field, Expression structVar )
        {
            var propExpr = Expression.Property( structVar, field.BackingProperty );
            return new ThriftWireField( field.Id, field.Name,
                                        field.WireType, field.BackingProperty.PropertyType.GetTypeInfo(),
                                        field.IsRequired ? ThriftFieldPresenseState.Required : ThriftFieldPresenseState.Optional, field.DefaultValue,
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
                                        ThriftFieldPresenseState.AlwaysPresent, null,
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
                                        ThriftFieldPresenseState.Optional, null,
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
                                        ThriftFieldPresenseState.Optional, null,
                                        value.Converter,
                                        returnValueVar, setter );
        }
    }
}