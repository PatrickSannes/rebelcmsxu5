using System.Diagnostics.Contracts;

namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    [ContractClassFor(typeof (AttributionSchemaDefinition))]
    internal abstract class AttributionSchemaDefinitionCodeContract :
        AttributionSchemaDefinition
    {
        [ContractInvariantMethod]
        protected override void ObjectInvariant()
        {
            base.ObjectInvariant();
            Contract.Invariant(!string.IsNullOrWhiteSpace(Alias));
        }
    }
}