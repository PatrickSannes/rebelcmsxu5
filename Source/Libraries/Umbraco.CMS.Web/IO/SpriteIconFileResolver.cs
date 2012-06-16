using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice;


namespace Umbraco.Cms.Web.IO
{
    public abstract class SpriteIconFileResolver : AbstractIconFileResolver
    {
        protected SpriteIconFileResolver(DirectoryInfo folderPath, UrlHelper url)
            : base(folderPath, url)
        {
        }

        private readonly List<KeyValuePair<FileInfo, FileInfo>> _sprites
            = new List<KeyValuePair<FileInfo, FileInfo>>();
        
        private bool _hasResolved = false;
        private FileInfo[] _resolvedFiles;

        public List<KeyValuePair<FileInfo, FileInfo>> Sprites
        {
            get
            {
                if (!_hasResolved)
                {
                    //force Resolve to load sprites
                    Resolve();
                }
                return _sprites;
            }
        }

        public abstract string SpriteNamePrefix { get; }

        protected override Icon GetItem(FileInfo file)
        {
            if (file.Extension.Equals(".css", StringComparison.InvariantCultureIgnoreCase))
                return null;

            //first, see if we've already got this sprite definition
            if (_sprites.Any(x => x.Key == file)) return null;

            //see if there is an associated .CSS file for this current file

            //ensures we only resolve the files once
            if (_resolvedFiles == null)
            {
                _resolvedFiles = AllowedExtensions.SelectMany(x => RootFolder.GetFiles(x, SearchOption.AllDirectories)).ToArray();
            }

            var spriteDefinition = _resolvedFiles
                .Where(x => x.Name.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => Path.GetExtension(file.Name) != Path.GetExtension(x.Name))
                .Where(x => Path.GetFileNameWithoutExtension(file.Name) == Path.GetFileNameWithoutExtension(x.Name))
                .SingleOrDefault();

            if (spriteDefinition != null)
            {
                _sprites.Add(new KeyValuePair<FileInfo, FileInfo>(file, spriteDefinition));
                //since it's a Sprite, don't include it in the results
                return null;
            }

            return base.GetItem(file);
        }

        public override IEnumerable<Icon> Resolve()
        {
            //this flag is used to determine if sprites are loaded/being loaded because if the "Sprites" property is 
            //referenced without calling Resolve, then no sprites will be in the list
            _hasResolved = true;

            var icons = base.Resolve().ToList();

            //we have the raw icon files, now need to parse the CSS files for sprite names
            foreach (var s in _sprites)
            {
                icons.AddRange(GetSpriteNames(s.Value, SpriteNamePrefix));
            }

            //return the sorted version
            return icons.OrderBy(x => x.Name).ToArray();
        }

        protected IEnumerable<Icon> GetSpriteNames(FileInfo spriteDefinitions, string spriteNamePrefix)
        {
            using (var sr = new StreamReader(spriteDefinitions.OpenRead()))
            {
                var contents = sr.ReadToEnd();

                var regex = @"\." + spriteNamePrefix + @"([a-zA-Z].*?)\s*{";

                var result = Enumerable.Empty<string>();
                try
                {
                    var matches = Regex.Matches(contents, regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    result = matches.Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                return result.Select(x => new Icon { Name = x, IconType = IconType.Sprite });
            }
        }
    }
}