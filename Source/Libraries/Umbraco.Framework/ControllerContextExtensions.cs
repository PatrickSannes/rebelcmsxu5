using System.Web.Mvc;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Localization.Web.Mvc;

namespace Umbraco.Framework
{
    public static class ControllerContextExtensions
    {
        public static TextManager TextManager(this ControllerContext ctx)
        {
            return LocalizationHelper.TextManager;
        }
    }
}
