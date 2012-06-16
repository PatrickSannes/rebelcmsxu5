using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.IO
{
    /// <summary>
    ///   Abstract class to resolve object based on the file system
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    public abstract class FileResolver<T> : IResolver<T> where T : class
    {

        protected abstract DirectoryInfo RootFolder { get; }
        protected abstract IEnumerable<string> AllowedExtensions { get; }

        #region IResolver<T> Members

        public virtual IEnumerable<T> Resolve()
        {
            var files = AllowedExtensions
                .SelectMany(x => RootFolder.GetFiles(x, SearchOption.AllDirectories))                
                .Select(GetItem)
                .WhereNotNull();
            return files;            
        }

        #endregion

        protected abstract T GetItem(FileInfo file);
    }
}