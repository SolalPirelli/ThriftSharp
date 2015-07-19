using System;
using System.Linq.Expressions;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Component of a Thrift struct; can represent either a real field, or a virtual one.
    /// </summary>
    internal sealed class ThriftWireField
    {
        public readonly short Id;

        public readonly string Name;

        public readonly ThriftType WireType;

        public readonly bool IsRequired;

        public readonly object DefaultValue;

        public readonly object Converter;

        public readonly Expression Getter;

        public readonly Func<Expression, Expression> Setter;

        public readonly Type UnderlyingType;

        private ThriftWireField( short id, string name, Type type,
                                 bool isRequired, object defaultValue,
                                 object converter,
                                 Expression getter, Func<Expression, Expression> setter )
        {
            Type wireType;
            if ( converter == null )
            {
                wireType = type;
            }
            else
            {
                wireType = converter.GetType().GetTypeInfo().GetGenericInterface( typeof( IThriftValueConverter<,> ) ).GenericTypeArguments[0];
                var nullableType = Nullable.GetUnderlyingType( type );
                if ( nullableType != null )
                {
                    wireType = typeof( Nullable<> ).MakeGenericType( new[] { wireType } );
                }
            }

            Id = id;
            Name = name;
            WireType = ThriftType.Get( wireType );
            IsRequired = isRequired;
            DefaultValue = defaultValue;
            Converter = converter;
            Getter = getter;
            Setter = setter;
            UnderlyingType = type;
        }


        public static ThriftWireField Field( ThriftField field, Expression structVar )
        {
            var propExpr = Expression.Property( structVar, field.BackingProperty );
            return new ThriftWireField( field.Id, field.Name, field.BackingProperty.PropertyType,
                                        field.IsRequired, field.DefaultValue,
                                        field.Converter,
                                        propExpr, e => Expression.Assign( propExpr, e ) );
        }

        public static ThriftWireField Parameter( ThriftParameter param, Expression parametersVar, int index )
        {
            var getterExpr = Expression.Convert(
                Expression.ArrayAccess( parametersVar, Expression.Constant( index ) ),
                param.TypeInfo.AsType()
            );
            return new ThriftWireField( param.Id, param.Name, param.TypeInfo.AsType(),
                                        false, null,
                                        param.Converter,
                                        getterExpr, null );
        }

        public static ThriftWireField ThrowsClause( ThriftThrowsClause clause )
        {
            return new ThriftWireField( clause.Id, clause.Name, clause.TypeInfo.AsType(),
                                        false, null,
                                        null,
                                        null, Expression.Throw );
        }

        public static ThriftWireField ReturnValue( ThriftReturnValue value, Expression returnValueVar, Expression hasReturnValueVar )
        {
            Func<Expression, Expression> setter = e => Expression.Block(
                Expression.Assign( returnValueVar, e ),
                Expression.Assign( hasReturnValueVar, Expression.Constant( true ) )
            );
            return new ThriftWireField( 0, null, value.TypeInfo.AsType(),
                                        false, null,
                                        value.Converter,
                                        returnValueVar, setter );
        }
    }
}