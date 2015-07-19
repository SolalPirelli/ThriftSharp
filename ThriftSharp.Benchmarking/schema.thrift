namespace csharp ThriftSharp.Benchmarking.Models.Thrift

// First, a single-field container for each type

struct BoolContainer {
    1: required bool value;
}

struct ByteContainer {
    1: required byte value;
}

struct DoubleContainer {
    1: required double value;
}

struct Int16Container {
    1: required i16 value;
}

struct Int32Container {
    1: required i32 value;
}

struct Int64Container {
    1: required i64 value;
}

struct BinaryContainer {
    1: required binary value;
}

struct StringContainer {
    1: required string value;
}

struct EmptyStruct {
}

struct StructContainer {
    1: required EmptyStruct value;
}

struct MapContainer {
    1: required map<i32, i32> value;
}

struct SetContainer {
    1: required set<i32> value;
}

struct ListContainer {
    1: required list<i32> value;
}

// Then a few realistic structs inspired from real code

struct Person {
    1: required string firstName;
    2: required list<string> middleNames;
    3: optional string lastName;
    4: optional i32 age;
    5: required string email;
    6: required list<string> websites;
}

struct TimePeriod {
    1: required i64 from;
    2: required i64 to;
    3: required bool available;
}

struct PersonAvailability {
    1: required Person person;
    2: required list<TimePeriod> periods;
}

// Finally, a struct containing everything

struct AllTypesContainer {
    1: required bool value1;
    2: required byte value2;
    3: required double value3;
    4: required i16 value4;
    5: required i32 value5;
    6: required i64 value6;
    7: required binary value7;
    8: required string value8;
    9: required EmptyStruct value9;
    10: required map<i32, i32> value10;
    11: required set<i32> value11;
    12: required list<i32> value12;
}