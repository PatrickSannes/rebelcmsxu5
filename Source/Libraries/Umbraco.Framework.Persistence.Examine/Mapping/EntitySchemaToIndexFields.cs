using Examine;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    public class EntitySchemaToIndexFields : MemberMapper<EntitySchema, LazyDictionary<string, ItemField>>
    {
        public override LazyDictionary<string, ItemField> GetValue(EntitySchema source)
        {
            //convert the source to a dictionary object ignoring some of its properties
            var d = source.ToDictionary<EntitySchema, object, object>(
                x => x.Id, 
                x => x.AttributeTypes, 
                x => x.AttributeDefinitions, 
                x => x.AttributeGroups,
                x => x.ConcurrencyToken, 
                x => x.RelationProxies);

            var output = new LazyDictionary<string, ItemField>();
            foreach (var i in d)
            {
                output.Add(i.Key, new ItemField(i.Value));
            }

            ExamineHelper.SetTimestampsOnEntityAndIndexFields(output, source);
            
            return output;
        }
    }
}