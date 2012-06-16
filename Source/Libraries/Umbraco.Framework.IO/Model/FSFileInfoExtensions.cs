using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace Umbraco.Framework.IO.Model
{
    /// <summary>
    /// Extension methods for the internal FS implementation of IFileRepository
    /// </summary>
    [Obsolete]
    internal static class FSFileInfoExtensions
    {

        /// <summary>
        /// Provides an FSFileInfo relevant for a FileInfo and specified repository-relative key
        /// </summary>
        /// <param name="file"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static FSFileInfo ToFSFileInfo (this FileSystemInfo file, string key)
        {
            return new FSFileInfo
            {
                FileInfo = file,
                Key = key,
                IsContainer = file is DirectoryInfo
            };
        }

        /// <summary>
        /// Casts an IFileInfo object as an FSFileInfo, failing if the type is incorrect
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static FSFileInfo AsFSFileInfo(this IFileInfo key)
        {
            var item = key as FSFileInfo;
            Mandate.That<ArgumentException>(item!=null);
            return item;
        }

        /// <summary>   
        /// Fetches a list of all files and directories at the current path, recursively if requested
        /// </summary>
        /// <param name="recurse"></param>
        /// <returns></returns>
        public static IEnumerable<FSFileInfo> ReadFileList(this DirectoryInfo currentDirectory, bool recurse)
        {
            //retrieve items we are concerned with
            var files = (recurse) ? currentDirectory.EnumerateFileSystemInfosRecursive()
                                  : currentDirectory.EnumerateFileSystemInfos();

            //convert to IFileInfo
            return files.Select(file => file.ToFSFileInfo(currentDirectory.RelativePathOfItem(file)));
        }



    }
}
