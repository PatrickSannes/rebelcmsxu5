using System.Collections.Generic;

namespace Umbraco.Framework
{
    /// <summary>
    /// Equality comparer to compare to HiveIds based on whether to compare providers or not
    /// </summary>
    public class HiveIdComparer : IEqualityComparer<HiveId>
    {
        private readonly bool _ignoreProvider;

        public HiveIdComparer(bool ignoreProvider)
        {
            _ignoreProvider = ignoreProvider;
        }

        public bool Equals(HiveId x, HiveId y)
        {
            return _ignoreProvider
                       ? x.EqualsIgnoringProviderId(y)
                       : x.Equals(y);
        }

        public int GetHashCode(HiveId obj)
        {
            return obj.GetHashCode();
        }
    }
}