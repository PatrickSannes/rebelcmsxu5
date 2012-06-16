namespace Umbraco.Framework
{
    public static class Validate
    {
        /// <summary>
        /// Validates the length of the specified array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="minimumLength">The minimum length.</param>
        /// <param name="maximumLength">The maximum length.</param>
        /// <returns></returns>
        public static bool Array<T>(T[] source, int minimumLength, int maximumLength)
        {
            if (source.Length < minimumLength || source.Length > maximumLength)
            {
                return false;
            }

            return true;
        }

        public static bool IsNull(object source)
        {
            return source == null;
        }
    }
}