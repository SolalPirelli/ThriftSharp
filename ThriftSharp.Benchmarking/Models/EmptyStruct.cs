namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "EmptyStruct" )]
    public sealed class EmptyStruct
    {
        public static EmptyStruct Sample
        {
            get { return new EmptyStruct(); }
        }


        public static explicit operator Thrift.EmptyStruct( EmptyStruct c )
        {
            return new Thrift.EmptyStruct();
        }
    }
}