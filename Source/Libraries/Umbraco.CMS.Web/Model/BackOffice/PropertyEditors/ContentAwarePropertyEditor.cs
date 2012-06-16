using System;
using System.Linq.Expressions;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{
    /// <summary>
    /// Abstract property editor class to inherit from if the editor requires the currently rendering content properties if they are available
    /// and does not require a pre value model
    /// </summary>
    /// <typeparam name="TEditorModel"></typeparam>
    public abstract class ContentAwarePropertyEditor<TEditorModel> : ContentAwarePropertyEditor<TEditorModel, BlankPreValueModel>
        where TEditorModel : EditorModel
    {
        public override BlankPreValueModel CreatePreValueEditorModel()
        {
            return new BlankPreValueModel();
        }

        public sealed override TEditorModel CreateEditorModel(BlankPreValueModel preValues)
        {
            return CreateEditorModel();
        }

        public abstract TEditorModel CreateEditorModel();
    }

    /// <summary>
    /// Abstract property editor class to inherit from if the editor requires the currently rendering content properties if they are available
    /// </summary>
    /// <typeparam name="TEditorModel"></typeparam>
    /// <typeparam name="TPreValueModel"></typeparam>
    public abstract class ContentAwarePropertyEditor<TEditorModel, TPreValueModel> : PropertyEditor<TEditorModel, TPreValueModel>, IContentAwarePropertyEditor
        where TEditorModel : EditorModel
        where TPreValueModel : PreValueModel
    {

        private ContentProperty _property;
        private BasicContentEditorModel _contentItem;
        private DocumentTypeEditorModel _docType;

        /// <summary>
        /// Sets the document type.
        /// </summary>
        /// <param name="docType">document type.</param>
        void IContentAwarePropertyEditor.SetDocumentType(DocumentTypeEditorModel docType)
        {
            _docType = docType;
        }

        /// <summary>
        /// Sets the content property.
        /// </summary>
        /// <param name="property">The property.</param>
        void IContentAwarePropertyEditor.SetContentProperty(ContentProperty property)
        {
            _property = property;
        }


        /// <summary>
        /// Sets the content item.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        void IContentAwarePropertyEditor.SetContentItem(BasicContentEditorModel contentItem)
        {
            _contentItem = contentItem;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is rendering in the context of a Document type
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has a document type available; otherwise, <c>false</c>.
        /// </value>
        public bool IsDocumentTypeAvailable
        {
            get { return _docType != null; }
        }

        /// <summary>
        /// Returns true if the property editor is rendering in the context of a ContentProperty
        /// </summary>
        public bool IsContentPropertyAvailable
        {
            get { return _property != null; }
        }

        /// <summary>
        /// Returns true if the property editor is rendering in the context of a BasicContentEditorModel
        /// </summary>
        public bool IsContentModelAvailable
        {
            get { return _contentItem != null; }
        }

        /// <summary>
        /// Gets the document type value.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propSelector">The prop selector.</param>
        /// <param name="ifNullOrUnavailable">If null or unavailable.</param>
        /// <returns></returns>
        public TProperty GetDocumentTypeValue<TProperty>(Expression<Func<DocumentTypeEditorModel, TProperty>> propSelector, TProperty ifNullOrUnavailable)
        {
            if (_property == null)
            {
                return ifNullOrUnavailable;
            }
            var propToSet = _docType.GetPropertyInfo(propSelector);
            var propVal = propToSet.GetValue(_docType, null);
            if (propVal == null)
            {
                return ifNullOrUnavailable;
            }
            return (TProperty)propVal;
        }

        /// <summary>
        /// The method to call to access the properties of the currently rendering ContentProperty
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propSelector"></param>
        /// <param name="ifNullOrUnavailable"></param>
        /// <returns></returns>
        public TProperty GetContentPropertyValue<TProperty>(Expression<Func<ContentProperty, TProperty>> propSelector, TProperty ifNullOrUnavailable)
        {
            if (_property == null)
            {
                return ifNullOrUnavailable;
            }
            var propToSet = _property.GetPropertyInfo(propSelector);
            var propVal = propToSet.GetValue(_property, null);
            if (propVal == null)
            {
                return ifNullOrUnavailable;
            }
            return (TProperty)propVal;
        }

        /// <summary>
        /// The method to call to access the properties of the currently rendering BasicContentEditorModel
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propSelector"></param>
        /// <param name="ifNullOrUnavailable">The value to return if the property value or the Content item itself is null</param>
        /// <returns></returns>
        /// <remarks>
        /// The content item will be null if property editor is rendering for new unsaved content
        /// </remarks>
        public TProperty GetContentModelValue<TProperty>(Expression<Func<BasicContentEditorModel, TProperty>> propSelector, TProperty ifNullOrUnavailable)
        {
            if (_contentItem == null)
            {
                return ifNullOrUnavailable;
            }
            var propToSet = _contentItem.GetPropertyInfo(propSelector);
            var propVal = propToSet.GetValue(_contentItem, null);
            if (propVal == null)
            {
                return ifNullOrUnavailable;
            }
            return (TProperty)propVal;
        }

        /// <summary>
        /// The method to call to access the properties of the currently rendering BasicContentEditorModel
        /// </summary>
        /// <typeparam name="TContentModel">The expected content model of the currently rendering content item</typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propSelector"></param>
        /// <param name="ifNullOrUnavailable">The value to return if the property value or the Content item itself is null</param>
        /// <returns></returns>
        public TProperty GetContentModelValue<TContentModel, TProperty>(Expression<Func<TContentModel, TProperty>> propSelector, TProperty ifNullOrUnavailable)
            where TContentModel : BasicContentEditorModel
        {
            var castedModel = _contentItem as TContentModel;
            if (castedModel == null)
                throw new InvalidCastException("Cannot cast from type " + typeof(BasicContentEditorModel).Name + " to " + typeof(TContentModel).Name);

            if (_contentItem == null)
            {
                return ifNullOrUnavailable;
            }
            var propToSet = castedModel.GetPropertyInfo(propSelector);
            var propVal = propToSet.GetValue(castedModel, null);
            if (propVal == null)
            {
                return ifNullOrUnavailable;
            }
            return (TProperty)propVal;
        }

    }
}