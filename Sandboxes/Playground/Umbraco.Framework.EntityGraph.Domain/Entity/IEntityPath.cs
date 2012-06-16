namespace Umbraco.Framework.EntityGraph.Domain.Entity
{
    /// <summary>
    ///   Represents the path of an entity relative to the root
    /// </summary>
    public interface IEntityPath
    {
        /// <summary>
        ///   The path of the IEntity relative to the root of the entire system
        /// </summary>
        string Absolute { get; set; }

        /// <summary>
        ///   The path of the IEntity relative to the root IEntity of its repository
        /// </summary>
        string RepositoryRootRelative { get; set; }

        /// <summary>
        ///   Each part of the Absolute path split on the <see cref = "PathDelimiter" />
        /// </summary>
        string[] Parts { get; }

        /// <summary>
        ///   The path delimiter, e.g. the character '/'
        /// </summary>
        string PathDelimiter { get; }
    }
}