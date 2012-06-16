using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Sandbox.PersistenceProviders.NHibernate.Mappings.Conventions
{
    public class PrimaryKeyNameConvention : IIdConvention
    {
        public void Apply(IIdentityInstance instance)
        {
            instance.Column(instance.EntityType.Name + "Id");
        }
    }
}