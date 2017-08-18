namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "Int32Container" )]
    public sealed class Int32Container
    {
        [ThriftField( 1, true, "value" )]
        public int Value { get; set; }


        public static Int32Container Sample
        {
            get { return new Int32Container { Value = 123456 }; }
        }


        public static explicit operator Thrift.Int32Container( Int32Container c )
        {
            return new Thrift.Int32Container( c.Value );
        }
    }
}