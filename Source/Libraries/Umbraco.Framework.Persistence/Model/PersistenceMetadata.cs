using Umbraco.Framework.Dynamics;

namespace Umbraco.Framework.Persistence.Model
{
    /// <summary>
    /// Represents metadata set by the provider responsible for persisting <see cref="AbstractEntity"/>.
    /// </summary>
    /// <remarks></remarks>
    public class PersistenceMetadata
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistenceMetadata"/> class.
        /// </summary>
        public PersistenceMetadata()
        {         
            SourceEntity = new BendyObject();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public PersistenceMetadata(string ownerProviderAlias, string returnedByProviderAlias)
        {
            OwnerProviderAlias = ownerProviderAlias;
            ReturnedByProviderAlias = returnedByProviderAlias;
            SourceEntity = new BendyObject();
        }

        public string OwnerProviderAlias { get; set; }

        public string ReturnedByProviderAlias { get; set; }

        /// <summary>
        /// Used to store the original source entity
        /// </summary>
        public BendyObject SourceEntity { get; set; }
    }
}