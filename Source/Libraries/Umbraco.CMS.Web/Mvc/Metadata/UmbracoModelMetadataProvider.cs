using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Localization.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.Metadata
{
    //TODO: We need to put some internal caching on the CreateMetadata method as it is called ALOT and we could seriously increase performance there!

    /// <summary>
    /// Umbraco meta data provider
    /// </summary>
    public class UmbracoModelMetadataProvider : LocalizingModelMetadataProvider
    {

       

        /// <summary>
        /// Performs some custom meta data binding based on custom attributes, etc...
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="containerType"></param>
        /// <param name="modelAccessor"></param>
        /// <param name="modelType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            //get the standard meta data
            var metadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);

            if (typeof(IMetadataAware).IsAssignableFrom(modelType) && modelAccessor != null)
            {
                ((IMetadataAware) modelAccessor()).OnMetadataCreated(metadata);
            }

            //check for embedded views
            string view;
            if (IsEmbeddedView(attributes, modelType, out view))
            {
                metadata.TemplateHint = view;
            }

            //check for hiding label
            if (!ShowLabel(attributes))
            {
                metadata.HideSurroundingHtml = true;
            }

            //check for AllowDocumentTypePropertyOverrideAttribute on pre-value models
            if (DocTypePropOverride(attributes))
            {
                //add to additional info
                metadata.AdditionalValues.Add(UmbracoMetadataAdditionalInfo.AllowDocumentTypePropertyOverride.ToString(), true);
            }

            return metadata;
        }

        private bool DocTypePropOverride(IEnumerable<Attribute> attributes)
        {
            var overrideAttribute = attributes.OfType<AllowDocumentTypePropertyOverrideAttribute>().FirstOrDefault();
            return overrideAttribute != null;
        }

        private bool ShowLabel(IEnumerable<Attribute> attributes)
        {
            var labelAttribute = attributes.OfType<ShowLabelAttribute>().FirstOrDefault();
            if (labelAttribute != null)
            {
                return labelAttribute.ShowLabel;
            }
            return true;
        }

        /// <summary>
        /// Checks for the EmbeddedViewAttribute
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="modelType"></param>
        /// <param name="viewPath">Returns the hash code for the view path</param>
        /// <returns></returns>
        /// <remarks>
        /// Because view paths can be quite long and we are limited to 260 characters based on the .Net framework, we need to 
        /// hash the URL, and store a reference to it. Then we can look up the hash again later. This will also improve performance
        /// when determining if a virtual URL is in fact a virtual URL.
        /// </remarks>
        protected bool IsEmbeddedView(IEnumerable<Attribute> attributes, Type modelType, out string viewPath)
        {
            Mandate.ParameterNotNull(modelType, "modelType");

            viewPath = EmbeddedViews.GetEmbeddedViewPath(
                attributes.OfType<EmbeddedViewAttribute>().FirstOrDefault());

            return viewPath != string.Empty;
        }
    
    }
}