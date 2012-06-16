using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
using Umbraco.Cms;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice;

using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Drawing = System.Drawing;
using Icon = Umbraco.Cms.Web.Model.Icon;

namespace Umbraco.Tests.Extensions
{
    public class MockedIconFileResolver : SpriteIconFileResolver
    {
        readonly IEnumerable<Icon> _icons = new List<Icon>
            {
                new Icon{Name= "Icon1"},
                new Icon{Name = "Icon2"}
            };

        public MockedIconFileResolver()
            : base(null, null)
        {
        }

        public override IEnumerable<Icon> Resolve()
        {
            return _icons;
        }

        public override string SpriteNamePrefix
        {
            get { return "blah-"; }
        }

        protected override Icon GetItem(FileInfo file)
        {
            return new Icon() {IconType = IconType.Image, IconSize = new Size(10, 10), Name = "Blah"};
        }
    }
}