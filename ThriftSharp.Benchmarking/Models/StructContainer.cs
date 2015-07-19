namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "StructContainer" )]
    public sealed class StructContainer
    {
        [ThriftField( 1, true, "value" )]
        public EmptyStruct Value { get; set; }


        public static StructContainer Sample
        {
            get { return new StructContainer { Value = EmptyStruct.Sample }; }
        }


        public static explicit operator Thrift.StructContainer( StructContainer c )
        {
            return new Thrift.StructContainer( (Thrift.EmptyStruct) c.Value );
        }
    }
}