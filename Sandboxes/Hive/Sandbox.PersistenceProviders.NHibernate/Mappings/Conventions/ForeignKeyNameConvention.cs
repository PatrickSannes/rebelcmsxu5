using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Sandbox.PersistenceProviders.NHibernate.Mappings.Conventions
{
    public class ForeignKeyNameConvention : IHasManyConvention
    {
        public void Apply(IOneToManyCollectionInstance instance)
        {
            instance.Key.Column(instance.EntityType.Name + "Id");
        }
    }
}