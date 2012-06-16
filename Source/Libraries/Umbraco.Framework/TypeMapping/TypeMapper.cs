namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// The class for any custom mapping operations between 2 types
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    public class TypeMapper<TSource, TTarget> : AbstractTypeMapper<TSource, TTarget>
    {
        public TypeMapper(AbstractFluentMappingEngine engine) : base(engine)
        {
        }
    }

}