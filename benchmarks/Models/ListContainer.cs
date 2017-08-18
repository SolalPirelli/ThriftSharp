using System.Collections.Generic;

namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "ListContainer" )]
    public sealed class ListContainer
    {
        [ThriftField( 1, true, "value" )]
        public List<int> Value { get; set; }


        public static ListContainer Sample
        {
            get { return new ListContainer { Value = new List<int> { 1, 2, 3, 4, 5 } }; }
        }


        public static explicit operator Thrift.ListContainer( ListContainer c )
        {
            return new Thrift.ListContainer( c.Value );
        }
    }
}