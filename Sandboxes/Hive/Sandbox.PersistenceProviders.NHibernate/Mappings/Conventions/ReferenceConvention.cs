using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Sandbox.PersistenceProviders.NHibernate.Mappings.Conventions
{
    public class ReferenceConvention : IReferenceConvention
    {
        public void Apply(IManyToOneInstance instance)
        {
            instance.Column(instance.Property.Name + "Id");
        }
    }
}