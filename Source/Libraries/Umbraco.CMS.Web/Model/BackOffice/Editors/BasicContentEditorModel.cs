using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// An abstract View model for content/media
    /// </summary>
    [Bind(Exclude = "Tabs,Properties")]
    public abstract class BasicContentEditorModel : EditorModel, IModelBindAware
    {
        private bool _isEditable;

        protected BasicContentEditorModel()
        {
            Properties = new HashSet<ContentProperty>();

            //set the tab index to the first one
            ActiveTabIndex = 0;
            IsEditable = true;
        }

        /// <summary>
        /// By Default this is true, but when set to false the model should not be allowed to be saved. This 
        /// is generally only set to false when an item is in the Recycle Bin.
        /// </summary>
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;

                PopulateUIElements();
            }
        }

        /// <summary>
        /// Override the 'Name' property to lookup/retreive from the NodeName dynamic property of this object
        /// </summary>
        public override string Name
        {
            get { return GetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Name); }
            set { SetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Name = value); }
        }

        [ReadOnly(true)]
        public string DocumentTypeName { get; set; }

        [ReadOnly(true)]
        public string DocumentTypeAlias { get; set; }

        /// <summary>
        /// This is used when creating brand new content
        /// </summary>
        [HiddenInput]
        public HiveId DocumentTypeId { get; set; }

        /// <summary>
        /// The tabs on the content node (defined by the document type)
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public HashSet<Tab> Tabs { get; set; }

        /// <summary>
        /// Returns the tabs in the correct sort order and without the general group tab
        /// </summary>
        public IEnumerable<Tab> SortedTabs
        {
            get
            {
                return Tabs
                    .Where(x => x.Alias != FixedGroupDefinitions.GeneralGroupAlias)
                    .OrderBy(x => x.SortOrder)
                    .ToArray();
            }
        }

        /// <summary>
        /// The tab to display
        /// </summary>
        [HiddenInput]
        public int ActiveTabIndex { get; set; }

        //public ContentProperty NodeName { get; set; }

        /// <summary>
        /// The properties of the content node
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public HashSet<ContentProperty> Properties { get; set; }

        /// <summary>
        /// Binds the model with the IUpdator
        /// </summary>
        /// <param name="modelUpdator"></param>
        public void BindModel(IModelUpdator modelUpdator)
        {
            Mandate.ParameterNotNull(modelUpdator, "updator");

            //First, bind the dynamic properties, then the normal properties, this is because we are overriding the 'Name' 
            //property which comes from the NodeName dynamic property
            foreach (var contentNodeProperty in Properties.Where(x => x.PropertyEditorModel != null))
            {
                modelUpdator.BindModel(contentNodeProperty.PropertyEditorModel, contentNodeProperty.Id.GetHtmlId());
            }

            modelUpdator.BindModel(this, string.Empty);
        }

        /// <summary>
        /// Populates the UI elements
        /// </summary>
        protected virtual void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());

            if (IsEditable)
                UIElements.Add(new PublishButtonUIElement());
        }

        /// <summary>
        /// Gets the property editor model value.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="getValue">The get value.</param>
        /// <returns></returns>
        protected dynamic GetPropertyEditorModelValue(string alias, Func<dynamic, object> getValue)
        {
            var property = Properties.SingleOrDefault(x => x.Alias == alias);
            if (property != null && property.PropertyEditorModel != null)
            {
                return getValue(property.PropertyEditorModel);
            }
            return null;
        }

        /// <summary>
        /// Sets the property editor model value.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="setValue">The set value.</param>
        protected void SetPropertyEditorModelValue(string alias, Action<dynamic> setValue)
        {
            var property = Properties.SingleOrDefault(x => x.Alias == alias);
            if (property != null && property.PropertyEditorModel != null)
            {
                setValue(property.PropertyEditorModel);
            }
        }

    }
}
