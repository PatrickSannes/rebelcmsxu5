using System.Collections.Generic;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.IO;

namespace Umbraco.Framework.IO
{
    //TODO: Decide if this is the right location to keep this file
    public static class EntityRelationCollectionExtensions
    {
        /// <summary>
        /// Gets the parent of the current Content entity relationships
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static File ParentAsFile(this EntityRelationCollection collection)
        {
            Mandate.ParameterNotNull(collection, "collection");

            return collection.Parent<File>(FixedRelationTypes.FileRelationType);
        }

        /// <summary>
        /// Gets the children of the current Content entity relationships
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<File> ChildrenAsFile(this EntityRelationCollection collection)
        {
            Mandate.ParameterNotNull(collection, "collection");

            return collection.Children<File>(FixedRelationTypes.FileRelationType);
        }

        /// <summary>
        /// Gets the ancestors of the current Content entity relationships
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<File> AncestorsAsFile(this EntityRelationCollection collection)
        {
            Mandate.ParameterNotNull(collection, "collection");

            return collection.Ancestors<File>(FixedRelationTypes.FileRelationType);
        }

        /// <summary>
        /// Gets the ancestors or self of the current Content entity relationships
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<File> AncestorsOrSelfAsFile(this EntityRelationCollection collection)
        {
            Mandate.ParameterNotNull(collection, "collection");

            return collection.AncestorsOrSelf<File>(FixedRelationTypes.FileRelationType);
        }

        /// <summary>
        /// Gets the decendents or self of the current Content entity relationships
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<File> DescendentsOrSelfAsFile(this EntityRelationCollection collection)
        {
            Mandate.ParameterNotNull(collection, "collection");

            return collection.DescendentsOrSelf<File>(FixedRelationTypes.FileRelationType);
        }
    }
}
