using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.RepositoryTypes
{
    [RepositoryType("dictionary://")]
    public interface IDictionaryStore : IProviderTypeFilter
    {

    }
}
