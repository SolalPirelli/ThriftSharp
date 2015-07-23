using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ThriftSharp.Internals
{
    internal enum ThriftWireFieldState
    {
        AlwaysPresent,
        Required,
        Optional
    }

    /// <summary>
    /// Component of a Thrift struct; can represent either a real field, or a virtual one.
    /// </summary>
    internal sealed class ThriftWireField
    {
        public readonly short Id;

        public readonly string Name;

        public readonly ThriftType WireType;

        public readonly Type UnderlyingType;

        public readonly ThriftWireFieldState State;

        public readonly object DefaultValue;

        public readonly ThriftConverter Converter;

        public readonly Expression Getter;

        public readonly Func<Expression, Expression> Setter;

        private ThriftWireField( short id, string name,
                                 ThriftType wireType, TypeInfo underlyingTypeInfo,
                                 ThriftWireFieldState state, object defaultValue,
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


        public static ThriftWireField Field( ThriftField field, Expression structVar )
        {
            var propExpr = Expression.Property( structVar, field.BackingProperty );
            return new ThriftWireField( field.Id, field.Name,
                                        field.WireType, field.BackingProperty.PropertyType.GetTypeInfo(),
                                        field.IsRequired ? ThriftWireFieldState.Required : ThriftWireFieldState.Optional, field.DefaultValue,
                                        field.Converter,
                                        propExpr, e => Expression.Assign( propExpr, e ) );
        }

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

        public static ThriftWireField ThrowsClause( ThriftThrowsClause clause )
        {
            return new ThriftWireField( clause.Id, clause.Name,
                                        clause.WireType, clause.UnderlyingTypeInfo,
                                        ThriftWireFieldState.Optional, null,
                                        clause.Converter,
                                        null, Expression.Throw );
        }

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