namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "StringContainer" )]
    public sealed class StringContainer
    {
        [ThriftField( 1, true, "value" )]
        public string Value { get; set; }


        public static StringContainer Sample
        {
            get { return new StringContainer { Value = "Hello, World!" }; }
        }


        public static explicit operator Thrift.StringContainer( StringContainer c )
        {
            return new Thrift.StringContainer( c.Value );
        }
    }
}