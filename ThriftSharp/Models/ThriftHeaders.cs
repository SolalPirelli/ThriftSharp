// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;

namespace ThriftSharp.Models
{
    /// <summary>
    /// Header of Thrift collections (List and Set).
    /// </summary>
    public struct ThriftCollectionHeader : IEquatable<ThriftCollectionHeader>
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


        /// <summary>
        /// Determines whether the header is equal to the specified header.
        /// </summary>
        /// <param name="other">The header to check for equality.</param>
        /// <returns>A value indicating whether the two headers are equal.</returns>
        public bool Equals( ThriftCollectionHeader other )
        {
            return Count == other.Count && ElementTypeId == other.ElementTypeId;
        }

        /// <summary>
        /// Determines whether the header is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return obj is ThriftCollectionHeader && Equals( (ThriftCollectionHeader) obj );
        }

        /// <summary>
        /// Computes a hash of the header.
        /// </summary>
        /// <returns>The hash.</returns>
        public override int GetHashCode()
        {
            return Count.GetHashCode() + 31 * (byte) ElementTypeId;
        }


        /// <summary>
        /// Checks whether the specified headers are equal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are equal.</returns>
        public static bool operator ==( ThriftCollectionHeader left, ThriftCollectionHeader right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Checks whether the specified headers are unequal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are unequal.</returns>
        public static bool operator !=( ThriftCollectionHeader left, ThriftCollectionHeader right )
        {
            return !left.Equals( right );
        }
    }

    /// <summary>
    /// Header of Thrift maps.
    /// </summary>
    public struct ThriftMapHeader : IEquatable<ThriftMapHeader>
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


        /// <summary>
        /// Determines whether the header is equal to the specified header.
        /// </summary>
        /// <param name="other">The header to check for equality.</param>
        /// <returns>A value indicating whether the two headers are equal.</returns>
        public bool Equals( ThriftMapHeader other )
        {
            return Count == other.Count && KeyTypeId == other.KeyTypeId && ValueTypeId == other.ValueTypeId;
        }

        /// <summary>
        /// Determines whether the header is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return obj is ThriftMapHeader && Equals( (ThriftMapHeader) obj );
        }

        /// <summary>
        /// Computes a hash of the header.
        /// </summary>
        /// <returns>The hash.</returns>
        public override int GetHashCode()
        {
            int hash = Count.GetHashCode();
            hash = 31 * hash + (byte) KeyTypeId;
            return 31 * hash + (byte) ValueTypeId;
        }


        /// <summary>
        /// Checks whether the specified headers are equal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are equal.</returns>
        public static bool operator ==( ThriftMapHeader left, ThriftMapHeader right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Checks whether the specified headers are unequal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are unequal.</returns>
        public static bool operator !=( ThriftMapHeader left, ThriftMapHeader right )
        {
            return !left.Equals( right );
        }
    }

    /// <summary>
    /// Header of Thrift fields.
    /// </summary>
    public struct ThriftFieldHeader : IEquatable<ThriftFieldHeader>
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


        /// <summary>
        /// Determines whether the header is equal to the specified header.
        /// </summary>
        /// <param name="other">The header to check for equality.</param>
        /// <returns>A value indicating whether the two headers are equal.</returns>
        public bool Equals( ThriftFieldHeader other )
        {
            return Id == other.Id && Name == other.Name && TypeId == other.TypeId;
        }

        /// <summary>
        /// Determines whether the header is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return obj is ThriftFieldHeader && Equals( (ThriftFieldHeader) obj );
        }

        /// <summary>
        /// Computes a hash of the header.
        /// </summary>
        /// <returns>The hash.</returns>
        public override int GetHashCode()
        {
            int hash = Id.GetHashCode();
            hash = 31 * hash + Name.GetHashCode();
            return 31 * hash + (byte) TypeId;
        }


        /// <summary>
        /// Checks whether the specified headers are equal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are equal.</returns>
        public static bool operator ==( ThriftFieldHeader left, ThriftFieldHeader right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Checks whether the specified headers are unequal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are unequal.</returns>
        public static bool operator !=( ThriftFieldHeader left, ThriftFieldHeader right )
        {
            return !left.Equals( right );
        }
    }

    /// <summary>
    /// Header of Thrift structs.
    /// </summary>
    public struct ThriftStructHeader : IEquatable<ThriftStructHeader>
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
        /// Determines whether the header is equal to the specified header.
        /// </summary>
        /// <param name="other">The header to check for equality.</param>
        /// <returns>A value indicating whether the two headers are equal.</returns>
        public bool Equals( ThriftStructHeader other )
        {
            return Name == other.Name;
        }

        /// <summary>
        /// Determines whether the header is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return obj is ThriftStructHeader && Equals( (ThriftStructHeader) obj );
        }

        /// <summary>
        /// Computes a hash of the header.
        /// </summary>
        /// <returns>The hash.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }


        /// <summary>
        /// Checks whether the specified headers are equal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are equal.</returns>
        public static bool operator ==( ThriftStructHeader left, ThriftStructHeader right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Checks whether the specified headers are unequal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are unequal.</returns>
        public static bool operator !=( ThriftStructHeader left, ThriftStructHeader right )
        {
            return !left.Equals( right );
        }
    }

    /// <summary>
    /// Header of Thrift messages.
    /// </summary>
    public struct ThriftMessageHeader : IEquatable<ThriftMessageHeader>
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


        /// <summary>
        /// Determines whether the header is equal to the specified header.
        /// </summary>
        /// <param name="other">The header to check for equality.</param>
        /// <returns>A value indicating whether the two headers are equal.</returns>
        public bool Equals( ThriftMessageHeader other )
        {
            return Name == other.Name && MessageType == other.MessageType;
        }

        /// <summary>
        /// Determines whether the header is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>A value indicating whether the two objects are equal.</returns>
        public override bool Equals( object obj )
        {
            return obj is ThriftMessageHeader && Equals( (ThriftMessageHeader) obj );
        }

        /// <summary>
        /// Computes a hash of the header.
        /// </summary>
        /// <returns>The hash.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() + 31 * (byte) MessageType;
        }


        /// <summary>
        /// Checks whether the specified headers are equal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are equal.</returns>
        public static bool operator ==( ThriftMessageHeader left, ThriftMessageHeader right )
        {
            return left.Equals( right );
        }

        /// <summary>
        /// Checks whether the specified headers are unequal.
        /// </summary>
        /// <param name="left">The first header.</param>
        /// <param name="right">The second header.</param>
        /// <returns>A value indicating whether the headers are unequal.</returns>
        public static bool operator !=( ThriftMessageHeader left, ThriftMessageHeader right )
        {
            return !left.Equals( right );
        }
    }
}