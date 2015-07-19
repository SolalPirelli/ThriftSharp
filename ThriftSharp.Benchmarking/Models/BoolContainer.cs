namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "BoolContainer" )]
    public sealed class BoolContainer
    {
        [ThriftField( 1, true, "value" )]
        public bool Value { get; set; }


        public static BoolContainer Sample
        {
            get { return new BoolContainer { Value = true }; }
        }


        public static explicit operator Thrift.BoolContainer( BoolContainer c )
        {
            return new Thrift.BoolContainer( c.Value );
        }
    }
}