namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Base class for mapping members using MapUsing
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TMember"></typeparam>
    public abstract class MemberMapper<TSource, TMember> : IMemberMapper
    {
        public abstract TMember GetValue(TSource source);

        object IMemberMapper.GetValue(object source)
        {
            return (TMember)GetValue((TSource)source);
        }
    }
}