using System;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public interface IGroupUnit<out TFilter> : IDisposable, IUnit
        where TFilter : class, IProviderTypeFilter
    {
        Uri IdRoot { get; }
        IEntityRepositoryGroup<TFilter> Repositories { get; }
        bool WasAbandoned { get; }
    }
}