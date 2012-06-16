using Examine;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    public class AttributeTypeToIndexFields : MemberMapper<AttributeType, LazyDictionary<string, ItemField>>
    {
        private readonly ExamineHelper _helper;

        public AttributeTypeToIndexFields(ExamineHelper helper)
        {
            _helper = helper;
        }

        public override LazyDictionary<string, ItemField> GetValue(AttributeType source)
        {

            ////TODO: This is purely a hack in order to get the installer working with Examine, its also a required hack until the underlying 
            //// issue is fixed: Currently we need to make sure that the properties of an Attribute type remain constant in the repository unless
            //// that property is explicitly changed. This is mostly to do with the RenderTypeProvider property as we don't want it to be overwritten
            //// with a empty value when a system attribute type is committed.
            //if (!source.Id.IsNullValueOrEmpty())
            //{                
            //    var existing = _helper.PerformGet<AttributeType>(true, LuceneIndexer.IndexNodeIdFieldName, source.Id);
            //    var destination = existing.LastOrDefault();
            //    if (destination != null)
            //    {
            //        //This copies across all changed (Dirty) properties from the source to the destination except for a few of the properties
            //        destination.SetAllChangedProperties<AttributeType, object>(source,
            //                                            x => x.Id,
            //                                            x => x.ConcurrencyToken,
            //                                            x => x.PersistenceMetadata,
            //                                            x => x.UtcCreated,
            //                                            x => x.UtcModified,
            //                                            x => x.UtcStatusChanged);
            //        //now that all properties are updated that have changed, overwrite the 'source' object with the destination so it can be mapped
            //        //to the index fields properly to be stored.
            //        source = destination;
            //    }
            //}

            //convert the source to a dictionary object ignoring some of its properties
            var d = source.ToDictionary<AttributeType, object, object>(
                x => x.Id,
                x => x.SerializationType,
                x => x.ConcurrencyToken);

            //add the serialization FQN so we can re-create it later
            d.Add(FixedIndexedFields.SerializationType, source.SerializationType.GetType().FullName);

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