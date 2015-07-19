using System.Collections.Generic;
using Thrift.Collections;

namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "SetContainer" )]
    public sealed class SetContainer
    {
        [ThriftField( 1, true, "value" )]
        public HashSet<int> Value { get; set; }


        public static SetContainer Sample
        {
            get { return new SetContainer { Value = new HashSet<int> { 1, 2, 3, 4, 5 } }; }
        }


        public static explicit operator Thrift.SetContainer( SetContainer c )
        {
            var tset = new THashSet<int>();
            foreach ( var item in c.Value )
            {
                tset.Add( item );
            }
            return new Thrift.SetContainer( tset );
        }
    }
}