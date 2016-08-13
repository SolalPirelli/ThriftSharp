using System.Collections.Generic;

namespace ThriftSharp.Benchmarking.Models
{
    [ThriftStruct( "Person" )]
    public sealed class Person
    {
        [ThriftField( 1, true, "firstName" )]
        public string FirstName { get; set; }

        [ThriftField( 2, true, "middleNames" )]
        public List<string> MiddleNames { get; set; }

        [ThriftField( 3, false, "lastName" )]
        public string LastName { get; set; }

        [ThriftField( 4, false, "age" )]
        public int? Age { get; set; }

        [ThriftField( 5, true, "email" )]
        public string Email { get; set; }

        [ThriftField( 6, true, "websites" )]
        public List<string> Websites { get; set; }


        public static Person Sample
        {
            get
            {
                return new Person
                {
                    FirstName = "Pablo",
                    MiddleNames = new List<string>
                    {
                        "Diego",
                        "José",
                        "Francisco de Paula",
                        "Juan",
                        "Nepomuceno",
                        "María de los Remedios",
                        "Cipriano de la Santísima Trinidad"
                    },
                    LastName = "Ruiz y Picasso",
                    Email = "pablo.picasso@example.org",
                    Websites = new List<string>
                    {
                        "http://picasso.example.org"
                    }
                };
            }
        }


        public static explicit operator Thrift.Person( Person p )
        {
            var tp = new Thrift.Person( p.FirstName, p.MiddleNames, p.Email, p.Websites );
            if( p.LastName != null )
            {
                tp.LastName = p.LastName;
            }
            if( p.Age.HasValue )
            {
                tp.Age = p.Age.Value;
            }
            return tp;
        }
    }
}