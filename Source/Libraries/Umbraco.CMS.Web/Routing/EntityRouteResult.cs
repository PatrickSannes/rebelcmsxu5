using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// The result that is returned when an entity is found based on a URL
    /// </summary>
    public class EntityRouteResult
    {
        public EntityRouteResult(TypedEntity entity, EntityRouteStatus status)
        {
            Status = status;
            RoutableEntity = entity;
        }

        public TypedEntity RoutableEntity { get; private set; }
        public EntityRouteStatus Status { get; private set; }
    }
}