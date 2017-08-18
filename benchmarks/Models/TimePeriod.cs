
namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "TimePeriod" )]
    public sealed class TimePeriod
    {
        [ThriftField( 1, true, "from" )]
        public long From { get; set; }

        [ThriftField( 2, true, "to" )]
        public long To { get; set; }

        [ThriftField( 3, true, "available" )]
        public bool Available { get; set; }


        public static TimePeriod Sample
        {
            get
            {
                return new TimePeriod { From = 1437300000, To = 1437309000, Available = true };
            }
        }


        public static explicit operator Thrift.TimePeriod( TimePeriod p )
        {
            return new Thrift.TimePeriod( p.From, p.To, p.Available );
        }
    }
}