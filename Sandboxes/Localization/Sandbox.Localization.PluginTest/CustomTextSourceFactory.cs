using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Foundation.Localization.Configuration;
using Umbraco.Foundation.Localization.Maintenance;
using Umbraco.Foundation.Localization;
using System.Reflection;

namespace Sandbox.Localization.PluginTest
{
    public class CustomTextSourceFactory : ILocalizationTextSourceFactory
    {

        public ITextSource GetSource(Umbraco.Foundation.Localization.TextManager textManager, Assembly referenceAssembly, string targetNamespace)
        {
            var source = new SimpleTextSource();
            source.Texts.Add(new LocalizedText
            {
                Namespace = targetNamespace,
                Key = "FactoryTest",
                Pattern = "I'm from a factory",
                Language = "en-US",
                Source = new TextSourceInfo { TextSource = source, ReferenceAssembly = referenceAssembly }
            });

            return source;
        }
    }
}
