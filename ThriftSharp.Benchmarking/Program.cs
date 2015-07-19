// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

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
    /* Benchmarks results on an i7-4710HQ in Release mode without debugging:
     * (100 warmup iterations, 100,000 iterations)
     * 
     * ThriftSharp v2.4.0.0
     * Read, simple   00:00:00.0000005
     * Read, complex  00:00:00.0000014
     * Write, simple  00:00:00.0000005
     * Write, complex 00:00:00.0000014
     * 
     * Thrift v0.9.1.3
     * Read, simple   00:00:00.0000024
     * Read, complex  00:00:00.0000032
     * Write, simple  00:00:00.0000004
     * Write, complex 00:00:00.0000010
     */
    public sealed class Program
    {
        private const int WarmupIterations = 100;
        private const int Iterations = 100000;

        public static void Main( string[] args )
        {
            var thriftSharpActions = new Dictionary<string, Func<TimeSpan>>
            {
                { "Read, simple", () => MeasureThriftSharpReadTime( SimplePerson ) },
                { "Read, complex", () => MeasureThriftSharpReadTime( ComplexPerson ) },
                { "Write, simple", () => MeasureThriftSharpWriteTime( SimplePerson ) },
                { "Write, complex", () => MeasureThriftSharpWriteTime( ComplexPerson ) }
            };

            var thriftActions = new Dictionary<string, Func<TimeSpan>>
            {
                { "Read, simple", () => MeasureThriftReadTime( (GeneratedPerson) SimplePerson ) },
                { "Read, complex", () => MeasureThriftReadTime( (GeneratedPerson) ComplexPerson ) },
                { "Write, simple", () => MeasureThriftWriteTime( (GeneratedPerson) SimplePerson ) },
                { "Write, complex", () => MeasureThriftWriteTime( (GeneratedPerson) ComplexPerson ) }
            };

            Console.WriteLine( "ThriftSharp v{0}", typeof( ThriftCommunication ).Assembly.GetName().Version );
            Measure( thriftSharpActions );

            Console.WriteLine();

            Console.WriteLine( "Thrift v{0}", typeof( TTransport ).Assembly.GetName().Version );
            Measure( thriftActions );

            Console.Read();
        }

        #region Benchmarking
        private static void Measure( Dictionary<string, Func<TimeSpan>> actions )
        {
            string format = string.Format( "{{0, -{0}}} {{1}}", actions.Keys.Max( s => s.Length ) );
            foreach ( var pair in actions )
            {
                var time = pair.Value();
                Console.WriteLine( format, pair.Key, time.ToString() );
            }
        }

        private static TimeSpan MeasureThriftSharpReadTime( Person person )
        {
            var personStruct = ThriftAttributesParser.ParseStruct( typeof( Person ).GetTypeInfo() );

            var transport = new LoopTransport();
            var protocol = new ThriftBinaryProtocol( transport );

            // write something to be read
            ThriftStructWriter.Write( personStruct, person, protocol );

            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations + WarmupIterations; n++ )
            {
                transport.PrepareRead();

                // clean up
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                if ( n >= WarmupIterations )
                {
                    watch.Start();
                }

                ThriftStructReader.Read<Person>( personStruct, protocol );

                watch.Stop();
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
                // clean up
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                if ( n >= WarmupIterations )
                {
                    watch.Start();
                }

                ThriftStructWriter.Write( personStruct, person, protocol );

                watch.Stop();

                transport.Reset();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }

        private static TimeSpan MeasureThriftReadTime( GeneratedPerson person )
        {
            // write something to be read
            byte[] bytes = TMemoryBuffer.Serialize( person );

            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations + WarmupIterations; n++ )
            {
                // clean up
                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
                GC.WaitForPendingFinalizers();

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
                // clean up
                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
                GC.WaitForPendingFinalizers();

                if ( n >= WarmupIterations )
                {
                    watch.Start();
                }

                TMemoryBuffer.Serialize( person );

                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }
        #endregion

        #region Data
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
        #endregion
    }
}