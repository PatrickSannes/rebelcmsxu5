using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// Identifies a default Umbraco macro engine that is shipped with the framework
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class UmbracoMacroEngineAttribute : Attribute
    {

    }
}
