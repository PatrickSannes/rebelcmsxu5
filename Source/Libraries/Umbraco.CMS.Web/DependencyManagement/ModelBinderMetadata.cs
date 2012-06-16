using System;
using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.DependencyManagement
{
    public class ModelBinderMetadata : MetadataComposition
    {
        public ModelBinderMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }

        public Type BinderType { get; set; }
    }
}