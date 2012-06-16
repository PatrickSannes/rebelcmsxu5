using System;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using Umbraco.Foundation;

namespace Umbraco.Framework.Persistence.NHibernate.Mappings.DialectMitigation
{
    public class NormalisedTableNameConvention : IClassConvention, IClassConventionAcceptance
    {
        public void Accept(IAcceptanceCriteria<IClassInspector> criteria)
        {
            // Only run this if the table name is set
            //criteria.Expect(x => x.TableName, Is.Set);
            // Only run this if the table name contains dbo
            criteria.Expect(x => NameRequiresNormalisation(x.TableName));
        }

        public void Apply(IClassInstance instance)
        {
            instance.Schema("");
            var tableName = NormaliseName(instance.TableName);
            instance.Table(tableName);
        }

        private static string NormaliseName(string name)
        {
            Mandate.ParameterNotNullOrEmpty(name, "name");

            return name.Replace("[dbo].", "").TrimStart('[').TrimEnd(']');
        }

        private static bool NameRequiresNormalisation(string name)
        {
            Mandate.ParameterNotNullOrEmpty(name, "name");

            return name.IndexOf("dbo", StringComparison.InvariantCultureIgnoreCase) != 0;
        }
    }
}