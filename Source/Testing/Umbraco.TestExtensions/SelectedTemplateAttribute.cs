using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Umbraco.Tests.Extensions
{
    /// <summary>
    /// Represents an attribute storing the selected template id to use for the entity
    /// </summary>
    public class SelectedTemplateAttribute : TypedAttribute
    {
        public SelectedTemplateAttribute(HiveId templateId, AttributeGroup group)
            : base(new SelectedTemplateAttributeDefinition(group), templateId.IsNullValueOrEmpty() ? "" : templateId.ToString())
        { }
    }
}