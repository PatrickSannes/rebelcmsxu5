namespace Umbraco.Framework.Persistence.Abstractions
{
    /// <summary>
    ///   Represents the path of an entity relative to the root
    /// </summary>
    public interface IPersistenceEntityPath
    {
        /// <summary>
        ///   The path of the IPersistenceEntity relative to the root of the entire system
        /// </summary>
        string Absolute { get; set; }

        /// <summary>
        ///   The path of the IPersistenceEntity relative to the root IPersistenceEntity of its repository
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