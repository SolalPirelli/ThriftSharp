using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ThriftSharp.Internals;

namespace ThriftSharp.Utilities
{
    internal static class Constructors
    {
        public static readonly ConstructorInfo ThriftCollectionHeader =
            typeof( ThriftCollectionHeader ).GetTypeInfo().DeclaredConstructors.Single();

        public static readonly ConstructorInfo ThriftMapHeader =
            typeof( ThriftMapHeader ).GetTypeInfo().DeclaredConstructors.Single();

        public static readonly ConstructorInfo ThriftFieldHeader =
            typeof( ThriftFieldHeader ).GetTypeInfo().DeclaredConstructors.Single();

        public static readonly ConstructorInfo ThriftStructHeader =
            typeof( ThriftStructHeader ).GetTypeInfo().DeclaredConstructors.Single();

        public static readonly ConstructorInfo ThriftMessageHeader =
            typeof( ThriftMessageHeader ).GetTypeInfo().DeclaredConstructors.Single();

        public static readonly ConstructorInfo ThriftProtocolException =
         typeof( ThriftProtocolException ).GetTypeInfo().DeclaredConstructors.Single( c => c.GetParameters().Length == 1 );
    }

    internal static class Methods
    {
        public static readonly MethodInfo IEnumerator_MoveNext =
            typeof( IEnumerator ).GetTypeInfo().GetDeclaredMethod( "MoveNext" );

        public static readonly MethodInfo IDisposable_Dispose =
               typeof( IDisposable ).GetTypeInfo().GetDeclaredMethod( "Dispose" );

        public static readonly MethodInfo Enum_IsDefined =
            typeof( Enum ).GetTypeInfo().GetDeclaredMethod( "IsDefined" );

        public static readonly MethodInfo ThriftStructReader_Read =
           typeof( ThriftStructReader ).GetTypeInfo().GetDeclaredMethod( "Read" );
    }

    internal static class TypeInfos
    {
        public static readonly TypeInfo Void =
            typeof( void ).GetTypeInfo();

        public static readonly TypeInfo String =
            typeof( string ).GetTypeInfo();
    }

    internal static class Types
    {
        public static readonly Type[] EmptyTypes = new Type[0];
    }
}