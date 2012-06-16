using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Tests.Extensions
{
    public class MockedParameterEditorFactory : IParameterEditorFactory
    {
        public MockedParameterEditorFactory()
        {
            //_paramEditors = FixedPropertyEditors.GetPropertyEditorDefinitions();
        }

        private readonly IEnumerable<Lazy<AbstractParameterEditor, ParameterEditorMetadata>> _paramEditors;

        public Lazy<AbstractParameterEditor, ParameterEditorMetadata> GetParameterEditor(Guid id)
        {
            return _paramEditors.GetParameterEditor(id);
        }
    }
}
