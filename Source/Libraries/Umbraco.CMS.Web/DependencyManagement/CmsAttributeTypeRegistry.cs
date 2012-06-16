using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Umbraco.Cms.Web.DependencyManagement
{
    /// <summary>
    /// An IAttributeTypeRegistry that already has the default AttributeTypes with their correct
    /// RenderTypeProviders setup without having to manually register them.
    /// </summary>
    public class CmsAttributeTypeRegistry : DefaultAttributeTypeRegistry
    {
        /// <summary>
        /// Registers all of the known Attribute types before any AttributeType is resolved or added.
        /// </summary>
        protected override void RegisterDefaultTypes()
        {
            base.RegisterDefaultTypes();
            RegisterUserAttributeTypes();
        }

        protected virtual void RegisterUserAttributeTypes()
        {
            RegisterAttributeType(() => new AttributeType("richTextEditor", "Rich Text Editor", "A WYSIWYG rich text editor", new LongStringSerializationType())
                        {
                            Id = new HiveId("rte-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.RichTextBoxPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='ShowLabel'><![CDATA[True]]></preValue>
    <preValue name='ShowContextMenu'><![CDATA[False]]></preValue>
    <preValue name='Size'><![CDATA[650x400]]></preValue>
    <preValue name='Features'><![CDATA[backcolor,bold,bullist,numlist,code,image,italic,underline,justifycenter,justifyfull,justifyleft,justifyright,umbracolink,unlink,umbracomacro,umbracomedia]]></preValue>
    <preValue name='Stylesheets'><![CDATA[]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("singleLineTextBox", "Textstring", "", new LongStringSerializationType())
                        {
                            Id = new HiveId("sltb-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.TextBoxPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[SingleLine]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("multiLineTextBox", "Textarea", "", new LongStringSerializationType())
                        {
                            Id = new HiveId("mltb-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.TextBoxPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[MultiLine]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("multiLineTextBoxWithControls", "Simple Editor", "", new LongStringSerializationType())
                        {
                            Id = new HiveId("mltbwc-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.TextBoxPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[MultiLineWithControls]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("colorSwatchPicker", "Color Swatch Picker", "", new StringSerializationType())
                        {
                            Id = new HiveId("csp-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.ColorSwatchPickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Colors'><![CDATA[#000000,#993300,#333300,#333399,#333333,#ff0099,#ff6600,#808000,#008000,#0000ff,#666699]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("tags", "Tags", "", new StringSerializationType())
                        {
                            Id = new HiveId("tag-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.TagsPropertyEditorId
                        });
            RegisterAttributeType(() => new AttributeType("contentPicker", "Content Picker", "", new StringSerializationType())
                        {
                            Id = new HiveId("content-picker-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.TreeNodePickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='TreeId'><![CDATA[" + CorePluginConstants.ContentTreeControllerId + @"]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("mediaPicker", "Media Picker", "", new StringSerializationType())
                        {
                            Id = new HiveId("media-picker-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.TreeNodePickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='TreeId'><![CDATA[" + CorePluginConstants.MediaTreeControllerId + @"]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("integer", "Integer", "", new StringSerializationType())
                        {
                            Id = new HiveId("integer-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.NumericPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='DecimalPlaces'><![CDATA[0]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("decimal", "Decimal", "", new StringSerializationType())
                        {
                            Id = new HiveId("decimal-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.NumericPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='DecimalPlaces'><![CDATA[2]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("uploader", "Uploader", "", new StringSerializationType())
                        {
                            Id = new HiveId("uploader-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.FileUploadPropertyEditorId
                        });
            RegisterAttributeType(() => new AttributeType("trueFalse", "True/False", "", new StringSerializationType())
                        {
                            Id = new HiveId("true-false-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.TrueFalsePropertyEditorId
                        });
            RegisterAttributeType(() => new AttributeType("dropdownList", "Dropdown List", "", new StringSerializationType())
                        {
                            Id = new HiveId("dropdown-list-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.ListPickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[DropDownList]]></preValue>
    <preValue name='ListItems'>
        <item id='option1'>Option 1</item>
        <item id='option2'>Option 2</item>
        <item id='option3'>Option 3</item>
    </preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("listBox", "List Box", "", new StringSerializationType())
                        {
                            Id = new HiveId("list-box-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.ListPickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[ListBox]]></preValue>
    <preValue name='ListItems'>
        <item id='option1'>Option 1</item>
        <item id='option2'>Option 2</item>
        <item id='option3'>Option 3</item>
    </preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("checkboxList", "Checkbox List", "", new StringSerializationType())
                        {
                            Id = new HiveId("checkbox-list-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.ListPickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[CheckboxList]]></preValue>
    <preValue name='ListItems'>
        <item id='option1'>Option 1</item>
        <item id='option2'>Option 2</item>
        <item id='option3'>Option 3</item>
    </preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("radioButtonList", "Radio Button List", "", new StringSerializationType())
                        {
                            Id = new HiveId("radio-button-list-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.ListPickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[RadioButtonList]]></preValue>
    <preValue name='ListItems'>
        <item id='option1'>Option 1</item>
        <item id='option2'>Option 2</item>
        <item id='option3'>Option 3</item>
    </preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("dateTimePicker", "Date Time Picker", "", new DateTimeSerializationType())
                        {
                            Id = new HiveId("date-time-picker-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.DateTimePickerPropertyEditorId,
                            RenderTypeProviderConfig = @"
<preValues>
    <preValue name='ShowTime'><![CDATA[False]]></preValue>
    <preValue name='IsRequired'><![CDATA[False]]></preValue>
</preValues>"
                        });
            RegisterAttributeType(() => new AttributeType("slider", "Slider", "", new StringSerializationType())
                        {
                            Id = new HiveId("slider-pe".EncodeAsGuid()),
                            RenderTypeProvider = CorePluginConstants.SliderPropertyEditorId,
                            RenderTypeProviderConfig = "{\"EnableRange\":false,\"RangeValue\":0,\"Value\":50,\"Value2\":0,\"MinValue\":0,\"MaxValue\":100,\"EnableStep\":true,\"StepValue\":5,\"Orientation\":0}"
                        });
            RegisterAttributeType(() => new AttributeType("label", "Label", "", new StringSerializationType())
            {
                Id = new HiveId("label-pe".EncodeAsGuid()),
                RenderTypeProvider = CorePluginConstants.LabelPropertyEditorId,
            });
        }
        protected override void RegisterSystemAttributeTypes()
        {
            RegisterAttributeType(() => new StringAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.TextBoxPropertyEditorId,
                RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[SingleLine]]></preValue>
</preValues>"
            });
            RegisterAttributeType(() => new TextAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.TextBoxPropertyEditorId,
                RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[MultiLine]]></preValue>
</preValues>"
            });
            RegisterAttributeType(() => new IntegerAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.NumericPropertyEditorId,
                RenderTypeProviderConfig = @"
<preValues>
    <preValue name='DecimalPlaces'><![CDATA[0]]></preValue>
</preValues>"
            });
            RegisterAttributeType(() => new DateTimeAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.DateTimePickerPropertyEditorId
            });
            RegisterAttributeType(() => new BoolAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.TrueFalsePropertyEditorId
            });
            RegisterAttributeType(() => new ReadOnlyAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.LabelPropertyEditorId
            });
            RegisterAttributeType(() => new ContentPickerAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.TreeNodePickerPropertyEditorId,
                RenderTypeProviderConfig = @"
<preValues>
    <preValue name='TreeId'><![CDATA[" + CorePluginConstants.ContentTreeControllerId + @"]]></preValue>
</preValues>"
            });
            RegisterAttributeType(() => new MediaPickerAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.TreeNodePickerPropertyEditorId,
                RenderTypeProviderConfig = @"
<preValues>
    <preValue name='TreeId'><![CDATA[" + CorePluginConstants.MediaTreeControllerId + @"]]></preValue>
</preValues>"
            });

            RegisterAttributeType(() => new ApplicationsListPickerAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.ListPickerPropertyEditorId,
                RenderTypeProviderConfig = @"
<preValues>
    <preValue name='Mode'><![CDATA[CheckboxList]]></preValue>
    <preValue name='ListItems' dataSource='Umbraco.Cms.Web.PropertyEditors.ListPicker.DataSources.ApplicationsListPickerDataSource, Umbraco.Cms.Web.PropertyEditors' />
</preValues>"
            });
            RegisterAttributeType(() => new NodeNameAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.NodeNamePropertyEditorId
            });
            RegisterAttributeType(() => new SelectedTemplateAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.SelectedTemplatePropertyEditorId
            });
            RegisterAttributeType(() => new UserGroupUsersAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.UserGroupUsersPropertyEditorId
            });
            RegisterAttributeType(() => new FileUploadAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.FileUploadPropertyEditorId,
                RenderTypeProviderConfig = @"
<preValues>
    <preValue name='StoragePath'><![CDATA[/UploadedFiles/]]></preValue>
</preValues>"
            });
            RegisterAttributeType(() => new DictionaryItemTranslationsAttributeType()
            {
                RenderTypeProvider = CorePluginConstants.DictionaryItemTranslationsPropertyEditorId
            });
        }
    }
}
