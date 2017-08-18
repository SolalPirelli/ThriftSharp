using System;
using System.Collections.Generic;
using System.Linq;
using Thrift.Collections;

namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "AllTypesContainer" )]
    public sealed class AllTypesContainer
    {
        [ThriftField( 1, true, "Value1" )]
        public bool Value1 { get; set; }

        [ThriftField( 2, true, "Value2" )]
        public sbyte Value2 { get; set; }

        [ThriftField( 3, true, "Value3" )]
        public double Value3 { get; set; }

        [ThriftField( 4, true, "Value4" )]
        public short Value4 { get; set; }

        [ThriftField( 5, true, "Value5" )]
        public int Value5 { get; set; }

        [ThriftField( 6, true, "Value6" )]
        public long Value6 { get; set; }

        [ThriftField( 7, true, "Value7" )]
        public sbyte[] Value7 { get; set; }

        [ThriftField( 8, true, "Value8" )]
        public string Value8 { get; set; }

        [ThriftField( 9, true, "Value9" )]
        public EmptyStruct Value9 { get; set; }

        [ThriftField( 10, true, "Value10" )]
        public Dictionary<int, int> Value10 { get; set; }

        [ThriftField( 11, true, "Value11" )]
        public HashSet<int> Value11 { get; set; }

        [ThriftField( 12, true, "Value12" )]
        public List<int> Value12 { get; set; }


        public static AllTypesContainer Sample
        {
            get
            {
                return new AllTypesContainer
                {
                    Value1 = true,
                    Value2 = 123,
                    Value3 = 123.456,
                    Value4 = 12345,
                    Value5 = 1234567,
                    Value6 = 123456789,
                    Value7 = Enumerable.Repeat<sbyte>( 42, 100 ).ToArray(),
                    Value8 = string.Join( ", ", Enumerable.Repeat( "Test", 100 ) ),
                    Value9 = new EmptyStruct(),
                    Value10 = Enumerable.Range( 1, 200 ).ToDictionary( n => n, n => n + 10000 ),
                    Value11 = new HashSet<int>( Enumerable.Range( 100, 200 ) ),
                    Value12 = Enumerable.Range( 1000, 300 ).ToList()
                };
            }
        }


        public static explicit operator Thrift.AllTypesContainer( AllTypesContainer c )
        {
            var value7 = new byte[c.Value7.Length];
            Buffer.BlockCopy( c.Value7, 0, value7, 0, value7.Length );

            var value11 = new THashSet<int>();
            foreach( var item in c.Value11 )
            {
                value11.Add( item );
            }

            return new Thrift.AllTypesContainer( c.Value1, c.Value2, c.Value3, c.Value4, c.Value5, c.Value6, value7, c.Value8, (Thrift.EmptyStruct) c.Value9, c.Value10, value11, c.Value12 );
        }
    }
}