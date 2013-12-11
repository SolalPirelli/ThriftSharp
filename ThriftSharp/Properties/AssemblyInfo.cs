// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle( "Thrift#" )]
[assembly: AssemblyDescription( "An attribute-based, IDL-less Thrift client for .NET" )]
[assembly: AssemblyCopyright( "Copyright © Solal Pirelli 2013" )]
[assembly: NeutralResourcesLanguage( "en" )]

[assembly: AssemblyVersion( "1.0.6.*" )]

[assembly: InternalsVisibleTo( "ThriftSharp.Extensions" )]
[assembly: InternalsVisibleTo( "ThriftSharp.Tests" )]