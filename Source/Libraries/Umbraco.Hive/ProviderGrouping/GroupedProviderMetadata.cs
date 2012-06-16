using System.Collections.Generic;
using System.Collections.ObjectModel;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderGrouping
{
    public class GroupedProviderMetadata : KeyedCollection<string, ProviderMetadata>
    {
        public GroupedProviderMetadata(IEnumerable<ProviderSupport.AbstractProviderRepository> childSessions)
        {
            childSessions.ForEach(x => Add(x.ProviderMetadata));
        }

        protected override string GetKeyForItem(ProviderMetadata item)
        {
            return item.Alias;
        }
    }
}