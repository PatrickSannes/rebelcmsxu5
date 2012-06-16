using System;
using Umbraco.Framework.Context;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public interface IReadonlyGroupUnit<out TFilter> : IDisposable, IUnit, IRequiresFrameworkContext
        where TFilter : class, IProviderTypeFilter
    {
        Uri IdRoot { get; }
        IReadonlyEntityRepositoryGroup<TFilter> Repositories { get; }
    }
}