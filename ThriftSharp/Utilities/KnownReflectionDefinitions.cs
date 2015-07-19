// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Linq;
using System.Reflection;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;

namespace ThriftSharp.Utilities
{
    internal static class TypeInfos
    {
        public static readonly TypeInfo
            Void = typeof( void ).GetTypeInfo(),
            String = typeof( string ).GetTypeInfo(),
            IThriftProtocol = typeof( IThriftProtocol ).GetTypeInfo(),
            ThriftStructReader = typeof( ThriftStructReader ).GetTypeInfo(),
            ThriftStructHeader = typeof( ThriftStructHeader ).GetTypeInfo(),
            ThriftFieldHeader = typeof( ThriftFieldHeader ).GetTypeInfo(),
            ThriftMapHeader = typeof( ThriftMapHeader ).GetTypeInfo(),
            ThriftCollectionHeader = typeof( ThriftCollectionHeader ).GetTypeInfo(),
            ThriftProtocolException = typeof( ThriftProtocolException ).GetTypeInfo(),
            ThriftSerializationException = typeof( ThriftSerializationException ).GetTypeInfo();
    }


    internal static class Constructors
    {
        public static readonly ConstructorInfo
            ThriftCollectionHeader = TypeInfos.ThriftCollectionHeader.DeclaredConstructors.Single(),
            ThriftMapHeader = TypeInfos.ThriftMapHeader.DeclaredConstructors.Single(),
            ThriftFieldHeader = TypeInfos.ThriftFieldHeader.DeclaredConstructors.Single(),
            ThriftStructHeader = TypeInfos.ThriftStructHeader.DeclaredConstructors.Single(),
            ThriftMessageHeader = typeof( ThriftMessageHeader ).GetTypeInfo().DeclaredConstructors.Single(),
            ThriftProtocolException = typeof( ThriftProtocolException ).GetTypeInfo().DeclaredConstructors.Single( c => c.GetParameters().Length == 1 );
    }

    internal static class Methods
    {
        public static readonly MethodInfo
            IDisposable_Dispose = typeof( IDisposable ).GetTypeInfo().GetDeclaredMethod( "Dispose" ),

            Enum_IsDefined = typeof( Enum ).GetTypeInfo().GetDeclaredMethod( "IsDefined" ),

            ThriftStructReader_Skip = TypeInfos.ThriftStructReader.GetDeclaredMethod( "Skip" ),

            ThriftSerializationException_TypeIdMismatch = TypeInfos.ThriftSerializationException.GetDeclaredMethod( "TypeIdMismatch" ),
            ThriftSerializationException_MissingRequiredField = TypeInfos.ThriftSerializationException.GetDeclaredMethod( "MissingRequiredField" ),
            ThriftSerializationException_RequiredFieldIsNull = TypeInfos.ThriftSerializationException.GetDeclaredMethod( "RequiredFieldIsNull" ),

            IThriftProtocol_ReadBoolean = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadBoolean" ),
            IThriftProtocol_ReadSByte = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadSByte" ),
            IThriftProtocol_ReadDouble = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadDouble" ),
            IThriftProtocol_ReadInt16 = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadInt16" ),
            IThriftProtocol_ReadInt32 = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadInt32" ),
            IThriftProtocol_ReadInt64 = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadInt64" ),
            IThriftProtocol_ReadString = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadString" ),
            IThriftProtocol_ReadBinary = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadBinary" ),
            IThriftProtocol_ReadMessageHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadMessageHeader" ),
            IThriftProtocol_ReadMessageEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadMessageEnd" ),
            IThriftProtocol_ReadStructHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadStructHeader" ),
            IThriftProtocol_ReadStructEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadStructEnd" ),
            IThriftProtocol_ReadFieldHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadFieldHeader" ),
            IThriftProtocol_ReadFieldEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadFieldEnd" ),
            IThriftProtocol_ReadMapHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadMapHeader" ),
            IThriftProtocol_ReadMapEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadMapEnd" ),
            IThriftProtocol_ReadListHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadListHeader" ),
            IThriftProtocol_ReadListEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadListEnd" ),
            IThriftProtocol_ReadSetHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadSetHeader" ),
            IThriftProtocol_ReadSetEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "ReadSetEnd" ),

            IThriftProtocol_WriteBoolean = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteBoolean" ),
            IThriftProtocol_WriteSByte = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteSByte" ),
            IThriftProtocol_WriteDouble = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteDouble" ),
            IThriftProtocol_WriteInt16 = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteInt16" ),
            IThriftProtocol_WriteInt32 = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteInt32" ),
            IThriftProtocol_WriteInt64 = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteInt64" ),
            IThriftProtocol_WriteString = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteString" ),
            IThriftProtocol_WriteBinary = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteBinary" ),
            IThriftProtocol_WriteMessageHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteMessageHeader" ),
            IThriftProtocol_WriteMessageEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteMessageEnd" ),
            IThriftProtocol_WriteStructHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteStructHeader" ),
            IThriftProtocol_WriteStructEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteStructEnd" ),
            IThriftProtocol_WriteFieldHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteFieldHeader" ),
            IThriftProtocol_WriteFieldStop = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteFieldStop" ),
            IThriftProtocol_WriteFieldEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteFieldEnd" ),
            IThriftProtocol_WriteMapHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteMapHeader" ),
            IThriftProtocol_WriteMapEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteMapEnd" ),
            IThriftProtocol_WriteListHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteListHeader" ),
            IThriftProtocol_WriteListEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteListEnd" ),
            IThriftProtocol_WriteSetHeader = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteSetHeader" ),
            IThriftProtocol_WriteSetEnd = TypeInfos.IThriftProtocol.GetDeclaredMethod( "WriteSetEnd" );
    }

    internal static class Fields
    {
        public static readonly FieldInfo
            ThriftMessageHeader_MessageType = typeof( ThriftMessageHeader ).GetTypeInfo().GetDeclaredField( "MessageType" ),
            ThriftMapHeader_KeyTypeId = TypeInfos.ThriftMapHeader.GetDeclaredField( "KeyTypeId" ),
            ThriftMapHeader_ValueTypeId = TypeInfos.ThriftMapHeader.GetDeclaredField( "ValueTypeId" ),
            ThriftMapHeader_Count = TypeInfos.ThriftMapHeader.GetDeclaredField( "Count" ),
            ThriftCollectionHeader_ElementTypeId = TypeInfos.ThriftCollectionHeader.GetDeclaredField( "ElementTypeId" ),
            ThriftCollectionHeader_Count = TypeInfos.ThriftCollectionHeader.GetDeclaredField( "Count" ),
            ThriftFieldHeader_Id = TypeInfos.ThriftFieldHeader.GetDeclaredField( "Id" ),
            ThriftFieldHeader_TypeId = TypeInfos.ThriftFieldHeader.GetDeclaredField( "TypeId" );
    }

    internal static class Types
    {
        public static readonly Type[] None = new Type[0];
    }
}