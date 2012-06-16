using System;
using System.Data;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;

using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    using Umbraco.Framework.Data;

    using Umbraco.Framework.Persistence.Model.Constants;

    internal class SearchResultToTypedEntity : TypeMapper<SearchResult, TypedEntity>
    {
        private readonly ExamineHelper _helper;

        public SearchResultToTypedEntity(ExamineHelper helper, AbstractFluentMappingEngine engine)
            : base(engine)
        {
            _helper = helper;
        }

        protected override void PerformMap(SearchResult source, TypedEntity target, MappingExecutionScope scope)
        {
            base.PerformMap(source, target, scope);

            //lookup the document type from examine

            var entitySchema = _helper.PerformGet<EntitySchema>(true, LuceneIndexer.IndexNodeIdFieldName, new HiveId(source.Fields[FixedIndexedFields.SchemaId])).ToArray();

            if (!entitySchema.Any())
                throw new DataException("Could not find an item in the index with id " + source.Fields[FixedIndexedFields.SchemaId]);
            target.EntitySchema = entitySchema.SingleOrDefault();

            var ancestorSchemaIds =
                _helper.PeformGetParentRelations(target.EntitySchema.Id, FixedRelationTypes.DefaultRelationType).
                    SelectRecursive(
                        x => _helper.PeformGetParentRelations(x.SourceId, FixedRelationTypes.DefaultRelationType)).
                    ToArray();
            if (ancestorSchemaIds.Any())
            {
                var ancestorSchemas = _helper.PerformGet<EntitySchema>(true, LuceneIndexer.IndexNodeIdFieldName, ancestorSchemaIds.Select(x => x.SourceId).ToArray()).ToArray();
                target.EntitySchema = new CompositeEntitySchema(target.EntitySchema, ancestorSchemas);
            }

            // We'll check this later if an attribute definition doesn't exist on the current schema so we can check parents
            var compositeSchema = target.EntitySchema as CompositeEntitySchema;

            //now we need to build up the attributes, get all attribute aliases and go from there
            foreach (var f in source.Fields.Where(x =>
                x.Key.StartsWith(FixedAttributeIndexFields.AttributePrefix)
                && x.Key.EndsWith(FixedAttributeIndexFields.AttributeAlias)))
            {
                //get the alias for the attribute
                var alias = f.Value;

                //now we can use this alias to find the rest of the attributes values
                //var nameKey = FixedAttributeIndexFields.AttributePrefix + alias + "." + FixedAttributeIndexFields.AttributeName;
                var valueKey = FixedAttributeIndexFields.AttributePrefix + alias;
                var idKey = FixedAttributeIndexFields.AttributePrefix + alias + "." + FixedAttributeIndexFields.AttributeId;

                //find the associated definition in the schema and set it
                var def = target.EntitySchema.AttributeDefinitions.SingleOrDefault(x => x.Alias == alias);

                // Check if the definition is "inherited" because it exists on a parent schema
                if (def == null)
                {
                    if (compositeSchema != null)
                    {
                        def = compositeSchema.AllAttributeDefinitions.SingleOrDefault(x => x.Alias == alias);
                    }
                }

                if (def != null)
                {
                    //get all values for the current value (as some attributes can store multiple named values, not just one)
                    var values = source.Fields
                        .Where(k => k.Key.StartsWith(valueKey)
                            //&& !k.Key.EndsWith(FixedAttributeIndexFields.AttributeName)
                            && !k.Key.EndsWith(FixedAttributeIndexFields.AttributeAlias)
                            && !k.Key.EndsWith(FixedAttributeIndexFields.AttributeId));

                    var attribute = new TypedAttribute(def)
                        {
                            Id = HiveId.Parse(source.Fields[idKey])
                        };

                    foreach (var v in values)
                    {
                        //get the value name, it could be blank if this attribute is only storing one value
                        var valueName = v.Key.Substring(valueKey.Length, v.Key.Length - valueKey.Length);
                        if (valueName.IsNullOrWhiteSpace())
                        {
                            //if its a null value name, then set the dynamic value
                            attribute.DynamicValue = GetRealValueFromField(def.AttributeType.SerializationType, v.Value);
                        }
                        else
                        {
                            //if its a named value, then set it by name
                            attribute.Values.Add(valueName.TrimStart('.'), GetRealValueFromField(def.AttributeType.SerializationType, v.Value));
                        }


                    }

                    target.Attributes.SetValueOrAdd(attribute);
                }
            }
        }

        private dynamic GetRealValueFromField(IAttributeSerializationDefinition serializationDefinition, string savedVal)
        {
            switch (serializationDefinition.DataSerializationType)
            {
                case DataSerializationTypes.Guid:
                case DataSerializationTypes.String:
                case DataSerializationTypes.LongString:
                case DataSerializationTypes.Boolean:
                    return savedVal;
                case DataSerializationTypes.SmallInt:
                case DataSerializationTypes.LargeInt:
                    int intOut;
                    if (int.TryParse(savedVal, out intOut))
                    {
                        return intOut;
                    }
                    throw new InvalidCastException("Could not parse value " + savedVal + " into an int Type");
                case DataSerializationTypes.Decimal:
                    decimal decOut;
                    if (decimal.TryParse(savedVal, out decOut))
                    {
                        return decOut;
                    }
                    throw new InvalidCastException("Could not parse value " + savedVal + " into an decimal Type");
                case DataSerializationTypes.Date:
                    var dateTime = ExamineHelper.FromExamineDateTime(savedVal);
                    if (dateTime != null)
                    {
                        return dateTime;
                    }
                    throw new InvalidCastException("Could not parse value " + savedVal + " into an DateTimeOffset Type");
                case DataSerializationTypes.ByteArray:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}