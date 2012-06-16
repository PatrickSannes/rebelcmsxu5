using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Web.Mvc;
using System.Web;
using System.Reflection;
using Umbraco.Foundation.Localization.Maintenance;
using Umbraco.Foundation.Localization;
using System.Globalization;
using System.Linq.Expressions;
using Umbraco.Foundation.Localization.Configuration;
using Umbraco.Foundation.Web.Localization;

namespace Sandbox.Localization.WebTest
{


    //With this little stunt the views can use L10n.Get directly without specifying type
    public class L10n : Localization<LocalizationBootstrapper> { }


    //Simple example of bootstrapping the localization engine

    public class LocalizationBootstrapper
    {
        private static Dictionary<string, LanguageInfo> _languages = new Dictionary<string, LanguageInfo>
        {
            {"en-GB", new LanguageInfo { Description = "English (UK)", Key = "en-GB"}.InferCultureFromKey()},
            {"en-US", new LanguageInfo { Description = "English (US)", Key = "en-US"}.InferCultureFromKey()},
            {"da-DK", new LanguageInfo { Description = "Dansk", Key = "da-DK", Culture=CultureInfo.GetCultureInfo("da-DK")}}
        };


        private static LanguageInfo _currentLanguage;        

        public static string CurrentLanguage
        {
            get { return _currentLanguage.Key; }
            set
            {
                LanguageInfo newLang;
                if (!_languages.TryGetValue(value, out newLang))
                {
                    throw new Exception("The language \"" + value + "\" is not defined in this context");
                }
                _currentLanguage = newLang;
            }
        }

        public static LanguageInfo CurrentLanguageInfo
        {
            get { return _currentLanguage; }
        }

        static LocalizationBootstrapper()
        {
            _languages["en-GB"].Fallbacks = new List<LanguageInfo> { _languages["en-US"] };
            _languages["da-DK"].Fallbacks = new List<LanguageInfo> { _languages["en-GB"], _languages["en-US"] };
            _currentLanguage = _languages["en-GB"];            
        }


        static TextManager _currentManager;

        public static void Setup()
        {

            var manager = LocalizationWebConfig.SetupDefaultManager<LocalizationBootstrapper>();

            manager.PrepareTextSources(manager.GetNamespace<DefaultTextManager>());
           
            manager.MissingTextHandler = (ns, key, lang) => "(" + key + ")";
            
            /*//Get texts defined in this project. They will override those from assemblies
            var xml = new XmlTextSource
            {
                Document = XDocument.Load(HostingEnvironment.MapPath("~/LocalizationEntries.xml"))
            };

            manager.Texts.Sources.Add(new PrioritizedTextSource(xml, 2));            */
                       

            _currentManager = manager;                                 
                        
        }

        public static TextManager CurrentManager
        {
            get
            {                
                return _currentManager;
            }
        }
    }

    

     
}
