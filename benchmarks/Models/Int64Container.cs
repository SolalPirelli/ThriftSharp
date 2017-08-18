namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "Int64Container" )]
    public sealed class Int64Container
    {
        [ThriftField( 1, true, "value" )]
        public long Value { get; set; }


        public static Int64Container Sample
        {
            get { return new Int64Container { Value = 123456789 }; }
        }


        public static explicit operator Thrift.Int64Container( Int64Container c )
        {
            return new Thrift.Int64Container( c.Value );
        }
    }
}