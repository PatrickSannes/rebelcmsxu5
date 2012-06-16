using System.Configuration;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

namespace Umbraco.Hive.Configuration
{
    public class SchemaTypeLoaderElement : PersistenceTypeLoaderElement
    {
        [ConfigurationProperty("revisioning")]
        public RevisionTypeLoaderElement<EntitySchema> Revisioning
        {
            get { return (RevisionTypeLoaderElement<EntitySchema>)base["revisioning"]; }
            set { base["revisioning"] = value; }
        }
    }
}