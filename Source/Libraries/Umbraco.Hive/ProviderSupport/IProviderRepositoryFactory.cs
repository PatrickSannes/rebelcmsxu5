namespace Umbraco.Hive.ProviderSupport
{
    public interface IProviderRepositoryFactory<out T, out TReadonly>
        : IProviderReadonlyRepositoryFactory<TReadonly>
        where TReadonly : AbstractProviderRepository
        where T : AbstractProviderRepository
    {
        T GetRepository();
    }
}