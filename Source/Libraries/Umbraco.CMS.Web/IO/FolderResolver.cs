using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Umbraco.Cms.Web.IO
{
    public abstract class FolderResolver<T> : IResolver<T> where T : class
    {
        private readonly string _pattern;
        private readonly bool _deep;
        protected DirectoryInfo Folder { get; private set; }

        protected FolderResolver(DirectoryInfo folder, string pattern, bool deep)
        {
            _pattern = pattern;
            _deep = deep;
            Folder = folder;
        }

        public virtual IEnumerable<T> Resolve()
        {
            Folder.Refresh();
            return Folder.GetDirectories(_pattern, _deep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(GetItem)
                .Where(item => item != null)
                .ToList();
        }

        protected abstract T GetItem(DirectoryInfo folder);
    }
}