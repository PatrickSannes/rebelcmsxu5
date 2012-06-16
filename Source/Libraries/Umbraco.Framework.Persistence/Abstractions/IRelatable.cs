using Umbraco.Framework.Persistence.Model.Associations;

namespace Umbraco.Framework.Persistence.Abstractions
{
    public interface IRelatable
    {
        /// <summary>
        /// Gets relations for the current item.
        /// </summary>
        /// <remarks></remarks>
        EntityRelationCollection Relations { get; }
    }
}