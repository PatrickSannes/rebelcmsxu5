namespace Umbraco.Framework.Persistence.Model.Associations
{
    public interface IReadonlyRelation<out TSource, out TDestination> : IRelationById
        where TSource : class, IRelatableEntity
        where TDestination : class, IRelatableEntity
    {
        TSource Source { get; }
        TDestination Destination { get; }
        bool EqualsIgnoringProviderId(IReadonlyRelation<IRelatableEntity, IRelatableEntity> other);
    }
}