using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData
{
    public class EntityGraphSchema : IEntityGraphSchema
    {
        public EntityGraphSchema()
        {
            PermittedDescendentTypes = new EntityCollection<IEntityTypeDefinition>();
            PermittedAssociationTypes = new EntityCollection<IEntityTypeDefinition>();
        }


        #region Implementation of IReferenceByAlias

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByAlias.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByAlias.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        public LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IEntityGraphSchema

        /// <summary>
        /// Gets a list of the permitted list of <see cref="IEntityTypeDefinition"/> allowed as descendents of a given <see cref="IEntityTypeDefinition"/>.
        /// </summary>
        /// <value>The permitted descendents.</value>
        public IEntityCollection<IEntityTypeDefinition> PermittedDescendentTypes { get; private set; }

        /// <summary>
        /// Gets the permitted association list of <see cref="IEntityTypeDefinition"/> allowed as associations of a given <see cref="IEntityTypeDefinition"/>.
        /// </summary>
        /// <value>The permitted association types.</value>
        public IEntityCollection<IEntityTypeDefinition> PermittedAssociationTypes { get; private set; }

        #endregion
    }
}