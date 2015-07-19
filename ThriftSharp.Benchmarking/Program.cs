// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Linq;
using System.Reflection;

namespace ThriftSharp.Benchmarking
{
    public sealed class Program
    {
        public static void Main( string[] args )
        {
            foreach ( var type in Assembly.GetExecutingAssembly().GetTypes().Where( t => t.Namespace == "ThriftSharp.Benchmarking.Models" ) )
            {
                var result = Benchmarker.Benchmark( type, 10000, 10 );
                Console.WriteLine( result );
            }
        }
    }
}