using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.TextBox
{

    /// <summary>
    /// The model representing the pre-value editor for the TextBox property editor
    /// </summary>    
    public class TextBoxPreValueModel : PreValueModel
    {

        /// <summary>
        /// The text box mode
        /// </summary>
        [UIHint("EnumDropDownList")]
        public TextBoxMode Mode { get; set; }

        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }

        [AllowDocumentTypePropertyOverride]
        public string RegexValidationStatement { get; set; }

		[AllowDocumentTypePropertyOverride, DisplayName("Character Limit")]
		public int CharacterLimit { get; set; }

        public override void SetModelValues(string serializedVal)
        {
            if (!string.IsNullOrEmpty(serializedVal))
            {
                try
                {
                    var xml = XElement.Parse(serializedVal);

                    TextBoxMode parsed;
                    var firstModeFromXml = xml.Elements("preValue").FirstOrDefault(x => (string)x.Attribute("name") == "Mode");
                    var parsedEnum = Enum.TryParse<TextBoxMode>(firstModeFromXml != null ? firstModeFromXml.Value : TextBoxMode.SingleLine.ToString(), out parsed);
                    if (!parsedEnum) parsed = TextBoxMode.SingleLine;

                    Mode = parsed;

                    IsRequired = (bool)xml.Elements("preValue").Where(x => (string)x.Attribute("name") == "IsRequired").Single();

                    RegexValidationStatement = (string)xml.Elements("preValue").Where(x => (string)x.Attribute("name") == "RegexValidationStatement").Single();

					CharacterLimit = (int)xml.Elements("preValue").Where(x => (string)x.Attribute("name") == "CharacterLimit").Single();
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException
                        || ex is InvalidOperationException 
                        || ex is InvalidCastException)
                    {
                        //swallow exception, there's an issue with the XML... perhaps the editor model has changed... so we'll revert to defaults.
                    }
                    else
                    {
                        throw ex;    
                    }
                }
            }
			else
			{
				CharacterLimit = -1;
			}
        }
    }
}
