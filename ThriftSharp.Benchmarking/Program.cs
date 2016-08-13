// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

// Uncomment this line to also bench Apache Thrift
// #define BENCH_APACHE_THRIFT

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
#if BENCH_APACHE_THRIFT
using Thrift.Transport;
using T = ThriftSharp.Benchmarking.Models.Thrift;
#endif
using ThriftSharp.Benchmarking.Models;

namespace ThriftSharp.Benchmarking
{
    public class Program
    {
#if BENCH_APACHE_THRIFT
        private static readonly byte[]
            T_All = TMemoryBuffer.Serialize( (T.AllTypesContainer) AllTypesContainer.Sample ),
            T_Binary = TMemoryBuffer.Serialize( (T.BinaryContainer) BinaryContainer.Sample ),
            T_Bool = TMemoryBuffer.Serialize( (T.BoolContainer) BoolContainer.Sample ),
            T_Byte = TMemoryBuffer.Serialize( (T.ByteContainer) ByteContainer.Sample ),
            T_Double = TMemoryBuffer.Serialize( (T.DoubleContainer) DoubleContainer.Sample ),
            T_Empty = TMemoryBuffer.Serialize( (T.EmptyStruct) EmptyStruct.Sample ),
            T_Int16 = TMemoryBuffer.Serialize( (T.Int16Container) Int16Container.Sample ),
            T_Int32 = TMemoryBuffer.Serialize( (T.Int32Container) Int32Container.Sample ),
            T_Int64 = TMemoryBuffer.Serialize( (T.Int64Container) Int64Container.Sample ),
            T_List = TMemoryBuffer.Serialize( (T.ListContainer) ListContainer.Sample ),
            T_Map = TMemoryBuffer.Serialize( (T.MapContainer) MapContainer.Sample ),
            T_Person = TMemoryBuffer.Serialize( (T.Person) Person.Sample ),
            T_PersonAvailability = TMemoryBuffer.Serialize( (T.PersonAvailability) PersonAvailability.Sample ),
            T_Set = TMemoryBuffer.Serialize( (T.SetContainer) SetContainer.Sample ),
            T_String = TMemoryBuffer.Serialize( (T.StringContainer) StringContainer.Sample ),
            T_Struct = TMemoryBuffer.Serialize( (T.StructContainer) StructContainer.Sample ),
            T_TimePeriod = TMemoryBuffer.Serialize( (T.TimePeriod) TimePeriod.Sample );
#endif

        private static readonly byte[]
            TS_All = MemoryBuffer.Serialize( AllTypesContainer.Sample ),
            TS_Binary = MemoryBuffer.Serialize( BinaryContainer.Sample ),
            TS_Bool = MemoryBuffer.Serialize( BoolContainer.Sample ),
            TS_Byte = MemoryBuffer.Serialize( ByteContainer.Sample ),
            TS_Double = MemoryBuffer.Serialize( DoubleContainer.Sample ),
            TS_Empty = MemoryBuffer.Serialize( EmptyStruct.Sample ),
            TS_Int16 = MemoryBuffer.Serialize( Int16Container.Sample ),
            TS_Int32 = MemoryBuffer.Serialize( Int32Container.Sample ),
            TS_Int64 = MemoryBuffer.Serialize( Int64Container.Sample ),
            TS_List = MemoryBuffer.Serialize( ListContainer.Sample ),
            TS_Map = MemoryBuffer.Serialize( MapContainer.Sample ),
            TS_Person = MemoryBuffer.Serialize( Person.Sample ),
            TS_PersonAvailability = MemoryBuffer.Serialize( PersonAvailability.Sample ),
            TS_Set = MemoryBuffer.Serialize( SetContainer.Sample ),
            TS_String = MemoryBuffer.Serialize( StringContainer.Sample ),
            TS_Struct = MemoryBuffer.Serialize( StructContainer.Sample ),
            TS_TimePeriod = MemoryBuffer.Serialize( TimePeriod.Sample );

        public static void Main( string[] args )
        {
            BenchmarkRunner.Run<Program>();
            Console.Read();
        }


#if BENCH_APACHE_THRIFT
        [Benchmark]
        public byte[] T_W_All()
        {
            return TMemoryBuffer.Serialize( (T.AllTypesContainer) AllTypesContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Binary()
        {
            return TMemoryBuffer.Serialize( (T.BinaryContainer) BinaryContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Bool()
        {
            return TMemoryBuffer.Serialize( (T.BoolContainer) BoolContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Byte()
        {
            return TMemoryBuffer.Serialize( (T.ByteContainer) ByteContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Double()
        {
            return TMemoryBuffer.Serialize( (T.DoubleContainer) DoubleContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Empty()
        {
            return TMemoryBuffer.Serialize( (T.EmptyStruct) EmptyStruct.Sample );
        }

        [Benchmark]
        public byte[] T_W_Int16()
        {
            return TMemoryBuffer.Serialize( (T.Int16Container) Int16Container.Sample );
        }

        [Benchmark]
        public byte[] T_W_Int32()
        {
            return TMemoryBuffer.Serialize( (T.Int32Container) Int32Container.Sample );
        }

        [Benchmark]
        public byte[] T_W_Int64()
        {
            return TMemoryBuffer.Serialize( (T.Int64Container) Int64Container.Sample );
        }

        [Benchmark]
        public byte[] T_W_List()
        {
            return TMemoryBuffer.Serialize( (T.ListContainer) ListContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Map()
        {
            return TMemoryBuffer.Serialize( (T.MapContainer) MapContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Person()
        {
            return TMemoryBuffer.Serialize( (T.Person) Person.Sample );
        }

        [Benchmark]
        public byte[] T_W_PersonAvailability()
        {
            return TMemoryBuffer.Serialize( (T.PersonAvailability) PersonAvailability.Sample );
        }

        [Benchmark]
        public byte[] T_W_Set()
        {
            return TMemoryBuffer.Serialize( (T.SetContainer) SetContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_String()
        {
            return TMemoryBuffer.Serialize( (T.StringContainer) StringContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_Struct()
        {
            return TMemoryBuffer.Serialize( (T.StructContainer) StructContainer.Sample );
        }

        [Benchmark]
        public byte[] T_W_TimePeriod()
        {
            return TMemoryBuffer.Serialize( (T.TimePeriod) TimePeriod.Sample );
        }
#endif


        [Benchmark]
        public byte[] TS_W_All()
        {
            return MemoryBuffer.Serialize( AllTypesContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Binary()
        {
            return MemoryBuffer.Serialize( BinaryContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Bool()
        {
            return MemoryBuffer.Serialize( BoolContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Byte()
        {
            return MemoryBuffer.Serialize( ByteContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Double()
        {
            return MemoryBuffer.Serialize( DoubleContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Empty()
        {
            return MemoryBuffer.Serialize( EmptyStruct.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Int16()
        {
            return MemoryBuffer.Serialize( Int16Container.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Int32()
        {
            return MemoryBuffer.Serialize( Int32Container.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Int64()
        {
            return MemoryBuffer.Serialize( Int64Container.Sample );
        }

        [Benchmark]
        public byte[] TS_W_List()
        {
            return MemoryBuffer.Serialize( ListContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Map()
        {
            return MemoryBuffer.Serialize( MapContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Person()
        {
            return MemoryBuffer.Serialize( Person.Sample );
        }

        [Benchmark]
        public byte[] TS_W_PersonAvailability()
        {
            return MemoryBuffer.Serialize( PersonAvailability.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Set()
        {
            return MemoryBuffer.Serialize( SetContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_String()
        {
            return MemoryBuffer.Serialize( StringContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_Struct()
        {
            return MemoryBuffer.Serialize( StructContainer.Sample );
        }

        [Benchmark]
        public byte[] TS_W_TimePeriod()
        {
            return MemoryBuffer.Serialize( TimePeriod.Sample );
        }


#if BENCH_APACHE_THRIFT
        [Benchmark]
        public object T_R_All()
        {
            return TMemoryBuffer.DeSerialize<T.AllTypesContainer>( T_All );
        }

        [Benchmark]
        public object T_R_Binary()
        {
            return TMemoryBuffer.DeSerialize<T.BinaryContainer>( T_Binary );
        }

        [Benchmark]
        public object T_R_Bool()
        {
            return TMemoryBuffer.DeSerialize<T.BoolContainer>( T_Bool );
        }

        [Benchmark]
        public object T_R_Byte()
        {
            return TMemoryBuffer.DeSerialize<T.ByteContainer>( T_Byte );
        }

        [Benchmark]
        public object T_R_Double()
        {
            return TMemoryBuffer.DeSerialize<T.DoubleContainer>( T_Double );
        }

        [Benchmark]
        public object T_R_Empty()
        {
            return TMemoryBuffer.DeSerialize<T.EmptyStruct>( T_Empty );
        }

        [Benchmark]
        public object T_R_Int16()
        {
            return TMemoryBuffer.DeSerialize<T.Int16Container>( T_Int16 );
        }

        [Benchmark]
        public object T_R_Int32()
        {
            return TMemoryBuffer.DeSerialize<T.Int32Container>( T_Int32 );
        }

        [Benchmark]
        public object T_R_Int64()
        {
            return TMemoryBuffer.DeSerialize<T.Int64Container>( T_Int64 );
        }

        [Benchmark]
        public object T_R_List()
        {
            return TMemoryBuffer.DeSerialize<T.ListContainer>( T_List );
        }

        [Benchmark]
        public object T_R_Map()
        {
            return TMemoryBuffer.DeSerialize<T.MapContainer>( T_Map );
        }

        [Benchmark]
        public object T_R_Person()
        {
            return TMemoryBuffer.DeSerialize<T.Person>( T_Person );
        }

        [Benchmark]
        public object T_R_PersonAvailability()
        {
            return TMemoryBuffer.DeSerialize<T.PersonAvailability>( T_PersonAvailability );
        }

        [Benchmark]
        public object T_R_Set()
        {
            return TMemoryBuffer.DeSerialize<T.SetContainer>( T_Set );
        }

        [Benchmark]
        public object T_R_String()
        {
            return TMemoryBuffer.DeSerialize<T.StringContainer>( T_String );
        }

        [Benchmark]
        public object T_R_Struct()
        {
            return TMemoryBuffer.DeSerialize<T.StructContainer>( T_Struct );
        }

        [Benchmark]
        public object T_R_TimePeriod()
        {
            return TMemoryBuffer.DeSerialize<T.TimePeriod>( T_TimePeriod );
        }
#endif


        [Benchmark]
        public object TS_R_All()
        {
            return MemoryBuffer.Deserialize<AllTypesContainer>( TS_All );
        }

        [Benchmark]
        public object TS_R_Binary()
        {
            return MemoryBuffer.Deserialize<BinaryContainer>( TS_Binary );
        }

        [Benchmark]
        public object TS_R_Bool()
        {
            return MemoryBuffer.Deserialize<BoolContainer>( TS_Bool );
        }

        [Benchmark]
        public object TS_R_Byte()
        {
            return MemoryBuffer.Deserialize<ByteContainer>( TS_Byte );
        }

        [Benchmark]
        public object TS_R_Double()
        {
            return MemoryBuffer.Deserialize<DoubleContainer>( TS_Double );
        }

        [Benchmark]
        public object TS_R_Empty()
        {
            return MemoryBuffer.Deserialize<EmptyStruct>( TS_Empty );
        }

        [Benchmark]
        public object TS_R_Int16()
        {
            return MemoryBuffer.Deserialize<Int16Container>( TS_Int16 );
        }

        [Benchmark]
        public object TS_R_Int32()
        {
            return MemoryBuffer.Deserialize<Int32Container>( TS_Int32 );
        }

        [Benchmark]
        public object TS_R_Int64()
        {
            return MemoryBuffer.Deserialize<Int64Container>( TS_Int64 );
        }

        [Benchmark]
        public object TS_R_List()
        {
            return MemoryBuffer.Deserialize<ListContainer>( TS_List );
        }

        [Benchmark]
        public object TS_R_Map()
        {
            return MemoryBuffer.Deserialize<MapContainer>( TS_Map );
        }

        [Benchmark]
        public object TS_R_Person()
        {
            return MemoryBuffer.Deserialize<Person>( TS_Person );
        }

        [Benchmark]
        public object TS_R_PersonAvailability()
        {
            return MemoryBuffer.Deserialize<PersonAvailability>( TS_PersonAvailability );
        }

        [Benchmark]
        public object TS_R_Set()
        {
            return MemoryBuffer.Deserialize<SetContainer>( TS_Set );
        }

        [Benchmark]
        public object TS_R_String()
        {
            return MemoryBuffer.Deserialize<StringContainer>( TS_String );
        }

        [Benchmark]
        public object TS_R_Struct()
        {
            return MemoryBuffer.Deserialize<StructContainer>( TS_Struct );
        }

        [Benchmark]
        public object TS_R_TimePeriod()
        {
            return MemoryBuffer.Deserialize<TimePeriod>( TS_TimePeriod );
        }
    }
}