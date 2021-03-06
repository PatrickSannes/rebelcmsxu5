﻿using System;
using System.IO;
using System.Reflection;

namespace Umbraco.Framework.Testing
{
    /// <summary>
    /// Common helper properties and methods useful to testing
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Gets the current assembly directory.
        /// </summary>
        /// <value>The assembly directory.</value>
        static public string CurrentAssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetCallingAssembly().CodeBase;
                var uri = new Uri(codeBase);
                var path = uri.LocalPath;
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Maps the given <paramref name="relativePath"/> making it rooted on <see cref="CurrentAssemblyDirectory"/>. <paramref name="relativePath"/> must start with <code>~/</code>
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns></returns>
        public static string MapPathForTest(string relativePath)
        {
            Mandate.ParameterCondition(relativePath.StartsWith("~/"), "relativePath");

            return relativePath.Replace("~/", CurrentAssemblyDirectory + "/");
        }
    }
}
