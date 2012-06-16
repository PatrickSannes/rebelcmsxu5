using System;
using System.IO;

namespace Umbraco.Framework.IO.Model
{
    [Obsolete]
    public class FSFileInfo : IFileInfo
    {
        /// <summary>
        /// Relative path to file, used as unique ID for the consuming file repository provider
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Underlying metadata about the FS item as exposed by System.IO.FileSystemInfo
        /// </summary>
        public FileSystemInfo FileInfo { get; set; }

        public string Name { get; set; }
        public string Location { get; set; }
        public byte[] Content { get; set; }

        /// <summary>
        /// Expresses whether item is a file or (if a common File-System equivalent) a directory
        /// </summary>
        public bool IsContainer { get; set; }

        public string AbsolutePath
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
