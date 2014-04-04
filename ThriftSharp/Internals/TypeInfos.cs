// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

// TODO Refactor / move / something

namespace ThriftSharp.Internals
{
    internal static class TypeInfos
    {
        public static readonly TypeInfo
            Reader = typeof( ThriftReader ).GetTypeInfo(),
            Writer = typeof( ThriftWriter ).GetTypeInfo(),
            Struct = typeof( ThriftStruct ).GetTypeInfo(),
            Field = typeof( ThriftField ).GetTypeInfo(),
            FieldCollection = typeof( ReadOnlyCollection<ThriftField> ).GetTypeInfo(),
            Option = typeof( Option ).GetTypeInfo(),
            Protocol = typeof( IThriftProtocol ).GetTypeInfo(),
            CollectionHeader = typeof( ThriftCollectionHeader ).GetTypeInfo(),
            MapHeader = typeof( ThriftMapHeader ).GetTypeInfo(),
            FieldHeader = typeof( ThriftFieldHeader ).GetTypeInfo(),
            IEnumerable = typeof( IEnumerable ).GetTypeInfo(),
            IEnumerator = typeof( IEnumerator ).GetTypeInfo(),
            Task = typeof( Task ).GetTypeInfo(),
            Encoding = typeof( Encoding ).GetTypeInfo(),
            AggregateException = typeof( AggregateException ).GetTypeInfo(),
            TaskCanceledException = typeof( TaskCanceledException ).GetTypeInfo(),
            SerializationError = typeof( ThriftSerializationException ).GetTypeInfo();
    }
}