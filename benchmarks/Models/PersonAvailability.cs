using System.Collections.Generic;
using System.Linq;

namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "PersonAvailability" )]
    public sealed class PersonAvailability
    {
        [ThriftField( 1, true, "person" )]
        public Person Person { get; set; }

        [ThriftField( 2, true, "periods" )]
        public List<TimePeriod> Periods { get; set; }


        public static PersonAvailability Sample
        {
            get
            {
                return new PersonAvailability
                {
                    Person = Person.Sample,
                    Periods = new List<TimePeriod>
                    {
                        TimePeriod.Sample,
                        TimePeriod.Sample,
                        TimePeriod.Sample
                    }
                };
            }
        }


        public static explicit operator Thrift.PersonAvailability( PersonAvailability pa )
        {
            return new Thrift.PersonAvailability( (Thrift.Person) pa.Person, pa.Periods.Select( p => (Thrift.TimePeriod) p ).ToList() );
        }
    }
}