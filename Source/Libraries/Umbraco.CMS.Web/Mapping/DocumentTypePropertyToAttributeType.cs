using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mapping
{
    internal class DocumentTypePropertyToAttributeType : StandardMemberMapper<DocumentTypeProperty, AttributeType>
    {
        public DocumentTypePropertyToAttributeType(AbstractFluentMappingEngine currentEngine, MapResolverContext context)
            : base(currentEngine, context)
        { }

        public override AttributeType GetValue(DocumentTypeProperty source)
        {
            // Commented this out, as there was a problem with the AttributeType being updated with the merged config,
            // then the content was saved forcing the AttributeType to resave, overwriting the AttributeTypes default config.
            // As far as I can tell, I can't see a situation where converting a DocTypeProperty to an AttributeType would require
            // it to be mapped, so instead I now just load straight from DB
            // MB

            if (!source.DataTypeId.IsNullValueOrEmpty())
            {
                using (var uow = ResolverContext.Hive.OpenReader<IContentStore>())
                {
                    return uow.Repositories.Schemas.Get<AttributeType>(source.DataTypeId);
                }
            }

            return CurrentEngine.Map<DataType, AttributeType>(source.DataType);
        }
    }
}
