using System.Collections.Generic;

namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "MapContainer" )]
    public sealed class MapContainer
    {
        [ThriftField( 1, true, "value" )]
        public Dictionary<int, int> Value { get; set; }


        public static MapContainer Sample
        {
            get { return new MapContainer { Value = new Dictionary<int, int> { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 }, { 9, 10 } } }; }
        }


        public static explicit operator Thrift.MapContainer( MapContainer c )
        {
            return new Thrift.MapContainer( c.Value );
        }
    }
}