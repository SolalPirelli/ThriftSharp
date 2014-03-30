// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;

namespace ThriftSharp.Benchmarking
{
    /*
     * Thrift# v 1.0.9  (i7-3612QM)
     * Simple person       00:00:00.0000060
     * Complex person      00:00:00.0001019
     * Very complex person 00:00:00.0003622
     */

    public sealed class Program
    {
        private const int WarmupIterations = 100;
        private const int Iterations = 100000;

        private static readonly IThriftProtocol Protocol = new ThriftBinaryProtocol( new LoopTransport() );
        private static readonly ThriftStruct ThriftPerson = ThriftAttributesParser.ParseStruct( typeof( Person ) );

        private static readonly Dictionary<string, Action> Actions = new Dictionary<string, Action>
        {
            { "Simple person", () => WriteAndRead( Person.GetSimplePerson() ) },
            { "Complex person", () => WriteAndRead( Person.GetComplexPerson() ) },
            { "Very complex person", () => WriteAndRead( Person.GetVeryComplexPerson() ) }
        };

        public static void Main( string[] args )
        {
            foreach ( var pair in Actions )
            {
                var time = MeasureExecutionTime( pair.Value );
                Console.WriteLine( "{0} {1}", pair.Key, time.ToString() );
            }
        }

        private static void WriteAndRead( Person person )
        {
            ThriftSerializer.WriteStruct( Protocol, ThriftPerson, person );
            ThriftSerializer.ReadStruct( Protocol, ThriftPerson, person );
        }

        private static TimeSpan MeasureExecutionTime( Action action )
        {
            for ( int n = 0; n < WarmupIterations; n++ )
            {
                action();
            }

            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations; n++ )
            {
                watch.Start();
                action();
                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }
    }
}