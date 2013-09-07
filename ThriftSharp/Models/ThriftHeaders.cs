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
        public int Count { get; private set; }

        /// <summary>
        /// Gets the collection elements' Thrift type.
        /// </summary>
        public ThriftType ElementType { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftCollectionHeader class with the specified values.
        /// </summary>
        /// <param name="count">The number of elements.</param>
        /// <param name="elementType">The elements' Thrift type.</param>
        public ThriftCollectionHeader( int count, ThriftType elementType )
        {
            Count = count;
            ElementType = elementType;
        }


        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return string.Format( "{0} [{1}]", Count, ElementType );
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
        public int Count { get; private set; }

        /// <summary>
        /// Gets the map keys' Thrift type.
        /// </summary>
        public ThriftType KeyType { get; private set; }

        /// <summary>
        /// Gets the map values' Thrift type.
        /// </summary>
        public ThriftType ValueType { get; private set; }


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
            ValueType = valueType;
        }


        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return string.Format( "{0} [{1} -> {2}]", Count, KeyType, ValueType );
        }
    }

    /// <summary>
    /// Header of Thrift fields.
    /// </summary>
    public sealed class ThriftFieldHeader
    {
        /// <summary>
        /// Indicates the end of fields in a struct.
        /// </summary>
        public const byte Stop = 0;

        /// <summary>
        /// Gets the field's ID.
        /// </summary>
        public short Id { get; private set; }

        /// <summary>
        /// Gets the field's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the field's Thrift type.
        /// </summary>
        public ThriftType FieldType { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftFieldHeader class with the specified values.
        /// </summary>
        /// <param name="id">The field ID.</param>
        /// <param name="name">The field name.</param>
        /// <param name="fieldType">The field Thrift type.</param>
        public ThriftFieldHeader( short id, string name, ThriftType fieldType )
        {
            Id = id;
            Name = name;
            FieldType = fieldType;
        }


        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return string.Format( "{0}: {2} {1}", Id, Name, FieldType );
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
        public string Name { get; private set; }


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
    public sealed class ThriftMessageHeader
    {
        /// <summary>
        /// Gets the message's ID.
        /// </summary>
        /// <remarks>
        /// Usage is not clear.
        /// </remarks>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the message's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the message's type.
        /// </summary>
        public ThriftMessageType MessageType { get; private set; }


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