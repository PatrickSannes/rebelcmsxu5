using System;

namespace Umbraco.Cms.Web
{

    /// <summary>
    /// Informs the system that the assembly tagged with this attribute contains plugins and the system should scan and load them
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class AssemblyContainsPluginsAttribute : Attribute
    {
    }
}
