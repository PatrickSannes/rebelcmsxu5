using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.Tasks
{
    //[Task("78EB161C-002C-422F-8483-6C5C165E27E7", Tasks.ModelPreSendToView, ContinueOnFailure = false)]
    //public class TestEditModelTask : AbstractWebTask
    //{
    //    public TestEditModelTask(IUmbracoApplicationContext applicationContext) 
    //        : base(applicationContext)
    //    { }

    //    public override void Execute(TaskExecutionContext context)
    //    {
    //        var eventArgs = (ModelEventArgs)context.EventArgs.CallerEventArgs;
    //        if(eventArgs.Model is BasicContentEditorModel)
    //        {
    //            var model = (BasicContentEditorModel) eventArgs.Model;
    //            model.Tabs.Add(new Tab
    //            {
    //                Id = "test-tab".EncodeAsGuid(),
    //                Alias = "test-tab",
    //                Name = "TEST TAB",
    //                SortOrder = 100
    //            });

    //            var prop = model.Properties.SingleOrDefault(x => x.Alias == NodeNameAttributeDefinition.AliasValue);
    //            if(prop != null)
    //            {
    //                prop.TabId = "test-tab".EncodeAsGuid();
    //            }
    //        }
    //    }
    //}
}
