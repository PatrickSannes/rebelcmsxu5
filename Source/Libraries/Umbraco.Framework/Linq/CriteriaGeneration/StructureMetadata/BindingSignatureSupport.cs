namespace Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    /// <summary>
    /// Provides metadata to an expression tree binder in order to identify which types of methods or members are denoted by which <see cref="ValuePredicateType"/>
    /// by returning a <see cref="SignatureSupportType"/>.
    /// </summary>
    /// <remarks></remarks>
    public class BindingSignatureSupport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingSignatureSupport"/> class.
        /// </summary>
        /// <param name="signatureSupportType">Type of the signature support.</param>
        /// <param name="nodeType">Type of the node.</param>
        /// <remarks></remarks>
        public BindingSignatureSupport(SignatureSupportType signatureSupportType, ValuePredicateType nodeType)
        {
            SignatureSupportType = signatureSupportType;
            NodeType = nodeType;
        }

        /// <summary>
        /// Gets or sets the type of the signature support.
        /// </summary>
        /// <value>The type of the signature support.</value>
        /// <remarks></remarks>
        public SignatureSupportType SignatureSupportType { get; protected set; }

        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        /// <value>The type of the node.</value>
        /// <remarks></remarks>
        public ValuePredicateType NodeType { get; set; }
    }
}