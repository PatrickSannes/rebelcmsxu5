using System;
using System.Collections.Generic;
using Examine;

using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    using Umbraco.Framework.Data;

    internal class TypedEntityToIndexFields : MemberMapper<TypedEntity, LazyDictionary<string, ItemField>>
    {
        public override LazyDictionary<string, ItemField> GetValue(TypedEntity source)
        {
            var d = new LazyDictionary<string, ItemField>();
            foreach (var a in source.Attributes)
            {
                if (a.Values.Count == 1)
                {
                    StoreFieldValue(a.AttributeDefinition.AttributeType.SerializationType,
                                    FixedAttributeIndexFields.AttributePrefix + a.AttributeDefinition.Alias,
                                    a.DynamicValue,
                                    d);                    
                }
                else if (a.Values.Count > 1)
                {
                    //we need to put multiple values in the index with dot notation
                    foreach (var val in a.Values)
                    {
                        var key = FixedAttributeIndexFields.AttributePrefix + a.AttributeDefinition.Alias + "." + val.Key;
                        StoreFieldValue(a.AttributeDefinition.AttributeType.SerializationType,
                                    key,
                                    val.Value,
                                    d);
                    }
                }
                
                //put attribute alias/name/id in there
                FixedAttributeIndexFields.AddAttributeAlias(d, a);
                //FixedAttributeIndexFields.AddAttributeName(d, a);
                FixedAttributeIndexFields.AddAttributeId(d, a);
            }

            //Get the parent Id.
            //TODO: We should support all relation types in some magical way
            foreach (var r in source.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType))
            {
                d.AddOrUpdate(FixedIndexedFields.ParentId, new ItemField(r.Item.SourceId.Value.ToString()));
            }

            d.AddOrUpdate(FixedIndexedFields.SchemaId,
                          new Lazy<ItemField>(() => new ItemField(source.EntitySchema.Id.Value.ToString())),
                          (k, i) => new Lazy<ItemField>(() => new ItemField(source.EntitySchema.Id.Value.ToString())));
            d.Add(FixedIndexedFields.SchemaAlias, new ItemField(source.EntitySchema.Alias));
            d.AddOrUpdate(FixedIndexedFields.SchemaName, new ItemField(source.EntitySchema.Name));
            d.AddOrUpdate(FixedIndexedFields.SchemaType, new ItemField(source.EntitySchema.SchemaType));

            ExamineHelper.SetTimestampsOnEntityAndIndexFields(d, source);
            return d;
        }

        /// <summary>
        /// Stores the value with the correct data type in Examine
        /// </summary>
        /// <param name="serializationDefinition"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <param name="d"></param>
        private void StoreFieldValue(IAttributeSerializationDefinition serializationDefinition, string key, object val, IDictionary<string, ItemField> d)
        {
            if (val == null)
            {
                return;
            }

            switch (serializationDefinition.DataSerializationType)
            {
                case DataSerializationTypes.Guid:
                case DataSerializationTypes.String:
                case DataSerializationTypes.LongString:
                case DataSerializationTypes.Boolean:
                    //through the serialization type is string, we still need to detect the 'real' type to see how to convert it to a string. For example,
                    // a date shouldn't just be a ToString() since we want to preserve as much information as possible.
                    var valType = val.GetType();
                    if (TypeFinder.IsTypeAssignableFrom<DateTime>(valType))
                    {
                        var dt = (DateTime) val;
                        //convert to the roundtrip date/time format http://msdn.microsoft.com/en-us/library/az4se3k1.aspx#Roundtrip
                        d.Add(key, new ItemField(dt.ToString("o")));
                    }
                    else if (TypeFinder.IsTypeAssignableFrom<DateTimeOffset>(valType))
                    {
                        var dt = (DateTimeOffset)val;
                        //convert to the roundtrip date/time format http://msdn.microsoft.com/en-us/library/az4se3k1.aspx#Roundtrip
                        d.Add(key, new ItemField(dt.ToString("o")));
                    }
                    else
                    {
                        d.Add(key, new ItemField(val.ToString()));    
                    }
                    break;
                case DataSerializationTypes.SmallInt:
                case DataSerializationTypes.LargeInt:
                    d.Add(key, new ItemField(val) { DataType = FieldDataType.Int });
                    break;
                case DataSerializationTypes.Decimal:
                    d.Add(key, new ItemField(val) { DataType = FieldDataType.Double });
                    break;
                case DataSerializationTypes.Date:
                    d.Add(key, new ItemField(val) { DataType = FieldDataType.DateTime });
                    break;                                
                case DataSerializationTypes.ByteArray:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
