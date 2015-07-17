// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Header of Thrift collections (List and Set).
    /// </summary>
    internal struct ThriftCollectionHeader
    {
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Gets the collection element's Thrift type ID.
        /// </summary>
        public readonly ThriftTypeId ElementTypeId;


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
    }

    /// <summary>
    /// Header of Thrift maps.
    /// </summary>
    internal struct ThriftMapHeader
    {
        /// <summary>
        /// Gets the number of elements in the map.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Gets the map keys' Thrift type ID.
        /// </summary>
        public readonly ThriftTypeId KeyTypeId;

        /// <summary>
        /// Gets the map values' Thrift type ID.
        /// </summary>
        public readonly ThriftTypeId ValueTypeId;


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
    }

    /// <summary>
    /// Header of Thrift fields.
    /// </summary>
    internal struct ThriftFieldHeader
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
            FieldType = null;
            FieldTypeId = fieldTypeId;
        }

        public bool IsEmpty()
        {
            return Id == 0 && Name == null && FieldType == null && FieldTypeId == 0;
        }
    }

    /// <summary>
    /// Header of Thrift structs.
    /// </summary>
    internal struct ThriftStructHeader
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
    }

    /// <summary>
    /// Header of Thrift messages.
    /// </summary>
    internal struct ThriftMessageHeader
    {
        // N.B. Thrift# does not implement message sequence IDs, as they aren't useful in languages with async/await

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
        /// <param name="name">The message name.</param>
        /// <param name="messageType">The message type.</param>
        public ThriftMessageHeader( string name, ThriftMessageType messageType )
        {
            Name = name;
            MessageType = messageType;
        }
    }
}