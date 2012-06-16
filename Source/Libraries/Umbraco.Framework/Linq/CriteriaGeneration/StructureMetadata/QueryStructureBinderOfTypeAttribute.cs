namespace Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata
{
    using System;

    using Umbraco.Framework.Linq.QueryModel;

    /// <summary>
    /// Used to identify that decorated classes should use the provided <see cref="AbstractQueryStructureBinder"/> when converting
    /// expressions containing the type to a <see cref="QueryDescription.Criteria"/> expression.
    /// </summary>
    /// <remarks></remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QueryStructureBinderOfTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Attribute"/> class.
        /// </summary>
        public QueryStructureBinderOfTypeAttribute(Type structureBinder)
        {
            StructureBinder = structureBinder;
        }

        /// <summary>
        /// Gets or sets the structure binder type.
        /// </summary>
        /// <value>The structure binder.</value>
        /// <remarks></remarks>
        public Type StructureBinder { get; protected set; }
    }
}
