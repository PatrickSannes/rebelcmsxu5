namespace Umbraco.Framework.Persistence.Model.Associations
{
    public interface IRelation<in TSource, in TDestination> : IRelationById
        where TSource : class, IRelatableEntity
        where TDestination : class, IRelatableEntity
    {
        TSource Source { set; }
        TDestination Destination { set; }
    }
}