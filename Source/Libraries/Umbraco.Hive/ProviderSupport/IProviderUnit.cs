using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.ProviderSupport
{
    public interface IProviderUnit : IUnit
    {
        AbstractEntityRepository EntityRepository { get; }
    }
}