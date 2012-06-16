using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;

namespace Umbraco.CMS.DataPersistence.Repositories.PackageXml
{
    public class DataTypeRepositoryReader :
        IRepositoryReader<IEntityCollection<IAttributeTypeDefinition>, IEntityCollection<IAttributeTypeDefinition>, IAttributeTypeDefinition, IAttributeTypeDefinition>
    {
        private XDocument xml;

        public DataTypeRepositoryReader(XDocument xml)
        {
            this.xml = xml;
        }
        #region IRepositoryReader<IEntityCollection<IAttributeTypeDefinition>,IEntityCollection<IAttributeTypeDefinition>,IAttributeTypeDefinition,IAttributeTypeDefinition> Members

        public IDbContext DbContext
        {
            get { throw new NotImplementedException(); }
        }

        public IAttributeTypeDefinition Get(IMappedIdentifier identifier)
        {
            //this class is just a dummy class so far, we need to work out how we're going to create the "DataType" objects from a data source
            return new AttributeTypeDefinition
            {
                Alias = "test",
                Name = "Test",
                SerializationType = new StringSerializationType() { Alias = "string" },
                Id = identifier
            };
        }

        public IEntityCollection<IAttributeTypeDefinition> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeTypeDefinition> GetAll()
        {
            throw new NotImplementedException();
        }

        public IAttributeTypeDefinition Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeTypeDefinition> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeTypeDefinition> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IAttributeTypeDefinition GetParent(IAttributeTypeDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IAttributeTypeDefinition GetParent(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeTypeDefinition> GetDescendents(IAttributeTypeDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeTypeDefinition> GetDescendents(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IMappedIdentifier ResolveIdentifier<TIdentifierValue>(TIdentifierValue value)
        {
            Guid g = Guid.Parse(value.ToString());
            return new GuidIdentifier()
            {
                Value = g
            };
        }

        #endregion

        #region ISupportsProviderInjection Members

        public IProviderManifest Provider { get; set; }

        #endregion
    }
}
