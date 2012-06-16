using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Umbraco.Foundation.Localization;

namespace Sandbox.Localization.PluginTest
{
    public class L10n : Localization<GetText> { }

    public class GetText
    {
        public static string Get()
        {            
            return L10n.Get("Plugin.Key", new { Count = 10 });
        }
    }
}
