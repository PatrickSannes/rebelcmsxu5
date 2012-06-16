using System.Configuration;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Hive.Configuration
{
    public class TypeLoaderElement : PersistenceTypeLoaderElement
    {
        [ConfigurationProperty("revisioning")]
        public RevisionTypeLoaderElement<TypedEntity> Revisioning
        {
            get { return (RevisionTypeLoaderElement<TypedEntity>)base["revisioning"]; }
            set { base["revisioning"] = value; }
        }

        [ConfigurationProperty("schema")]
        public SchemaTypeLoaderElement Schema
        {
            get { return (SchemaTypeLoaderElement)base["schema"]; }
            set { base["schema"] = value; }
        }
    }
}
