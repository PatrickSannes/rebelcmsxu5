using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web
{
    public static class EntityPathCollectionExtensions
    {
        /// <summary>
        /// Converts an entity path collection to a JSON object
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static JArray ToJson(this EntityPathCollection paths)
        {
            return new JArray(paths.Select(x => new JArray(x.Select(y => y.ToJsonObject()))));
        }
    }
}
