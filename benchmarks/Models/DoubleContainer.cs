namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "DoubleContainer" )]
    public sealed class DoubleContainer
    {
        [ThriftField( 1, true, "value" )]
        public double Value { get; set; }


        public static DoubleContainer Sample
        {
            get { return new DoubleContainer { Value = 123.456 }; }
        }


        public static explicit operator Thrift.DoubleContainer( DoubleContainer c )
        {
            return new Thrift.DoubleContainer( c.Value );
        }
    }
}