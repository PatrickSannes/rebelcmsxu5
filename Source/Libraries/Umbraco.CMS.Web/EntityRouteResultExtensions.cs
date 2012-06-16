using Umbraco.Cms.Web.Routing;

namespace Umbraco.Cms.Web
{
    public static class EntityRouteResultExtensions
    {
        /// <summary>
        /// If the entity is routable (didn't return an error status)
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool IsRoutable(this EntityRouteResult result)
        {
            return (int)result.Status < 10;
        }
    }
}