using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.NodeName
{
    [Bind(Exclude = "Urlify")]
    //[ModelBinder(typeof(NodeNameModelBinder))]
    public class NodeNameEditorModel : EditorModel
    {
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public Func<string, string> Urlify { get; protected set; }

        public NodeNameEditorModel(Func<string, string> urlifyDelegate)
        {
            Urlify = urlifyDelegate;
        }

        public override bool ShowUmbracoLabel
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the name of the node
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        public string Name { get; set; }

        private string _urlName;

        /// <summary>
        /// Gets or sets the name of the URL.
        /// </summary>
        /// <value>
        /// The name of the URL.
        /// </value>
        [Required]
        public string UrlName
        {
            get
            {
                //need to check if the UrlName has been set, if not, we will use the 'Name' to generate one
                if (string.IsNullOrEmpty(_urlName))
                {
                    _urlName = Urlify(Name);
                }
                return _urlName;
            }
            set
            {
                //always converts to UrlAlias
                _urlName = Urlify(value);
            }
        }


        /// <summary>
        /// Gets or sets the URL aliases.
        /// </summary>
        /// <value>
        /// The URL aliases.
        /// </value>
        public IEnumerable<string> UrlAliases { get; set; }

        public override IDictionary<string, object> GetSerializedValue()
        {
            return new Dictionary<string, object>
                {
                    { "Name", Name },
                    { "UrlName", UrlName },
                };
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            Name = serializedVal.GetValueAsString("Name");
            UrlName = serializedVal.GetValueAsString("UrlName");
        }
     
    }
}
