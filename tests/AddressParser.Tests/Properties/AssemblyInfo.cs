using System;
using System.Runtime.InteropServices;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;

[assembly: UseReporter(typeof(DiffReporter))]
[assembly: ComVisible(false)]
[assembly: Guid("09e66d40-173e-4c07-a053-4c0c04cc2c1a")]
[assembly: CLSCompliant(true)]
[assembly: UseApprovalSubdirectory("approvals")]