using System.Collections.Generic;

namespace Umbraco.Cms.Web.Model
{
    public abstract class AttributedNode : Node
    {
        protected AttributedNode()
        {
            Fields = new HashSet<Field>();
        }

        public IEnumerable<Field> Fields { get; protected set; }
    }
}
