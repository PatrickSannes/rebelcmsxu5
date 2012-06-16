using System;

namespace Umbraco.Cms.Web.Packages
{
    /// <summary>
    /// Used to tag an assembly as containing Package actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class PackageActionAssemblyAttribute : Attribute
    {

    }
}