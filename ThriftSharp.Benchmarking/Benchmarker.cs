using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Thrift.Protocol;
using Thrift.Transport;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;

namespace ThriftSharp.Benchmarking
{
    public static class Benchmarker
    {
        public static BenchmarkResult Benchmark( Type type, int iterations, int count )
        {
            var convertMethod = type.GetMethod( "op_Explicit", BindingFlags.Public | BindingFlags.Static );
            var tsReadMethod = GetMethod( "ThriftSharpRead", type );
            var tsWriteMethod = GetMethod( "ThriftSharpWrite", type );
            var tReadMethod = GetMethod( "ThriftRead", convertMethod.ReturnType );
            var obj = type.GetProperty( "Sample", BindingFlags.Public | BindingFlags.Static ).GetMethod.Invoke( null, null );
            var convertedObj = (TAbstractBase) convertMethod.Invoke( null, new object[] { obj } );

            // This actually does count + 1 outer loop iterations, with one ignored for warmup
            var results = new List<BenchmarkResult>();
            for ( int n = 0; n < count + 1; n++ )
            {
                var tsRead = tsReadMethod( obj, iterations );
                var tsWrite = tsWriteMethod( obj, iterations );
                var tRead = tReadMethod( convertedObj, iterations );
                var tWrite = ThriftWrite( convertedObj, iterations );

                results.Add( new BenchmarkResult( type.Name, tsRead, tsWrite, tRead, tWrite ) );
            }

            return BenchmarkResult.Average( results.Skip( 1 ) );
        }

        private static TimeSpan ThriftSharpRead<T>( T obj, int iterations )
        {
            var personStruct = ThriftAttributesParser.ParseStruct( typeof( T ).GetTypeInfo() );

            var transport = new LoopTransport();
            var protocol = new ThriftBinaryProtocol( transport );

            ThriftStructWriter.Write( personStruct, obj, protocol );

            var watch = new Stopwatch();
            for ( int n = 0; n < iterations; n++ )
            {
                transport.PrepareRead();

                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );

                watch.Start();
                ThriftStructReader.Read<T>( personStruct, protocol );
                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks );
        }

        private static TimeSpan ThriftSharpWrite<T>( T obj, int iterations )
        {
            var personStruct = ThriftAttributesParser.ParseStruct( typeof( T ).GetTypeInfo() );

            var transport = new LoopTransport();
            var protocol = new ThriftBinaryProtocol( transport );

            var watch = new Stopwatch();
            for ( int n = 0; n < iterations; n++ )
            {
                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );

                watch.Start();
                ThriftStructWriter.Write( personStruct, obj, protocol );
                watch.Stop();

                transport.Reset();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks );
        }

        private static TimeSpan ThriftRead<T>( T obj, int iterations )
            where T : TAbstractBase
        {
            byte[] bytes = TMemoryBuffer.Serialize( obj );

            var watch = new Stopwatch();
            for ( int n = 0; n < iterations; n++ )
            {
                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );

                watch.Start();
                TMemoryBuffer.DeSerialize<T>( bytes );
                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks );
        }

        private static TimeSpan ThriftWrite( TAbstractBase obj, int iterations )
        {
            var watch = new Stopwatch();
            for ( int n = 0; n < iterations; n++ )
            {
                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );

                watch.Start();
                TMemoryBuffer.Serialize( obj );
                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks );
        }


        private static Func<object, int, TimeSpan> GetMethod( string methodName, Type genericArg )
        {
            var method = typeof( Benchmarker ).GetMethod( methodName, BindingFlags.NonPublic | BindingFlags.Static )
                                              .MakeGenericMethod( genericArg );
            return ( obj, iters ) => (TimeSpan) method.Invoke( null, new object[] { obj, iters } );
        }
    }
}