﻿// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp.Models
{
    /// <summary>
    /// Header of Thrift collections (List and Set).
    /// </summary>
    public sealed class ThriftCollectionHeader
    {
        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Gets the collection elements' type ID.
        /// </summary>
        public readonly ThriftTypeId ElementTypeId;


        /// <summary>
        /// Initializes a new instance of the ThriftCollectionHeader class with the specified values.
        /// </summary>
        /// <param name="count">The number of elements.</param>
        /// <param name="elementTypeId">The elements' type ID.</param>
        public ThriftCollectionHeader( int count, ThriftTypeId elementTypeId )
        {
            Count = count;
            ElementTypeId = elementTypeId;
        }
    }

    /// <summary>
    /// Header of Thrift maps.
    /// </summary>
    public sealed class ThriftMapHeader
    {
        /// <summary>
        /// Gets the number of elements in the map.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Gets the map keys' type ID.
        /// </summary>
        public readonly ThriftTypeId KeyTypeId;

        /// <summary>
        /// Gets the map values' type ID.
        /// </summary>
        public readonly ThriftTypeId ValueTypeId;


        /// <summary>
        /// Initializes a new instance of the ThriftMapHeader class with the specified values.
        /// </summary>
        /// <param name="count">The number of elements.</param>
        /// <param name="keyTypeId">The keys' type ID.</param>
        /// <param name="valueTypeId">The values' type ID.</param>
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
    public sealed class ThriftFieldHeader
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
        /// Gets the field's type ID.
        /// </summary>
        public readonly ThriftTypeId TypeId;

        /// <summary>
        /// Initializes a new instance of the ThriftFieldHeader class with the specified values.
        /// </summary>
        /// <param name="id">The field's ID.</param>
        /// <param name="name">The field's name.</param>
        /// <param name="typeId">The field's type ID.</param>
        public ThriftFieldHeader( short id, string name, ThriftTypeId typeId )
        {
            Id = id;
            Name = name;
            TypeId = typeId;
        }
    }

    /// <summary>
    /// Header of Thrift structs.
    /// </summary>
    public sealed class ThriftStructHeader
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
    public sealed class ThriftMessageHeader
    {
        // N.B. Thrift# does not implement message sequence IDs, as it does not need them

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