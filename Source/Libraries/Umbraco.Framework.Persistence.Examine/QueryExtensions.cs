using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Hive;

namespace Umbraco.Framework.Persistence.Examine
{
    public static class QueryExtensions
    {
        /// <summary>
        /// Search based on a parent HiveId
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IBooleanOperation ParentHiveId(this IQuery criteria, HiveId id)
        {
            return criteria.HiveId(id, FixedIndexedFields.ParentId);
        }

        /// <summary>
        /// Search based on a HiveId which converts it to its value as a string
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IBooleanOperation HiveId(this IQuery criteria, HiveId id)
        {
            return criteria.Id(id.Value.ToString());
        }

        /// <summary>
        /// Searches for an Id in the specified field
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="id"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static IBooleanOperation Id(this IQuery criteria, string id, string fieldName)
        {
            return criteria.Field(fieldName, id.Escape());
        }

        /// <summary>
        /// Search for a HiveId in a particular field
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="id"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static IBooleanOperation HiveId(this IQuery criteria, HiveId id, string fieldName)
        {
            return criteria.Field(fieldName, id.Value.ToString().Escape());
        }

        /// <summary>
        /// Searches the index based on entity type which is stored in the Category 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public static IBooleanOperation EntityType<T>(this IQuery criteria)
        {
            //get the base type entity as the name
            return criteria.Field(LuceneIndexer.IndexCategoryFieldName, typeof (T).GetEntityBaseType().Name.Escape());
        }
    }
}
