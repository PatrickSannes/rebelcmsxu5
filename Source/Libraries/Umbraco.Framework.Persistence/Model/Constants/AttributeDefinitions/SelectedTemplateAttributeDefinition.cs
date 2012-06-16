using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions
{

   

    public class SelectedTemplateAttributeDefinition : AttributeDefinition
    {

        public const string AliasValue = "system-internal-selected-template";

        public SelectedTemplateAttributeDefinition(AttributeGroup group)
        {
            this.Setup(AliasValue, "Selected template");
            this.AttributeType = AttributeTypeRegistry.Current.GetAttributeType(SelectedTemplateAttributeType.AliasValue) ?? new SelectedTemplateAttributeType();
            //this.AttributeType = new SelectedTemplateAttributeType();
            this.AttributeGroup = group;
        }
    }
}