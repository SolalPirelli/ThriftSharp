// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Benchmarking
{
    [ThriftStruct( "Person" )]
    public sealed class Person
    {
        [ThriftField( 1, true, "FirstName" )]
        public string Name { get; set; }

        [ThriftField( 2, false, "MiddleNames" )]
        public string[] MiddleNames { get; set; }

        [ThriftField( 3, true, "LastName" )]
        public string LastName { get; set; }

        [ThriftField( 4, false, "Age" )]
        public int? Age { get; set; }

        [ThriftField( 5, true, "IsAlive" )]
        public bool IsAlive { get; set; }

        [ThriftField( 6, true, "Hobbies" )]
        public Hobby[] Hobbies { get; set; }

        [ThriftField( 7, true, "Relatives" )]
        public Person[] Relatives { get; set; }


        public static Person GetSimplePerson()
        {
            return new Person
            {
                Name = "John",
                LastName = "Doe",
                Age = 42,
                IsAlive = true,
                Hobbies = new Hobby[0],
                Relatives = new Person[0]
            };
        }

        public static Person GetComplexPerson()
        {
            return new Person
            {
                Name = "Pablo",
                MiddleNames = new[]
                {
                    "Diego", "José", "Francisco de Paula", "Juan", "Nepomuceno", "María de los Remedios", "Cipriano de la Santísima Trinidad"
                },
                LastName = "Ruiz y Picasso",
                IsAlive = false,
                Hobbies = new[] { Hobby.Painting },
                Relatives = new[] 
                { 
                    new Person
                    {
                        Name = "Olga", 
                        MiddleNames = new[] { "Stepanovna" }, 
                        LastName = "Khokhlova",
                        IsAlive = false,
                        Hobbies = new[] { Hobby.Dancing },
                        Relatives = new Person[0]
                    }
                }
            };
        }

        public static Person GetVeryComplexPerson()
        {
            return new Person
            {
                Name = "John",
                MiddleNames = new[]
                {
                    "Q", "W", "E", "R", "T", "Z", "UIOP", "ASDF", "GHJ", "KL", "YX", "CVB", "NM"
                },
                LastName = "Public",
                Age = 34,
                IsAlive = true,
                Hobbies = new[] { Hobby.Programming, Hobby.Sleeping, Hobby.Sports },
                Relatives = new[] 
                { 
                    new Person
                    {
                        Name = "Jane",
                        MiddleNames = new[]
                        {
                            "1", "2", "345", "67890"
                        },
                        LastName = "Public",
                        Age = 35,
                        IsAlive = true,
                        Hobbies = new[] { Hobby.Programming, Hobby.Sleeping },
                        Relatives = new[]
                        {
                            new Person
                            {
                                Name = "Alice",
                                MiddleNames = new[]
                                {
                                    "Carol", "Eve"
                                },
                                LastName = "Doe",
                                Age = 69,
                                IsAlive = true,
                                Hobbies = new Hobby[0],
                                Relatives = new[]
                                {
                                    new Person
                                    {
                                        Name = "Bob",
                                        MiddleNames = new string[0],
                                        LastName = "Doe",
                                        IsAlive = false,
                                        Hobbies = new[] { Hobby.Sleeping },
                                        Relatives = new Person[0]
                                    }
                                }
                            }
                        }
                    },
                    new Person
                    {
                        Name = "John Jr",
                        MiddleNames = new[]
                        {
                            "Q", "1", "E", "2"
                        },
                        LastName = "Public",
                        Age = 3,
                        IsAlive = true,
                        Hobbies = new[] { Hobby.Sleeping },
                        Relatives = new Person[0]
                    }
                }
            };
        }


        [ThriftEnum]
        public enum Hobby
        {
            Painting,
            Programming,
            Sports,
            Sleeping,
            Dancing
        }
    }
}