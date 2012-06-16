using System;
using System.Collections;
using Umbraco.Cms.Web.Configuration.UmbracoSystem;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.NodeName
{

    [UmbracoPropertyEditor]
    [PropertyEditor(CorePluginConstants.NodeNamePropertyEditorId, "NodeNameEditor", "Node Name Editor")]
    public class NodeNameEditor : PropertyEditor<NodeNameEditorModel>
    {
        private IUmbracoApplicationContext _appContext;

        public NodeNameEditor(IUmbracoApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public override NodeNameEditorModel CreateEditorModel()
        {
            Func<string, string> urlifyDelegate = (name) =>
            {
                var replacementDict = new global::System.Collections.Generic.Dictionary<string, string>();
                _appContext.Settings.Urls.CharReplacements.ForEach(x => replacementDict.Add(x.Char, x.Value));

                return name.ToUrlAlias(replacementDict, 
                    _appContext.Settings.Urls.RemoveDoubleDashes, 
                    _appContext.Settings.Urls.StripNonAscii,
                    _appContext.Settings.Urls.UrlEncode);
            };

            return new NodeNameEditorModel(urlifyDelegate);
        }

    }
}
