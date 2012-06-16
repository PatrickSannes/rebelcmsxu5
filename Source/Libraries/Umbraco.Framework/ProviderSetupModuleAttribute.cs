using System;

namespace Umbraco.Framework
{
    /// <summary>
    /// Identifies a class as being the default IoC dependency builder for a provider
    /// </summary>
    /// <remarks></remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ProviderSetupModuleAttribute : Attribute
    {
    }
}