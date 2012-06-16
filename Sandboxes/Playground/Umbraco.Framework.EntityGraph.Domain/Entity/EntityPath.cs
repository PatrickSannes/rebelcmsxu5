namespace Umbraco.Framework.EntityGraph.Domain.Entity
{
    /// <summary>
    /// Represents the path of an entity relative to the root
    /// </summary>
    public class EntityPath : IEntityPath
    {
        #region Implementation of IEntityPath

        /// <summary>
        /// The path of the IEntity relative to the root of the entire system
        /// </summary>
        public string Absolute { get; set; }

        /// <summary>
        /// The path of the IEntity relative to the root IEntity of its repository
        /// </summary>
        public string RepositoryRootRelative { get; set; }

        /// <summary>
        /// Each part of the Absolute path split on the <see cref="IEntityPath.PathDelimiter"/>
        /// </summary>
        public string[] Parts { get; private set; }

        /// <summary>
        /// The path delimiter, e.g. the character '/'
        /// </summary>
        public string PathDelimiter { get; private set; }

        #endregion
    }
}