#region

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("JPB.DataAccess")]
[assembly: AssemblyDescription("ORM that uses Reflection and IL code to create POCO's and fill them with data")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("JPB")]
[assembly: AssemblyProduct("JPB.DataAccess")]
[assembly: AssemblyCopyright("Copyright © Jean-Pierre Bachmann 2017")]
[assembly: AssemblyTrademark("")]
//[assembly: AssemblyCulture("en-us")]

[assembly: AssemblyCulture("")]
[assembly: AssemblyDefaultAlias("DataAccess")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("bae27d49-4abf-4e73-a21e-dbe03dc0e806")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("2.0.200.0")]
[assembly: AssemblyFileVersion("2.0.*")]
[assembly: InternalsVisibleTo("JPB.DataAccess.EntityCreator.Core")]
[assembly: InternalsVisibleTo("JPB.DataAccess.Tests")]
[assembly: InternalsVisibleTo("JPB.DataAccess.Tests.MsSQL")]
[assembly: InternalsVisibleTo("JPB.DataAccess.Tests.SqLite")]