// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System.Collections.Generic;
using System.Linq;

namespace ThriftSharp.Benchmarking
{
    [ThriftStruct( "GeneratedPerson" )] // all names are exactly the same as the Thrift generated ones
    public sealed class Person
    {
        [ThriftField( 1, true, "FirstName" )]
        public string FirstName { get; set; }

        [ThriftField( 2, false, "MiddleNames" )]
        public List<string> MiddleNames { get; set; }

        [ThriftField( 3, true, "LastName" )]
        public string LastName { get; set; }

        [ThriftField( 4, false, "Age" )]
        public int? Age { get; set; }

        [ThriftField( 5, true, "IsAlive" )]
        public bool IsAlive { get; set; }

        [ThriftField( 6, true, "Hobbies" )]
        public List<Hobby> Hobbies { get; set; }

        [ThriftField( 7, false, "Description" )]
        public string Description { get; set; }


        // to re-use the test data
        public static explicit operator GeneratedPerson( Person person )
        {
            var hobbies = person.Hobbies.Select( h => (GeneratedHobby) h ).ToList();
            var genPerson = new GeneratedPerson( person.FirstName, person.LastName, person.IsAlive, hobbies );

            if ( person.MiddleNames != null )
            {
                genPerson.MiddleNames = person.MiddleNames;
            }
            if ( person.Age.HasValue )
            {
                genPerson.Age = person.Age.Value;
            }
            if ( person.Description != null )
            {
                genPerson.Description = person.Description;
            }

            return genPerson;
        }
    }
}