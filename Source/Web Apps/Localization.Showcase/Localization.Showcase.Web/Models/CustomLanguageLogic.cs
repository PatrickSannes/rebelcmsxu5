using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Localization.Configuration;
using System.Threading;
using System.Globalization;

namespace Localization.Showcase.Web.Models
{
    /// <summary>
    /// This class show how to implement your own notion of languages.
    /// You impelmentation can contain UI specific information etc...
    /// </summary>
    public static class CustomLanguageLogic
    {
        public static List<CustomLanguageInfo> Languages { get; private set; }

        public static LanguageInfo CurrentLanguage { get; private set; }
        
        static CustomLanguageLogic()
        {
            Languages = new List<CustomLanguageInfo>();

            var enUS = new CustomLanguageInfo { Key = "en-US", ImageUrl = "english.gif" };
            var daDK = new CustomLanguageInfo
            {                
                Key = "da-DK",
                ImageUrl = "danish.gif",
                Fallbacks = new List<LanguageInfo> { enUS }.ToList()
            };
            var whatever = new CustomLanguageInfo
            {
                Key = "whatever",
                Culture = CultureInfo.GetCultureInfo("en-US"),
                ImageUrl = "whatever.gif",
                Fallbacks = new List<LanguageInfo> { enUS }.ToList()
            };

            CurrentLanguage = enUS;

            Languages = new[] {
                    enUS,
                    daDK,
                    whatever
                }.ToList();
        }


        public static void TryChangeLanguage(string key)
        {
            var lang = Languages.FirstOrDefault(x => x.Key == key);            
            if (lang != null)
            {
                CurrentLanguage = lang;                
            }
        }

        public static void Toggle(bool toggle)
        {
            var manager = (DefaultTextManager)LocalizationConfig.TextManager;

            if (toggle)
            {
                
                manager.CurrentLanguage = () => CurrentLanguage;                
            }
            else
            {
                manager.CurrentLanguage = MvcApplication.DefaultLanguageResolver;                   
            }
        }
    }

    public class CustomLanguageInfo : LanguageInfo
    {
        public string ID { get; set; }
        public string ImageUrl { get; set; }
        
        public string Whatever { get; set; }
    }
}