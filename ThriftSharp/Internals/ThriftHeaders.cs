// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Header of Thrift collections (List and Set).
    /// </summary>
    internal sealed class ThriftCollectionHeader
    {
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Gets the collection elements' Thrift type.
        /// </summary>
        public readonly ThriftType ElementType;


        /// <summary>
        /// Gets the collection element's Thrift type ID.
        /// </summary>
        public readonly ThriftTypeId ElementTypeId;


        /// <summary>
        /// Initializes a new instance of the ThriftCollectionHeader class with the specified values.
        /// </summary>
        /// <param name="count">The number of elements.</param>
        /// <param name="elementType">The elements' Thrift type.</param>
        public ThriftCollectionHeader( int count, ThriftType elementType )
        {
            Count = count;
            ElementType = elementType;
            ElementTypeId = elementType.Id;
        }

        /// <summary>
        /// Initializes a new instance of the ThriftCollectionHeader class with the specified values.
        /// </summary>
        /// <param name="count">The number of elements.</param>
        /// <param name="elementTypeId">The elements' Thrift type ID.</param>
        public ThriftCollectionHeader( int count, ThriftTypeId elementTypeId )
        {
            Count = count;
            ElementTypeId = elementTypeId;
        }


        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return string.Format( "{0} [{1}]", Count, ElementTypeId );
        }
    }

    /// <summary>
    /// Header of Thrift maps.
    /// </summary>
    internal sealed class ThriftMapHeader
    {
        /// <summary>
        /// Gets the number of elements in the map.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Gets the map keys' Thrift type.
        /// </summary>
        public readonly ThriftType KeyType;

        /// <summary>
        /// Gets the map keys' Thrift type ID.
        /// </summary>
        public readonly ThriftTypeId KeyTypeId;

        /// <summary>
        /// Gets the map values' Thrift type.
        /// </summary>
        public readonly ThriftType ValueType;

        /// <summary>
        /// Gets the map values' Thrift type ID.
        /// </summary>
        public readonly ThriftTypeId ValueTypeId;


        /// <summary>
        /// Initializes a new instance of the ThriftMapHeader class with the specified values.
        /// </summary>
        /// <param name="count">The number of elements.</param>
        /// <param name="keyType">The keys' Thrift type.</param>
        /// <param name="valueType">The values' Thrift type.</param>
        public ThriftMapHeader( int count, ThriftType keyType, ThriftType valueType )
        {
            Count = count;
            KeyType = keyType;
            KeyTypeId = keyType.Id;
            ValueType = valueType;
            ValueTypeId = valueType.Id;
        }

        /// <summary>
        /// Initializes a new instance of the ThriftMapHeader class with the specified values.
        /// </summary>
        /// <param name="count">The number of elements.</param>
        /// <param name="keyTypeId">The keys' Thrift type ID.</param>
        /// <param name="valueTypeId">The values' Thrift type ID.</param>
        public ThriftMapHeader( int count, ThriftTypeId keyTypeId, ThriftTypeId valueTypeId )
        {
            Count = count;
            KeyTypeId = keyTypeId;
            ValueTypeId = valueTypeId;
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return string.Format( "{0} [{1} -> {2}]", Count, KeyTypeId, ValueTypeId );
        }
    }

    /// <summary>
    /// Header of Thrift fields.
    /// </summary>
    internal sealed class ThriftFieldHeader
    {
        /// <summary>
        /// Indicates the end of fields in a struct.
        /// </summary>
        public const byte Stop = 0;

        /// <summary>
        /// Gets the field's ID.
        /// </summary>
        public readonly short Id;

        /// <summary>
        /// Gets the field's name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Gets the field's Thrift type.
        /// </summary>
        public readonly ThriftType FieldType;

        /// <summary>
        /// Gets the field's Thrift type ID.
        /// </summary>
        public readonly ThriftTypeId FieldTypeId;

        /// <summary>
        /// Initializes a new instance of the ThriftFieldHeader class with the specified values.
        /// </summary>
        /// <param name="id">The field ID.</param>
        /// <param name="name">The field name.</param>
        /// <param name="fieldType">The field type.</param>
        public ThriftFieldHeader( short id, string name, ThriftType fieldType )
        {
            Id = id;
            Name = name;
            FieldType = fieldType;
            FieldTypeId = fieldType.Id;
        }

        /// <summary>
        /// Initializes a new instance of the ThriftFieldHeader class with the specified values.
        /// </summary>
        /// <param name="id">The field ID.</param>
        /// <param name="name">The field name.</param>
        /// <param name="fieldTypeId">The field type ID.</param>
        public ThriftFieldHeader( short id, string name, ThriftTypeId fieldTypeId )
        {
            Id = id;
            Name = name;
            FieldTypeId = fieldTypeId;
        }


        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return string.Format( "{0}: {2} {1}", Id, Name, FieldTypeId );
        }
    }

    /// <summary>
    /// Header of Thrift structs.
    /// </summary>
    internal sealed class ThriftStructHeader
    {
        /// <summary>
        /// Gets the struct's name.
        /// </summary>
        public readonly string Name;


        /// <summary>
        /// Initializes a new instance of the ThriftStructHeader class with the specified parameters.
        /// </summary>
        /// <param name="name">The struct name.</param>
        public ThriftStructHeader( string name )
        {
            Name = name;
        }


        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Header of Thrift messages.
    /// </summary>
    internal sealed class ThriftMessageHeader
    {
        /// <summary>
        /// Gets the message's ID.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Gets the message's name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Gets the message's type.
        /// </summary>
        public readonly ThriftMessageType MessageType;


        /// <summary>
        /// Initializes a new instance of the ThriftMessageHeader class with the specified values.
        /// </summary>
        /// <param name="id">The message ID.</param>
        /// <param name="name">The message name.</param>
        /// <param name="messageType">The message type.</param>
        public ThriftMessageHeader( int id, string name, ThriftMessageType messageType )
        {
            Id = id;
            Name = name;
            MessageType = messageType;
        }


        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return string.Format( "{0}: {1} ({2})", Id, Name, MessageType );
        }
    }
}