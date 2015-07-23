using System;
using System.Reflection;

namespace ThriftSharp.Internals
{
    public sealed class ThriftConverter
    {
        public object Value { get; }
        public TypeInfo InterfaceTypeInfo { get; }

        public Type FromType
        {
            get { return InterfaceTypeInfo.GenericTypeArguments[0]; }
        }

        public Type ToType
        {
            get { return InterfaceTypeInfo.GenericTypeArguments[1]; }
        }

        public ThriftConverter( object value, TypeInfo interfaceTypeInfo )
        {
            Value = value;
            InterfaceTypeInfo = interfaceTypeInfo;
        }
    }
}