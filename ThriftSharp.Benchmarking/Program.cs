// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Thrift.Transport;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;

namespace ThriftSharp.Benchmarking
{
    /* Benchmarks executed on an i7-3612QM in Release mode without debugging.
     * 
     * ThriftSharp v2.1.0
     * Read, simple   00:00:00.0000007
     * Read, complex  00:00:00.0000020
     * Write, simple  00:00:00.0000008
     * Write, complex 00:00:00.0000024
     * 
     * Thrift v0.9.1
     * Read, simple   00:00:00.0000003
     * Read, complex  00:00:00.0000008
     * Write, simple  00:00:00.0000001
     * Write, complex 00:00:00.0000008
     */

    public sealed class Program
    {
        private const int WarmupIterations = 100;
        private const int Iterations = 100000;

        public static void Main( string[] args )
        {
            var actions = new Dictionary<string, Func<TimeSpan>>
            {
                { "ThriftSharp: read, simple", () => MeasureThriftSharpReadTime( SimplePerson ) },
                { "ThriftSharp: read, complex", () => MeasureThriftSharpReadTime( ComplexPerson ) },
                { "ThriftSharp: write, simple", () => MeasureThriftSharpWriteTime( SimplePerson ) },
                { "ThriftSharp: write, complex", () => MeasureThriftSharpWriteTime( ComplexPerson ) },
                { "Thrift: read, simple", () => MeasureThriftReadTime( (GeneratedPerson) SimplePerson ) },
                { "Thrift: read, complex", () => MeasureThriftReadTime( (GeneratedPerson) ComplexPerson ) },
                { "Thrift: write, simple", () => MeasureThriftWriteTime( (GeneratedPerson) SimplePerson ) },
                { "Thrift: write, complex", () => MeasureThriftWriteTime( (GeneratedPerson) ComplexPerson ) }
            };

            string format = string.Format( "{{0, -{0}}} {{1}}", actions.Keys.Max( s => s.Length ) );
            foreach ( var pair in actions )
            {
                var time = pair.Value();
                Console.WriteLine( format, pair.Key, time.ToString() );
            }

            Console.Read();
        }

        private static TimeSpan MeasureThriftReadTime( GeneratedPerson person )
        {
            // write something to be read
            byte[] bytes = TMemoryBuffer.Serialize( person );

            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations + WarmupIterations; n++ )
            {
                if ( n >= WarmupIterations )
                {
                    watch.Start();
                }

                TMemoryBuffer.DeSerialize<GeneratedPerson>( bytes );

                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }

        private static TimeSpan MeasureThriftWriteTime( GeneratedPerson person )
        {
            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations + WarmupIterations; n++ )
            {
                if ( n >= WarmupIterations )
                {
                    watch.Start();
                }

                TMemoryBuffer.Serialize( person );

                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }

        private static TimeSpan MeasureThriftSharpReadTime( Person person )
        {
            var personStruct = ThriftAttributesParser.ParseStruct( typeof( Person ).GetTypeInfo() );

            var transport = new LoopTransport();
            var protocol = new ThriftBinaryProtocol( transport );

            // write something to be read
            ThriftWriter.Write( personStruct, person, protocol );

            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations + WarmupIterations; n++ )
            {
                if ( n >= WarmupIterations )
                {
                    watch.Start();
                }

                ThriftReader.Read( personStruct, protocol );

                watch.Stop();

                transport.Reset();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }

        private static TimeSpan MeasureThriftSharpWriteTime( Person person )
        {
            var personStruct = ThriftAttributesParser.ParseStruct( typeof( Person ).GetTypeInfo() );

            var transport = new LoopTransport();
            var protocol = new ThriftBinaryProtocol( transport );

            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations + WarmupIterations; n++ )
            {
                if ( n >= WarmupIterations )
                {
                    watch.Start();
                }
                ThriftWriter.Write( personStruct, person, protocol );
                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }


        private static Person SimplePerson
        {
            get
            {
                return new Person
                {
                    FirstName = "John",
                    LastName = "Doe",
                    IsAlive = true,
                    Hobbies = new List<Hobby> { Hobby.Painting }
                };
            }
        }

        private static Person ComplexPerson
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
                    Age = 91,
                    IsAlive = false,
                    Hobbies = new List<Hobby>
                    {
                        Hobby.Painting,
                        Hobby.Drawing,
                        Hobby.Sculpting,
                        Hobby.Printmaking,
                        Hobby.Ceramics,
                        Hobby.StageDesign,
                        Hobby.Writing
                    },
                    Description = @"Pablo Ruiz y Picasso, also known as Pablo Picasso (Spanish: [ˈpaβlo piˈkaso]; 25 October 1881 – 8 April 1973), was a Spanish painter, sculptor, printmaker, ceramicist, stage designer, poet and playwright who spent most of his adult life in France. As one of the greatest and most influential artists of the 20th century, he is known for co-founding the Cubist movement, the invention of constructed sculpture,[2][3] the co-invention of collage, and for the wide variety of styles that he helped develop and explore. Among his most famous works are the proto-Cubist Les Demoiselles d'Avignon (1907), and Guernica (1937), a portrayal of the German bombing of Guernica during the Spanish Civil War."
                };
            }
        }
    }
}