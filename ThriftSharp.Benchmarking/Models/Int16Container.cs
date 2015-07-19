namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "Int16Container" )]
    public sealed class Int16Container
    {
        [ThriftField( 1, true, "value" )]
        public short Value { get; set; }


        public static Int16Container Sample
        {
            get { return new Int16Container { Value = 123 }; }
        }


        public static explicit operator Thrift.Int16Container( Int16Container c )
        {
            return new Thrift.Int16Container( c.Value );
        }
    }
}