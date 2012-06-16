namespace Umbraco.Framework.IO.Model
{
    /// <summary>
    /// Represents information about a file
    /// </summary>
    public interface IFileInfo
    {
        /// <summary>
        /// The name of the file
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The location of the file in the repository storage
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// The content of the file
        /// </summary>
        byte[] Content { get; set; }

        /// <summary>
        /// Repository-relative key for this file
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Expresses whether item is a file or (if a common File-System equivalent) a directory
        /// </summary>
        bool IsContainer { get; set; }

        /// <summary>
        /// The absolute path of the file from the root of the provider. Generally a web path
        /// </summary>
        string AbsolutePath { get; set; }
    }
}
