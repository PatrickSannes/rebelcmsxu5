using System;
using Examine;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    /// <summary>
    /// Converts a TypedEntity to a NestedHiveIndexOperation
    /// </summary>
    internal class TypedEntityToIndexOperation : TypeMapper<TypedEntity, NestedHiveIndexOperation>
    {
        

        public TypedEntityToIndexOperation(AbstractFluentMappingEngine engine)
            : base(engine)
        {
            
            MappingContext
                .CreateUsing(x => new NestedHiveIndexOperation())
                .ForMember(x => x.Entity, opt => opt.MapFrom(y => y))
                .ForMember(x => x.OperationType, opt => opt.MapFrom(y => IndexOperationType.Add))
                .ForMember(x => x.Id, opt => opt.MapFrom(y => new Lazy<string>(() => y.Id.Value.ToString())))
                .ForMember(x => x.ItemCategory, opt => opt.MapFrom(y => typeof(TypedEntity).Name))
                .ForMember(x => x.Fields, opt => opt.MapUsing<TypedEntityToIndexFields>())
                .AfterMap((s, t) =>
                    {
                        //create sub operations for each of the children
                        //NOTE: we don't store TypedAttribute seperately in the index, they get stored with the TypedEntity!

                        //NOTE: we need to store the entity id in a seperate field to the normal id when using revisions
                        t.AddOrUpdateField(FixedIndexedFields.EntityId, new Lazy<string>(() => s.Id.Value.ToString()));

                        s.MapRelations(t, MappingContext.Engine);

                        t.SubIndexOperations.Add(MappingContext.Engine.Map<EntitySchema, NestedHiveIndexOperation>(s.EntitySchema));

                        ExamineHelper.SetTimestampsOnEntityAndIndexFields(t.Fields, s);

                        //ensure the IsLatest flag is set, this is only used if we're supporting Revisions. If we change
                        //to a NullRevisionFactory then only one TypedEntity will ever exist.
                        t.Fields.Add(FixedRevisionIndexFields.IsLatest, new ItemField(1) { DataType = FieldDataType.Int });

                    });
        }
    }
}