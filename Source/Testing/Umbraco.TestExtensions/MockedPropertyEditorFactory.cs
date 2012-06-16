using System;
using System.Collections.Generic;
using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions
{
    public class MockedPropertyEditorFactory : IPropertyEditorFactory
    {
        public MockedPropertyEditorFactory(IUmbracoApplicationContext appContext)
        {
            _propEditors = new FixedPropertyEditors(appContext).GetPropertyEditorDefinitions();
        }

        private readonly IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>>  _propEditors;

        public Lazy<PropertyEditor, PropertyEditorMetadata> GetPropertyEditor(Guid id)
        {
            return _propEditors.GetPropertyEditor(id);
        }
    }
}