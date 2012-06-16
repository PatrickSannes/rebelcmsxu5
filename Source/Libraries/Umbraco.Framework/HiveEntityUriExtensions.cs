namespace Umbraco.Framework
{
    ///<summary>
    /// Extension methods for HiveEntityUri
    ///</summary>
    public static class HiveEntityUriExtensions
    {
        /// <summary>
        /// Returns null if the id is null or empty, otherwise returns the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static HiveEntityUri ConvertToNullIfEmpty(this HiveEntityUri id)
        {
            return HiveEntityUri.IsNullOrEmpty(id) ? null : id;
        }

        /// <summary>
        /// Extension method to do IsNullOrEmpty fluently
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this HiveEntityUri id)
        {
            return HiveEntityUri.IsNullOrEmpty(id);
        }

    }
}