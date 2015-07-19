namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "ByteContainer" )]
    public sealed class ByteContainer
    {
        [ThriftField( 1, true, "value" )]
        public sbyte Value { get; set; }


        public static ByteContainer Sample
        {
            get { return new ByteContainer { Value = 123 }; }
        }


        public static explicit operator Thrift.ByteContainer( ByteContainer c )
        {
            return new Thrift.ByteContainer( c.Value );
        }
    }
}