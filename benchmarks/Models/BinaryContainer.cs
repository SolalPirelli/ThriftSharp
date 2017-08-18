using System;

namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "BinaryContainer" )]
    public sealed class BinaryContainer
    {
        [ThriftField( 1, true, "value" )]
        public sbyte[] Value { get; set; }


        public static BinaryContainer Sample
        {
            get { return new BinaryContainer { Value = new sbyte[] { 1, 2, 3, 4, 5 } }; }
        }


        public static explicit operator Thrift.BinaryContainer( BinaryContainer c )
        {
            // Thrift for C# uses byte[] for binary (even though it uses sbyte for Thrift bytes)
            byte[] bytes = new byte[c.Value.Length];
            Buffer.BlockCopy( c.Value, 0, bytes, 0, bytes.Length );
            return new Thrift.BinaryContainer( bytes );
        }
    }
}