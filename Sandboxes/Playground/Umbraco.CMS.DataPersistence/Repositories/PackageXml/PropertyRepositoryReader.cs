using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Umbraco.CMS.DataPersistence.Repositories.PackageXml
{
    /// <summary>
    /// This class is responsible for resolving AttributeDefinitions (DocTypeProperties) from the Umbraco package XML
    /// </summary>
    public class PropertyRepositoryReader
        : IRepositoryReader<IEntityCollection<IAttributeDefinition>, IEntityCollection<IAttributeDefinition>, IAttributeDefinition, IAttributeDefinition>
    {
        private XDocument xml;
        private DataTypeRepositoryReader dataTypeRepo;
        //cache each object as they are created
        private readonly LazyMappedIdentifierCollection<IAttributeDefinition> cache;
        private readonly object cacheLock = new object();

        public PropertyRepositoryReader(XDocument xml, DataTypeRepositoryReader dataTypeRepo)
        {
            this.xml = xml;
            this.dataTypeRepo = dataTypeRepo;
            this.cache = new LazyMappedIdentifierCollection<IAttributeDefinition>();
        }
        #region IRepositoryReader<IEntityCollection<IAttributeDefinition>,IEntityCollection<IAttributeDefinition>,IAttributeDefinition,IAttributeDefinition> Members

        public IDbContext DbContext
        {
            get { throw new NotImplementedException(); }
        }

        public IAttributeDefinition Get(IMappedIdentifier identifier)
        {
            if (!cache.ContainsKey(identifier))
            {
                lock (cacheLock)
                {
                    if (!cache.ContainsKey(identifier))
                    {
                        cache.Add(identifier, new Lazy<IAttributeDefinition>(() => {
                            var propertyPath = identifier.Value;

                            //the ID is actually a path, ie: DocTypeAlias/PropertyAlias so we can resolve it that way
                            var propertyAsXml = xml
                                .Descendants("DocumentType").First(x => (string)x.Element("Info").Element("Alias") == propertyPath.Split('/')[0])
                                .Descendants("GenericProperty").First(x => (string)x.Element("Alias") == propertyPath.Split('/')[1]);

                            //find the data type
                            var dataTypeId = dataTypeRepo.ResolveIdentifier((string)propertyAsXml.Element("Type"));
                            var dataType = dataTypeRepo.Get(dataTypeId);

                            //create an AttributeDefinition
                            //TODO: How do we do the "UI" data?
                            var attrDef = new AttributeDefinition();
                            attrDef.Id = identifier;
                            attrDef.AttributeType = dataType;
                            attrDef.Alias = (string)propertyAsXml.Element("Alias");
                            attrDef.Name = (string)propertyAsXml.Element("Name");

                            return attrDef;
                        }));
                    }
                }
            }

            return cache[identifier].Value;
        }

        public IEntityCollection<IAttributeDefinition> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeDefinition> GetAll()
        {
            throw new NotImplementedException();
        }

        public IAttributeDefinition Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeDefinition> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeDefinition> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IAttributeDefinition GetParent(IAttributeDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IAttributeDefinition GetParent(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeDefinition> GetDescendents(IAttributeDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeDefinition> GetDescendents(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IMappedIdentifier ResolveIdentifier<TIdentifierValue>(TIdentifierValue value)
        {
            //TODO: Validation that the 'value' is a string in the format of docTypeAlias/propertyAlias
            return new StringIdentifier(value, "Property");
        }

        #endregion

        #region ISupportsProviderInjection Members

        public IProviderManifest Provider { get; set; }

        #endregion
    }
}
