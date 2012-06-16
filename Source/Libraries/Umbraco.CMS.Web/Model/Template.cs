using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model
{
    public class Template : AttributedNodeWithAlias
    {
        public Template(HiveId id)
        {
            Id = id;
        }

        public override string NodeType { get { return "Template"; } }
    }
}