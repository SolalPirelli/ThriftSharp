// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle( "Thrift#" )]
[assembly: AssemblyDescription( "An attribute-based, IDL-less Thrift client for .NET" )]
[assembly: AssemblyCopyright( "Copyright © Solal Pirelli 2014" )]
[assembly: NeutralResourcesLanguage( "en" )]

[assembly: ComVisible( false )]

[assembly: AssemblyVersion( "2.0.5.*" )]

[assembly: InternalsVisibleTo( "ThriftSharp.Benchmarking" )]
[assembly: InternalsVisibleTo( "ThriftSharp.Extensions" )]
[assembly: InternalsVisibleTo( "ThriftSharp.Tests" )]