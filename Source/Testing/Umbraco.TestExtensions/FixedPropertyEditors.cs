using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.PropertyEditors.DateTimePicker;
using Umbraco.Cms.Web.PropertyEditors.Label;
using Umbraco.Cms.Web.PropertyEditors.ListPicker;
using Umbraco.Cms.Web.PropertyEditors.Numeric;
using Umbraco.Cms.Web.PropertyEditors.TreeNodePicker;
using Umbraco.Cms.Web.PropertyEditors.TrueFalse;
using Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.NodeName;
using Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.SelectedTemplate;
using Umbraco.Cms.Web.PropertyEditors.Upload;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Tests.Extensions.Stubs.PropertyEditors;
using Umbraco.Cms.Web;

namespace Umbraco.Tests.Extensions
{
    public class FixedPropertyEditors
    {
        public FixedPropertyEditors(IUmbracoApplicationContext fakeUmbracoApplicationContext)
        {
            var fakeBackOfficeContext = new FakeBackOfficeRequestContext(fakeUmbracoApplicationContext);

            _propertyEditors = new PropertyEditor[]
                {
                    new NonMandatoryPropertyEditor(Guid.Parse("D3DC1AC8-F83D-4D73-A13B-024E3100A600"), "rte", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("781FA5A4-C26D-4BBB-9F58-6CBBE0A9D14A"), "cust", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("3F5ED845-7018-4BDE-AB4E-C7106EE0992D"), "text", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("9CCB7060-3550-11E0-8A80-074CDFD72085"), "swatch", ""),
                    new NonMandatoryPropertyEditor(Guid.Parse("2B4DF3F1-C84E-4611-87EE-1D90ED437337"), "tags", ""),
                    new NonMandatoryPropertyEditor(),
                    new MandatoryPropertyEditor(),
                    new RegexPropertyEditor(),
                    new PreValueRegexPropertyEditor(),
                    //ensure node name is there
                    new NodeNameEditor(fakeBackOfficeContext.Application),
                    new SelectedTemplateEditor(fakeBackOfficeContext),
                    new UploadEditor(fakeBackOfficeContext),
                    new LabelEditor(),
                    new TrueFalseEditor(),
                    new DateTimePickerEditor(),
                    new NumericEditor(),
                    new TreeNodePicker(Enumerable.Empty<Lazy<TreeController, TreeMetadata>>()),
                    new ListPickerEditor(),
                    new TestContentAwarePropertyEditor()
                };
        }

        private readonly PropertyEditor[] _propertyEditors;

        public IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>> GetPropertyEditorDefinitions()
        {
            return _propertyEditors.ToPropertyEditorDefinitions();
        }

        public IEnumerable<PropertyEditor> GetPropertyEditors()
        {
            return _propertyEditors;
        }
    }
}