using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Configuration;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker.DataSources
{
    public class ApplicationsListPickerDataSource : IListPickerDataSource
    {
        public IDictionary<string, string> GetData()
        {
            return UmbracoSettings.GetSettings().Applications.ToDictionary(x => x.Alias, y => y.Name);
        }
    }
}
