using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Umbraco.Foundation.Localization.Configuration;
using Sandbox.Localization.PluginTest;
using System.Web.UI;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Sandbox.Localization.PluginTest")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Sandbox.Localization.PluginTest")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c5f483f6-8197-4d0e-baeb-b84cc9d24902")]

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: LocalizationXmlSource("Redirect.Texts.xml")]
[assembly: LocalizationXmlSource("Redirect.en-US.xml")]


[assembly: LocalizationSourceFactory(typeof(MutatingTextSourceFactory))]
[assembly: LocalizationSourceFactory(typeof(CustomTextSourceFactory))]

[assembly:WebResource("Sandbox.Localization.PluginTest.Tulips.jpg", "image/jpeg")]