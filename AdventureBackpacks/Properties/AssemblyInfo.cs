using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AdventureBackpacks")]
[assembly: AssemblyDescription("A Valheim Mod for adding progression multiple backpacks as an item/utility gear addition.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Vapok Gaming")]
[assembly: AssemblyProduct("AdventureBackpacks")]
[assembly: AssemblyCopyright("Copyright ©  2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("997CB563-FCC7-44B7-8F71-069747D27CC5")]

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
#if ! API
[assembly: AssemblyVersion("1.7.10.0")]
[assembly: AssemblyFileVersion("1.7.10.0")]
#else
[assembly: AssemblyVersion("1.1.0")]
[assembly: AssemblyFileVersion("1.1.0")]
#endif
