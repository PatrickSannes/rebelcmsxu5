using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Framework;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.Brokers;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;
using Umbraco.CMS.Domain;

namespace Umbraco.CMS.DataPersistence.Repositories.PackageXml
{
    public class DocTypeEntityRepositoryReader
        :
            IRepositoryReader
                <IEntityCollection<IEntityTypeDefinition>, IEntityGraph<IEntityTypeDefinition>, IEntityTypeDefinition,
                IEntityTypeDefinition>
    {
        private XDocument PackageXml { get; set; }
        private IRepositoryReader<IEntityCollection<IAttributeGroupDefinition>, IEntityCollection<IAttributeGroupDefinition>, IAttributeGroupDefinition, IAttributeGroupDefinition> attributeGroupRepository;
        private IRepositoryReader<IEntityCollection<IAttributeDefinition>, IEntityCollection<IAttributeDefinition>, IAttributeDefinition, IAttributeDefinition> attributeRepository;

        public DocTypeEntityRepositoryReader(
            XDocument xDocument, 
            IDbContext ctx, 
            IProviderManifest provider,
            IRepositoryReader<IEntityCollection<IAttributeGroupDefinition>, IEntityCollection<IAttributeGroupDefinition>, IAttributeGroupDefinition, IAttributeGroupDefinition> groupRepo,
            IRepositoryReader<IEntityCollection<IAttributeDefinition>, IEntityCollection<IAttributeDefinition>, IAttributeDefinition, IAttributeDefinition> attrRepo
            )
        {
            PackageXml = xDocument;
            DbContext = ctx;
            Provider = provider;
            attributeGroupRepository = groupRepo;
            attributeRepository = attrRepo;
        }

        //public DocTypeEntityRepositoryReader(string xmlPackagePath)
        //    :this(xmlPackagePath, new XmlDbContext(), new StandardProviderManifest())
        //{
        //}

        //public DocTypeEntityRepositoryReader(string xmlPackagePath, IDbContext ctx, IProviderManifest provider)
        //    : this(XDocument.Load(xmlPackagePath), ctx, provider)
        //{
        //}

        #region Implementation of ISupportsProviderInjection

        public IProviderManifest Provider { get; set; }

        #endregion

        #region Implementation of IRepositoryReader<out IEntityCollection<IEntityTypeDefinition>,out IEntityGraph<IEntityTypeDefinition>, IEntityTypeDefinition, out IEntityTypeDefinition>

        public IDbContext DbContext { get; private set; }
        public IEntityTypeDefinition Get(IMappedIdentifier identifier)
        {
            return Get(identifier, 0);
        }

        public IEntityCollection<IEntityTypeDefinition> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            var returntype = new EntityCollection<IEntityTypeDefinition>();

            foreach (var identifier in identifiers)
                returntype.Add(Get(identifier));

            return returntype;
        }

        public IEntityGraph<IEntityTypeDefinition> GetAll()
        {
            var elements = PackageXml.Element("umbPackage").Element("DocumentTypes").Elements("DocumentType");

            var returnVal = new EntityGraph<IEntityTypeDefinition>();
            foreach (var xElement in elements)
            {
                var xml = xElement;
                var id = new StringIdentifier((string)xml.Element("Info").Element("Alias"), this.GetType().FullName);
                returnVal.Add(id, new Lazy<IEntityTypeDefinition>(() =>
                {
                    var info = xml.Element("Info");
                    var entityTypeDefinition = new DocType();
                    var root = new TypedEntityVertex().SetupRoot();
                    entityTypeDefinition.SetupTypeDefinition((string)info.Element("Alias"), (string)info.Element("Name"), root);
                    entityTypeDefinition.Id = id;

                    //find all the tabs so we can get the ID's off them
                    var tabs = xml.Element("Tabs").Elements("Tab");
                    foreach (var tab in tabs)
                    {
                        //get the repo to create the ID
                        var groupId = attributeGroupRepository.ResolveIdentifier((string)tab.Element("Caption"));
                        //resolve the group from the repo based on the ID schema
                        var group = attributeGroupRepository.Get(groupId);

                        //add the group to the attribute schema
                        entityTypeDefinition.AttributeSchema.AttributeGroupDefinitions.Add(group);
                    }

                    var properties = xml.Element("GenericProperties").Elements("GenericProperty");
                    foreach (var property in properties)
                    {
                        //TODO: Make this lazy, currently we're resolving all the attributes (and all its sub-graph) immediately...

                        //get the ID of the property using the repo to generate it
                        var attributeId = attributeRepository.ResolveIdentifier( entityTypeDefinition.Alias + "/" + (string)property.Element("Alias"));
                        //resolve the attribute
                        var attribute = attributeRepository.Get(attributeId);
                        //find the group that this attribute is associated with
                        var groupId = attributeGroupRepository.ResolveIdentifier((string)property.Element("Tab"));
                        var group = attributeGroupRepository.Get(groupId);
                        //add this attribute to the group

                        group.AttributeDefinitions.Add(attribute);
                    }

                    return entityTypeDefinition;
                }));
                
            }

            return returnVal;
        }

        public IEntityTypeDefinition Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityGraph<IEntityTypeDefinition> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            var elements = PackageXml.XPathSelectElements("DocumentType");

            var returnVal = new EntityGraph<IEntityTypeDefinition>();

            foreach (var xElement in elements)
            {
                EntityTypeDefinition entityTypeDefinition = new EntityTypeDefinition();
                entityTypeDefinition.Id = new RepositoryGeneratedIdentifier();
                entityTypeDefinition.Name = xElement.Element("Name").Value;
                entityTypeDefinition.Alias = xElement.Element("Alias").Value;
                entityTypeDefinition.GraphSchema = new EntityGraphSchema();
                
            }

            return returnVal;
        }

        public IEntityGraph<IEntityTypeDefinition> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityTypeDefinition GetParent(IEntityTypeDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IEntityTypeDefinition GetParent(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IEntityTypeDefinition> GetDescendents(IEntityTypeDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IEntityTypeDefinition> GetDescendents(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IMappedIdentifier ResolveIdentifier<TIdentifierValue>(TIdentifierValue value)
        {
            return new StringIdentifier(value, this.GetType().FullName);
        }

        #endregion
    }
}
