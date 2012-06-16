using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;
using Umbraco.Framework.EntityGraph.Domain.Brokers;

namespace Umbraco.CMS.DataPersistence.Repositories.PackageXml
{
    public class TabRepositoryReader
        : IRepositoryReader<IEntityCollection<IAttributeGroupDefinition>, IEntityCollection<IAttributeGroupDefinition>, IAttributeGroupDefinition, IAttributeGroupDefinition>
    {
        private XDocument xml;
        private readonly LazyMappedIdentifierCollection<IAttributeGroupDefinition> cache;
        private readonly object cacheLock = new object();
        private readonly IMappedIdentifier genericGroupId;

        public TabRepositoryReader(XDocument xml)
        {
            this.xml = xml;
            cache = new LazyMappedIdentifierCollection<IAttributeGroupDefinition>();

            //The "Generic Properties" group should always exist, so we'll create it in the ctor
            genericGroupId = this.ResolveIdentifier("Generic Properties");
            cache.Add(genericGroupId, new Lazy<IAttributeGroupDefinition>(() =>
            {
                return new AttributeGroupDefinition
                {
                    Id = genericGroupId,
                    Name = "Generic Properties",
                    Alias = "Generic",
                };
            }));
        }
        #region IRepositoryReader<IEntityCollection<IAttributeGroupDefinition>,IEntityCollection<IAttributeGroupDefinition>,IAttributeGroupDefinition,IAttributeGroupDefinition> Members

        public IDbContext DbContext
        {
            get { throw new NotImplementedException(); }
        }

        public IAttributeGroupDefinition Get(IMappedIdentifier identifier)
        {
            if (!cache.ContainsKey(identifier))
            {
                lock (cacheLock)
                {
                    if (!cache.ContainsKey(identifier))
                    {
                        cache.Add(identifier, new Lazy<IAttributeGroupDefinition>(() =>
                        {
                            //look for the tab in the XML, this is kind of dodgy as we don't handle the tab being on multiple doc types, ie: it's not handling the tab hierarchy
                            var group = (from tabs in xml.Descendants("Tabs")
                                      from tab in tabs.Elements("Tab")
                                      where (string)tab.Element("Caption") == identifier.Value
                                      select tab).FirstOrDefault();

                            //if we didn't find the group we're going to use the generic one
                            if (group == default(XElement))
                                return Get(genericGroupId);

                            //parse the XML into a simple group object
                            var t = new AttributeGroupDefinition()
                            {
                                Id = identifier,
                                Alias = identifier.ValueAsString,
                                Name = group.Descendants("Caption").First().Value
                            };

                            return t;
                        }));
                    }
                }
            }

            return cache[identifier].Value;
        }

        public IEntityCollection<IAttributeGroupDefinition> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeGroupDefinition> GetAll()
        {
            throw new NotImplementedException();
        }

        public IAttributeGroupDefinition Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeGroupDefinition> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeGroupDefinition> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IAttributeGroupDefinition GetParent(IAttributeGroupDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IAttributeGroupDefinition GetParent(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeGroupDefinition> GetDescendents(IAttributeGroupDefinition forEntity)
        {
            throw new NotImplementedException();
        }

        public IEntityCollection<IAttributeGroupDefinition> GetDescendents(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IMappedIdentifier ResolveIdentifier<TIdentifierValue>(TIdentifierValue value)
        {
            return new StringIdentifier(value.ToString(), "Tab");
        }

        #endregion

        #region ISupportsProviderInjection Members

        public IProviderManifest Provider { get; set; }

        #endregion
    }
}
