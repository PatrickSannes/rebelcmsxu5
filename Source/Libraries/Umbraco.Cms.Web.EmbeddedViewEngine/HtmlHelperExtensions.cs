using System.Web.Mvc;

namespace Umbraco.Cms.Web.EmbeddedViewEngine
{
    public static class HtmlHelperExtensions
    {

        /// <summary>
        /// Returns the embedded view path for the model specified or an empty string if the embedded view attribute was not found
        /// </summary>
        /// <param name="html"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetEmbeddedViewPath(this HtmlHelper html, dynamic model)
        {
            return EmbeddedViews.GetEmbeddedViewPath(model);
        }

        public static string GetEmbeddedViewPath(this HtmlHelper html, string viewPath, string assembly)
        {
            return EmbeddedViewPath.Create(viewPath, assembly);
        }

    }
}
