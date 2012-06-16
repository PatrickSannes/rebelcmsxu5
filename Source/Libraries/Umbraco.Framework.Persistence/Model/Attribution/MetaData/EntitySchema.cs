using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    public class EntitySchema : AbstractSchemaPart<EntitySchema>, IReferenceByName, IRelatableEntity, IVersionableEntity
    {
        private readonly EntityCollection<AttributeGroup> _attributeGroups;
        private readonly EntityCollection<AttributeDefinition> _attributeDefinitions;
        private readonly ReaderWriterLockSlim _groupLocker = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim _attribLocker = new ReaderWriterLockSlim();

        public EntitySchema()
        {
            _attributeDefinitions = new EntityCollection<AttributeDefinition>();
            _attributeDefinitions.CollectionChanged += AttributeDefinitionsCollectionChanged;
            _attributeGroups = new EntityCollection<AttributeGroup>()
                {
                    OnRemove = EnsureAttributeDefsRemoved
                };
            _attributeGroups.CollectionChanged += AttributeGroupsCollectionChanged;
            EnsureXmlConfigSetup();

            //v2: create the relation proxy collection
            RelationProxies = new RelationProxyCollection(this);

            SchemaType = FixedSchemaTypes.GenericUnknown;
        }

        public EntitySchema(string @alias, LocalizedString name) 
            : this()
        {
            Mandate.ParameterNotNull(@alias, "@alias");
            Mandate.ParameterNotNull(name, "name");
            
            this.Setup(alias, name);
        }

        public EntitySchema(string @alias)
            : this()
        {
            Alias = alias;
        }

        private string _schemaType;
        private XDocument _xmlConfiguration;
        private readonly static PropertyInfo AttributeDefinitionsSelector = ExpressionHelper.GetPropertyInfo<EntitySchema, EntityCollection<AttributeDefinition>>(x => x.AttributeDefinitions);
        private readonly static PropertyInfo AttributeGroupsSelector = ExpressionHelper.GetPropertyInfo<EntitySchema, EntityCollection<AttributeGroup>>(x => x.AttributeGroups);
        private readonly static PropertyInfo SchemaTypeSelector = ExpressionHelper.GetPropertyInfo<EntitySchema, string>(x => x.SchemaType);
        private readonly static PropertyInfo XmlConfigurationSelector = ExpressionHelper.GetPropertyInfo<EntitySchema, XDocument>(x => x.XmlConfiguration);


        void AttributeDefinitionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _groupPopulationValid = false;
            OnPropertyChanged(AttributeDefinitionsSelector);
        }
        void AttributeGroupsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _groupPopulationValid = false;
            OnPropertyChanged(AttributeGroupsSelector);
        }

        /// <summary>
        /// Gets or sets the type of the schema, for example "content", "media", "user" etc.
        /// </summary>
        /// <value>The type of the schema.</value>
        /// <remarks></remarks>
        public string SchemaType
        {
            get { return _schemaType; }
            set
            {
                _schemaType = value;
                OnPropertyChanged(SchemaTypeSelector);
            }
        }

        ///// <summary>
        /////   Gets or the attribute types for this schema.
        ///// </summary>
        ///// <value>The attribute type definitions.</value>
        //public EntityCollection<AttributeType> AttributeTypes
        //{
        //    get
        //    {
        //        return new EntityCollection<AttributeType>(AttributeDefinitions.Select(x => x.AttributeType)
        //                                                       .Distinct(new AttributeTypeDefinitionComparer()));
        //    }
        //}

        public IEnumerable<AttributeType> AttributeTypes
        {
            get
            {
                return AttributeDefinitions.Select(x => x.AttributeType).Distinct();
                    //.Distinct(new AttributeTypeDefinitionComparer());
            }
        }


        /// <summary>
        ///   Gets the attribute definitions for this schema.
        /// </summary>
        /// <value>The attribute type definitions.</value>
        public EntityCollection<AttributeDefinition> AttributeDefinitions
        {
            get { return _attributeDefinitions; }
        }

        /// <summary>
        /// Gets the attribute group definitions.
        /// </summary>
        /// <value>The attribute group definitions.</value>
        public EntityCollection<AttributeGroup> AttributeGroups
        {
            get
            {
                // Get the groups from the AttributeDefinitions in order to union them with the internal collection
                // of stuff that has been added manually
                EnsureGroupsPopulated();
                return _attributeGroups;
            }
        }

        private void EnsureAttributeDefsRemoved(AttributeGroup item)
        {
            using (new WriteLockDisposable(_attribLocker))
            {
                // Delete items that belonged to this group
                var forRemoval = _attributeDefinitions.Where(x => x.AttributeGroup.Alias == item.Alias).ToArray();
                foreach (var attributeDefinition in forRemoval)
                {
                    _attributeDefinitions.Remove(attributeDefinition);
                }
            }
        }

        private bool _groupPopulationValid = false;
        /// <summary>
        /// Ensures the groups specified in this instance's <see cref="AttributeDefinitions"/> are contained in the local collection
        /// of groups. 
        /// </summary>
        /// <remarks></remarks>
        private void EnsureGroupsPopulated()
        {
            using (new WriteLockDisposable(_groupLocker))
            {
                if (_groupPopulationValid) return;

                // Get the groups from the attribute definitions. For those that have an ID (i.e. exist), ensure
                // our copy is distinct.
                var groupsFromDefs = GetGroupsFromDefsDistinctByAlias(); //<-- changed to distinct by alias not Id

                var groupsWithId = GetGroupsWithId(groupsFromDefs); 
                var groupsWithoutId = GetGroupsWithoutId(groupsFromDefs);
                var union = UnionGroups(groupsWithoutId, groupsWithId);

                // Get the groups from the above selection that we don't already have
                var compoundAdd = GroupsNotPresent(union);

                // Add them to our local collection where they don't already exist in our local collection with a valid id
                compoundAdd.Where(x => !_attributeGroups.Any(y => !y.Id.IsNullValueOrEmpty() && y.Id == x.Id))
                    .ForEach(x => _attributeGroups.Add(x));

                // After adding, check if we have any with matching aliases and remove those with a null ID
                _attributeGroups.RemoveAll(nullGroup => nullGroup.Id.IsNullValueOrEmpty() && _attributeGroups.Any(y => y.Alias == nullGroup.Alias && !y.Id.IsNullValueOrEmpty()));
                
                // Ensure we have distinct groups based on alias
                _attributeGroups.RemoveAll(existing => !_attributeGroups.DistinctBy(x => x.Alias).Contains(existing));

                _groupPopulationValid = true;
            }
        }

        private IEnumerable<AttributeGroup> GroupsNotPresent(IEnumerable<AttributeGroup> union)
        {
            var incoming = new HashSet<AttributeGroup>(union);
            incoming.ExceptWith(_attributeGroups);
            return incoming;
            //var compoundAdd = union.Except(_attributeGroups).ToList();
            //return compoundAdd;
        }

        private static IEnumerable<AttributeGroup> UnionGroups(IEnumerable<AttributeGroup> groupsWithoutId, IEnumerable<AttributeGroup> groupsWithId)
        {
            var union = groupsWithId.Concat(groupsWithoutId).ToList();
            return union;
        }

        private static IEnumerable<AttributeGroup> GetGroupsWithoutId(IEnumerable<AttributeGroup> groupsFromDefs)
        {
            var groupsWithoutId = groupsFromDefs.Where(x => x.Id.IsNullValueOrEmpty()).Distinct(DeferredEqualityComparer<AttributeGroup>.CompareMember(x => x.Alias)).ToList();
            return groupsWithoutId;
        }

        private static IEnumerable<AttributeGroup> GetGroupsWithId(IEnumerable<AttributeGroup> groupsFromDefs)
        {
            var groupsWithId = groupsFromDefs.Where(x => !x.Id.IsNullValueOrEmpty()).Distinct(DeferredEqualityComparer<AttributeGroup>.CompareMember(x => x.Alias)).ToList();
            return groupsWithId;
        }

        private IEnumerable<AttributeGroup> GetGroupsFromDefsDistinctByAlias()
        {
            var groupsFromDefs = AttributeDefinitions.Select(x => x.AttributeGroup).Distinct().WhereNotNull().ToList();
            return groupsFromDefs;
        }

        public XDocument XmlConfiguration
        {
            get { return _xmlConfiguration; }
            set
            {
                _xmlConfiguration = value;
                OnPropertyChanged(XmlConfigurationSelector);
            }
        }

        /// <summary>
        /// Expose from the XML config the abstract setting for the document type 
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                var elem = GetXmlConfigElement("is-abstract");
                return elem != null && bool.Parse(elem.Value);
            }
        }

        /// <summary>
        /// Returns a list of string values that have been put into the XML config as a list
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<string> GetXmlPropertyAsList(string key)
        {
            Mandate.ParameterNotNull(key, "key");
            var child = GetXmlConfigElement(key);
            if (child == null)
                return null;
            if (!child.Elements("value").Any())
                return null;
            return child.Elements("value")
                .Select(e => e.Value)
                .ToList();
        }

        /// <summary>
        /// Returns a list of T values that have been put into the XML config as a list
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<T> GetXmlPropertyAsList<T>(string key)
        {
            Mandate.ParameterNotNull(key, "key");            
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                throw new NotSupportedException("The type " + typeof(T) + " does not have an associated TypeConverter");
            var vals = GetXmlPropertyAsList(key);
            if (vals == null) 
                return Enumerable.Empty<T>();
            return vals
                .Select(val => (T) converter.ConvertFromInvariantString(val))
                .ToList();
        }

        /// <summary>
        /// Returns a  value from the xml config property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetXmlConfigProperty<T>(string key)
        {
            Mandate.ParameterNotNull(key, "key");
            var val = GetXmlConfigProperty(key);
            if (val == null) 
                return default(T);
            var converter = TypeDescriptor.GetConverter(typeof (T));
            if (converter != null)
            {
                return (T)converter.ConvertFromInvariantString(val);
            }
            throw new NotSupportedException("The type " + typeof (T) + " does not have an associated TypeConverter");
        }

        /// <summary>
        /// Returns a string value from the XML property
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetXmlConfigProperty(string key)
        {
            Mandate.ParameterNotNull(key, "key");
            var child = GetXmlConfigElement(key);
            return child == null ? null : child.Value;
        }

        private XElement GetXmlConfigElement(string key)
        {
            Mandate.ParameterNotNull(key, "key");
            EnsureXmlConfigSetup();
            return XmlConfiguration.Root.Element(key);
        }

        /// <summary>
        /// Stores the list of T in an XML property
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void SetXmlConfigProperty<T>(string key, params T[] values)
        {
            SetXmlConfigProperty(key, values
                .Select(x => x.ToString())                
                .ToArray());
        }

        /// <summary>
        /// Stores the list of values in an XML property
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void SetXmlConfigProperty(string key, params string[] values)
        {
            Mandate.ParameterNotNull(key, "key");            
            EnsureXmlConfigSetup();            
            var element = GetXmlConfigElement(key);
            if (element == null)
            {
                element = new XElement(key);
                XmlConfiguration.Root.Add(element);
            }
            element.ReplaceAll(values
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => new XElement("value", new XCData(x))));

            OnPropertyChanged(XmlConfigurationSelector);
        }

        /// <summary>
        /// Stores as string value in the XML property
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetXmlConfigProperty(string key, string value)
        {
            Mandate.ParameterNotNull(key, "key");
            //ensure that the value is not null, if so chnage to empty string
            var valueAsString = value ?? string.Empty;
            EnsureXmlConfigSetup();
            var element = GetXmlConfigElement(key);
            if (element != null)
                element.ReplaceAll(new XCData(valueAsString)); 
            else
                XmlConfiguration.Root.Add(new XElement(key, new XCData(valueAsString)));

            OnPropertyChanged(XmlConfigurationSelector);
        }

        protected internal void EnsureXmlConfigSetup()
        {
            if (XmlConfiguration == null || XmlConfiguration.Root == null) XmlConfiguration = new XDocument(new XElement("configuration"));
        }


        /// <summary>
        /// A store of relation proxies for this entity, to support enlisting relations to this entity.
        /// The relations will not be persisted until the entity is passed to a repository for saving.
        /// If <see cref="RelationProxyCollection.IsConnected"/> is <code>true</code>, this sequence may have
        /// <see cref="RelationProxy"/> objects lazily loaded by enumerating the results of calling <see cref="RelationProxyCollection.LazyLoadDelegate"/>.
        /// </summary>
        /// <value>The relation proxies.</value>
        public RelationProxyCollection RelationProxies { get; private set; }
    }
}