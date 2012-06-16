using System.Linq;

namespace Umbraco.Cms.Web.EmbeddedViewEngine
{
    /// <summary>
    /// A utility class
    /// </summary>
    public static class EmbeddedViews
    {
        /// <summary>
        /// Returns the embedded view attribute object for the model specified, or null if the attribute was not found
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EmbeddedViewAttribute GetEmbeddedView(object model)
        {
            return model == null ? null : model.GetType().GetCustomAttributes(false).OfType<EmbeddedViewAttribute>().FirstOrDefault();
        }

        /// <summary>
        /// Returns the embedded view path for the model specified or an empty string if the embedded view attribute was not found
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetEmbeddedViewPath(dynamic model)
        {
            if (model == null) return string.Empty;
            var a = GetEmbeddedView(model);
            return GetEmbeddedViewPath(a);
        }

        /// <summary>
        /// Returns the embedded view path for the model specified or an empty string if the embedded view attribute was not found
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string GetEmbeddedViewPath(EmbeddedViewAttribute a)
        {
            return a == null ? string.Empty : EmbeddedViewPath.Create(string.Concat(a.CompiledViewName, ",", a.AssemblyName));
        }
    }
}