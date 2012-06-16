namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Base interface for mapping members using MapUsing
    /// </summary>
    public interface IMemberMapper
    {
        object GetValue(object source);
    }
}