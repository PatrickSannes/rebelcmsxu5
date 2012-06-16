using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.Upload
{
    [PropertyEditor(CorePluginConstants.FileUploadPropertyEditorId, "Upload", "Upload", IsParameterEditor = true)]
    public class UploadEditor : ContentAwarePropertyEditor<UploadEditorModel, UploadPreValueModel>
    {
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadEditor"/> class.
        /// </summary>
        /// <param name="backOfficeRequestContext">The back office request context.</param>
        public UploadEditor(IBackOfficeRequestContext backOfficeRequestContext)
        {
            _backOfficeRequestContext = backOfficeRequestContext;
        }

        public override UploadEditorModel CreateEditorModel(UploadPreValueModel preValues)
        {
            return new UploadEditorModel(preValues, _backOfficeRequestContext, 
                GetContentModelValue(x => x.Id, HiveId.Empty),
                GetContentPropertyValue(x => x.Alias, ""));
        }

        public override UploadPreValueModel CreatePreValueEditorModel()
        {
            return new UploadPreValueModel();
        }

    }
}
