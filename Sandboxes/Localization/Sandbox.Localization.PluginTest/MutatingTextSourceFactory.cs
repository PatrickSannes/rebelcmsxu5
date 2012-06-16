using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Foundation.Localization.Maintenance;
using Umbraco.Foundation.Localization.Configuration;
using Umbraco.Foundation.Localization;
using System.Reflection;

namespace Sandbox.Localization.PluginTest
{
    public class MutatingTextSourceFactory : ILocalizationTextSourceFactory
    {

        private static SimpleTextSource _source = new SimpleTextSource();
        private static string _targetNamespace;
        private static Assembly _referenceAssembly;

        public static void Mutate(string newValue)
        {
            using (_source.BeginUpdate())
            {
                _source.Texts.Clear();
                _source.Texts.Add(new LocalizedText
                {
                    Namespace = _targetNamespace,
                    Key = "Mutating",
                    Pattern = newValue,
                    Language = "en-US",
                    Source = new TextSourceInfo { TextSource = _source, ReferenceAssembly = _referenceAssembly }
                });
            }
        }



        public ITextSource GetSource(Umbraco.Foundation.Localization.TextManager textManager, Assembly referenceAssembly, string targetNamespace)
        {
            _targetNamespace = targetNamespace;
            _referenceAssembly = referenceAssembly;
            return _source;
        }
    }
}
