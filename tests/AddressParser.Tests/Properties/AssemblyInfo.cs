using System;
using System.Reflection;
using System.Runtime.InteropServices;
using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(DiffReporter))]
[assembly: AssemblyTitle("AddressParser.Tests")]
[assembly: AssemblyDescription("Tests for AddressParser")]
[assembly: AssemblyCompany("Jim Counts")]
[assembly: AssemblyProduct("AddressParser.Tests")]
[assembly: AssemblyCopyright("Copyright © Jim Counts 2014")]
[assembly: ComVisible(false)]
[assembly: Guid("09e66d40-173e-4c07-a053-4c0c04cc2c1a")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: CLSCompliant(true)]