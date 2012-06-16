namespace Umbraco.Framework.Persistence.Abstractions.Attribution
{
    /// <summary>
    /// Represents a collection of <see cref="ITypedAttribute"/>
    /// </summary>
    public interface ITypedAttributeCollection : IPersistenceEntityCollection<ITypedAttribute>
    {
        // TODO: 2011 01 08: APN - ITypedAttributeCollection:
        // Originally ITypedAttributeCollection was an IEntityGraph but this has been removed pending removal of
        // circular reference between flat persistence and graphable persistence types

        ITypedAttribute this[string attributeName] { get; }
        ITypedAttribute Get(ITypedAttributeName attributeName);
        ITypedAttribute Get(string attributeName);
        bool Contains(string attributeName);
    }
}