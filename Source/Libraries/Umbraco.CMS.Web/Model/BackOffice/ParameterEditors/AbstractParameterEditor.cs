using System;
using System.Linq;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.Model.BackOffice.ParameterEditors
{
    public abstract class AbstractParameterEditor
    {
        private IPropertyEditorFactory _propertyEditorFactory;
        private dynamic _propertyEditor;
        private dynamic _propertyEditorModel;

        /// <summary>
        /// Constructor for a ParameterEditor
        /// </summary>
        protected AbstractParameterEditor(IPropertyEditorFactory propertyEditorFactory)
        {
            //Locate the metadata attribute
            var paramEditorAttributes = GetType()
                .GetCustomAttributes(typeof(ParameterEditorAttribute), true)
                .OfType<ParameterEditorAttribute>();

            if (!paramEditorAttributes.Any())
                throw new InvalidOperationException(
                    string.Format("The ParameterEditor of type {0} is missing the {1} attribute", GetType().FullName,
                                  typeof(ParameterEditorAttribute).FullName));

            //assign the properties of this object to those of the metadata attribute
            var attr = paramEditorAttributes.First();
            Id = attr.Id;
            Name = attr.Name;
            Alias = attr.Alias;
            PropertyEditorId = attr.PropertyEditorId;

            _propertyEditorFactory = propertyEditorFactory;

            var propEditor = _propertyEditorFactory.GetPropertyEditor(PropertyEditorId);
            if(propEditor == null)
                throw new InvalidOperationException("Unable to find a Property Editor with the id '"+ PropertyEditorId +"'");
            if (!propEditor.Metadata.IsParameterEditor)
                throw new InvalidOperationException("Property Editor '"+ propEditor.Metadata.Name +"' is not a valid Parameter Editor type");
            _propertyEditor = propertyEditorFactory.GetPropertyEditor(PropertyEditorId).Value;
        }

        public virtual Guid Id { get; protected set; }

        //This method will probably become an IMappedIdentifier down the track
        public virtual string Name { get; protected set; }

        public virtual string Alias { get; protected set; }

        public virtual Guid PropertyEditorId { get; protected set; }

        public virtual string PropertyEditorPreValues { get; protected set; }

        public dynamic PropertyEditorModel
        {
            get
            {
                if(_propertyEditorModel == null)
                {
                    var preValueModel = _propertyEditor.CreatePreValueEditorModel();
                    if (!typeof(BlankPreValueModel).IsAssignableFrom(preValueModel.GetType()))
                        preValueModel.SetModelValues(PropertyEditorPreValues);

                    _propertyEditorModel = _propertyEditor.CreateEditorModel(preValueModel);
                }
                return _propertyEditorModel;
            }
        }
    }
}