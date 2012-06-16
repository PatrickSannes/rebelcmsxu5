using System;
using Examine;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    public class AttributeDefinitionToIndexFields : MemberMapper<AttributeDefinition, LazyDictionary<string, ItemField>>
    {
        public override LazyDictionary<string, ItemField> GetValue(AttributeDefinition source)
        {
            //convert the source to a dictionary object ignoring some of its properties
            var d = source.ToDictionary<AttributeDefinition, object, object>(
                x => x.Id,
                x => x.AttributeGroup,
                x => x.AttributeType,
                x => x.ConcurrencyToken);

            var output = new LazyDictionary<string, ItemField>();
            foreach (var i in d)
            {
                output.Add(i.Key, new ItemField(i.Value));
            }

            ExamineHelper.SetTimestampsOnEntityAndIndexFields(output, source);

            //add some lazy delegates for the ids of the group/attribute type
            output.Add(FixedIndexedFields.GroupId, new Lazy<ItemField>(() => new ItemField(source.AttributeGroup.Id.Value)));
            output.Add(FixedIndexedFields.AttributeTypeId, new Lazy<ItemField>(() => new ItemField(source.AttributeType.Id.Value)));

            return output;
        }
    }
}
