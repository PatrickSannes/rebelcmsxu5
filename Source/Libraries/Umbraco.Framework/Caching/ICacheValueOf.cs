namespace Umbraco.Framework.Caching
{
    public interface ICacheValueOf<out T>
    {
        T Item { get; }
        ICachePolicy Policy { get; }
    }
}