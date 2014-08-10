// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;

namespace ThriftSharp.Benchmarking
{
    /* v1.0.9 (i7-3612QM)
     * Read+Write: Simple       00:00:00.0000060
     * Read+Write: Complex      00:00:00.0001019
     * Read+Write: Very complex 00:00:00.0003622
     * 
     * v2.0 (i7-3612QM)
     * Read+Write: Simple       00:00:00.0000050
     * Read+Write: Complex      00:00:00.0000123
     * Read+Write: Very complex 00:00:00.0000340
     * 
     * v2.1 (i7-3612QM)
     * Read: Simple        00:00:00.0000012
     * Read: Complex       00:00:00.0000027
     * Read: Very complex  00:00:00.0000077
     * Write: Simple       00:00:00.0000013
     * Write: Complex      00:00:00.0000033
     * Write: Very complex 00:00:00.0000090
     */

    public sealed class Program
    {
        private const int WarmupIterations = 100;
        private const int Iterations = 100000;

        private static readonly Dictionary<string, Func<TimeSpan>> Actions = new Dictionary<string, Func<TimeSpan>>
        {
            { "Read: Simple", () => MeasureReadTime( Person.GetSimplePerson() ) },
            { "Read: Complex", () => MeasureReadTime( Person.GetComplexPerson() ) },
            { "Read: Very complex", () => MeasureReadTime( Person.GetVeryComplexPerson() ) },
            { "Write: Simple", () => MeasureWriteTime( Person.GetSimplePerson() ) },
            { "Write: Complex", () => MeasureWriteTime( Person.GetComplexPerson() ) },
            { "Write: Very complex", () => MeasureWriteTime( Person.GetVeryComplexPerson() ) }
        };

        public static void Main( string[] args )
        {
            string format = string.Format( "{{0, -{0}}} {{1}}", Actions.Keys.Max( s => s.Length ) );
            foreach ( var pair in Actions )
            {
                var time = pair.Value();
                Console.WriteLine( format, pair.Key, time.ToString() );
            }

            Console.Read();
        }

        private static TimeSpan MeasureReadTime( Person person )
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

        private static TimeSpan MeasureWriteTime( Person person )
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
    }
}