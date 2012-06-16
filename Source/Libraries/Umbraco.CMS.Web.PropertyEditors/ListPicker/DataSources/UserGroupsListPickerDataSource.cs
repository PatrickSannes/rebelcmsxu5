using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;

using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker.DataSources
{
    public class UserGroupsListPickerDataSource : IListPickerDataSource
    {
        public IDictionary<string, string> GetData()
        {
            var data = new Dictionary<string, string>();

            var context = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            using (var uow = context.Hive.OpenReader<ISecurityStore>(new Uri("security://user-groups")))
            {
                var items = uow.Repositories.GetEntityByRelationType<TypedEntity>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                    .OrderBy(x => x.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue)
                    .ToArray();

                foreach (var typedEntity in items)
                {
                    data.Add(typedEntity.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue, typedEntity.Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue);
                }
            }

            return data;
        }
    }
}
