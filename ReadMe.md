[![Build](https://img.shields.io/appveyor/ci/SolalPirelli/ThriftSharp.svg?style=flat-square)](https://ci.appveyor.com/project/SolalPirelli/ThriftSharp)
[![NuGet Package](https://img.shields.io/nuget/v/ThriftSharp.svg?style=flat-square&label=ThriftSharp)](https://www.nuget.org/packages/ThriftSharp/)

**Thrift#** is an implementation of the [Thrift protocol](http://thrift.apache.org/) for .NET.

Unlike [Apache's Thrift implementation](https://github.com/apache/thrift/), it does not use a compiler or separate Thrift interface definition files; everything is done using attributes.
This allows complete freedom over the types, and enables interesting features such as converters.

Read the [documentation](https://github.com/SolalPirelli/ThriftSharp/wiki) to get started.

Pull requests, bug reports and suggestions are welcome.

To package as a NuGet package, run `dotnet pack -c Release --include-source --include-symbols` in `src/`.
