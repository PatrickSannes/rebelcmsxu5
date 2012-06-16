using Umbraco.Framework.Persistence.Model.Associations;

namespace Umbraco.Framework.Persistence.Examine
{
    internal static class RelationExtensions
    {
        /// <summary>
        /// Returns a composite Id to be used for a relation
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        internal static string GetCompositeId(this IRelationById relation)
        {
            Mandate.ParameterNotEmpty(relation.SourceId, "relation.SourceId");
            Mandate.ParameterNotEmpty(relation.DestinationId, "relation.DestinationId");
            Mandate.ParameterNotEmpty(relation.SourceId.Value, "relation.SourceId.Value");
            Mandate.ParameterNotEmpty(relation.DestinationId.Value, "relation.SourceId.Value");
            Mandate.ParameterNotNull(relation.Type, "relation.Type");
            Mandate.ParameterNotNullOrEmpty(relation.Type.RelationName, "relation.Type.RelationName");

            return relation.SourceId.Value + "," + relation.DestinationId.Value + "," + relation.Type.RelationName;
        }

    }
}