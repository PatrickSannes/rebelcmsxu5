using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.ProviderSupport
{
    public interface IReadonlyProviderUnit : IUnit
    {
        AbstractReadonlyEntityRepository EntityRepository { get; }
    }
}