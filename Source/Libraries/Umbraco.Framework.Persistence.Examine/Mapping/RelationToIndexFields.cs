using System;
using Examine;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    internal class RelationToIndexFields : MemberMapper<IReadonlyRelation<IRelatableEntity, IRelatableEntity>, LazyDictionary<string, ItemField>>
    {
        public override LazyDictionary<string, ItemField> GetValue(IReadonlyRelation<IRelatableEntity, IRelatableEntity> source)
        {
            var d = new LazyDictionary<string, ItemField>
                {
                    {FixedRelationIndexFields.SourceId, new Lazy<ItemField>(() => new ItemField(source.SourceId.Value.ToString()))}, 
                    {FixedRelationIndexFields.DestinationId, new Lazy<ItemField>(() => new ItemField(source.DestinationId.Value.ToString()))}, 
                    {FixedRelationIndexFields.RelationType, new ItemField(source.Type.RelationName)},
                    //{FixedRelationIndexFields.RelationSourceType, new ItemField(source.Source.GetType().AssemblyQualifiedName) },
                    {FixedIndexedFields.Ordinal, new ItemField(source.Ordinal.ToString()){ DataType = FieldDataType.Int } }
                };
            
            //store the metadata in the same document, but as keyed prefixed items like:
            // M.MyKey , MyValue
            foreach(var m in source.MetaData)
            {
                d.Add(FixedRelationIndexFields.MetadatumPrefix + m.Key, new ItemField(m.Value));
            }

            return d;
        }
    }
}