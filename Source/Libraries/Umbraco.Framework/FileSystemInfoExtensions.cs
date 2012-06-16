using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Umbraco.Framework
{
    public static class FileSystemInfoExtensions
    {
        /// <summary>
        /// Calculates the FileSystemInfo object's relative path to a DirectoryInfo object
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string RelativePathOfItem(this DirectoryInfo directory, FileSystemInfo item)
        {
            Mandate.That<ArgumentNullException>(item != null);
            Mandate.That<ArgumentException>(item.FullName.Contains(directory.FullName));

            return item.FullName.Replace(directory.FullName, String.Empty).TrimStart(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Retreives all FileSystemInfo objects underneath a specified directory and all child directories recursively
        /// </summary>
        public static IEnumerable<FileSystemInfo> EnumerateFileSystemInfosRecursive(this DirectoryInfo directory)
        {
            return directory.EnumerateFileSystemInfos().Concat(directory.EnumerateDirectories().SelectMany(EnumerateFileSystemInfosRecursive));
        }

    }
}
