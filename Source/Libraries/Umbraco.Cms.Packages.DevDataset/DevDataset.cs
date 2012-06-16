using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.System;
using Umbraco.Framework.Persistence;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Packages.DevDataset
{
    /// <summary>
    /// USED ONLY FOR DEMO DATA!!!
    /// </summary>
    /// <remarks>
    /// Is most likely NOT thread safe
    /// </remarks>
    public class DevDataset
    {
        private readonly IFrameworkContext _frameworkContext;
        private readonly IAttributeTypeRegistry _attributeTypeRegistry;

        public DevDataset(IPropertyEditorFactory propertyEditorFactory, IFrameworkContext frameworkContext, IAttributeTypeRegistry attributeTypeRegistry)
        {
            _frameworkContext = frameworkContext;
            _attributeTypeRegistry = attributeTypeRegistry;
            PropertyEditorFactory = propertyEditorFactory;
            InitCreators();
            InitDataTypes();
            InitTemplates();
            InitDocTypes();
            _nodeData = XDocument.Parse(Files.umbraco);

        }

        private void InitCreators()
        {
            var creators = new Dictionary<int, string>
                               {
                                   {0, "Administrator"}
                               };
            _creators = creators;
        }


        /// <summary>
        /// creates a test repository of UmbracoNodes
        /// </summary>
        private List<ContentEditorModel> _testRepository;
        private List<DataType> _dataTypes;

        private readonly XDocument _nodeData;
        private IEnumerable<DocumentTypeEditorModel> _docTypes;
        private IEnumerable<TemplateFile> _templates;
        private Dictionary<int, string> _creators;
        private int _tabIdCounter = 990000;
        private AttributeGroup _generalGroup = FixedGroupDefinitions.GeneralGroup;
        //defined internal attribute definitions/data types
        private DataType _selectedTemplateDataType;
        private DataType _nodeNameDataType;


        /// <summary>
        /// TEMPORARY method to install all data required for dev data set excluding all of the core data
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="framework"></param>
        internal void InstallDevDataset(IHiveManager manager, IFrameworkContext framework)
        {

            //a list of all the schemas we've added
            var schemas = new List<EntitySchema>();

            using (var writer = manager.OpenWriter<IContentStore>())
            {
                //create all of the document type's and their associated tabs first
                //foreach (var d in _devDataSet.ContentData.Select(x => x.DocumentType).DistinctBy(x => x.Id))
                foreach (var d in DocTypes)
                {

                    var schema = new EntitySchema(d.Alias, d.Name);
                    schema.Id = d.Id;
                    schema.AttributeGroups.AddRange(
                        framework.TypeMappers.Map<IEnumerable<Tab>, IEnumerable<AttributeGroup>>(
                            d.DefinedTabs));
                    writer.Repositories.Schemas.AddOrUpdate(schema);
                    schemas.Add(schema);

                    foreach (var parentId in d.InheritFrom.Where(x => x.Selected).Select(x => HiveId.Parse(x.Value)))
                        writer.Repositories.AddRelation(new Relation(FixedRelationTypes.DefaultRelationType, parentId, d.Id));
                }
                writer.Complete();
            }

            using (var writer = manager.OpenWriter<IContentStore>())
            {
                //now we can hopefully just map the schema and re-save it so it maps all properties
                //foreach (var d in _devDataSet.ContentData.Select(x => x.DocumentType).DistinctBy(x => x.Id))
                foreach (var d in DocTypes)
                {
                    var schema = framework.TypeMappers.Map<DocumentTypeEditorModel, EntitySchema>(d);
                    writer.Repositories.Schemas.AddOrUpdate(schema);
                }
                writer.Complete();
            }

            using (var writer = manager.OpenWriter<IContentStore>())
            {
                //now just map the entire content entities and persist them, since the attribution definitions and attribution 
                //groups are created these should just map up to the entities in the database.

                var mappedCollection = framework
                    .TypeMappers.Map<IEnumerable<ContentEditorModel>, IEnumerable<Revision<TypedEntity>>>(ContentData)
                    .ToArray();
                mappedCollection.ForEach(x => x.MetaData.StatusType = FixedStatusTypes.Published);

                //var allAttribTypes = AllAttribTypes(mappedCollection);

                writer.Repositories.Revisions.AddOrUpdate(mappedCollection);

                writer.Complete();
            }

            ////now that the data is in there, we need to setup some structure... probably a nicer way to do this but whatevs... its just for testing
            //using (var writer = mappingGroup.CreateReadWriteUnitOfWork())
            //{
            //    var homeSchema = writer.ReadWriteRepository.GetEntity<EntitySchema>(HiveId.ConvertIntToGuid(1045));
            //    var contentSchema = writer.ReadWriteRepository.GetEntity<EntitySchema>(HiveId.ConvertIntToGuid(1045));
            //    var faqContainerSchema = writer.ReadWriteRepository.GetEntity<EntitySchema>(HiveId.ConvertIntToGuid(1055));
            //    var faqCatSchema = writer.ReadWriteRepository.GetEntity<EntitySchema>(HiveId.ConvertIntToGuid(1056));
            //    var faqSchema = writer.ReadWriteRepository.GetEntity<EntitySchema>(HiveId.ConvertIntToGuid(1057));

            //}
        }

        private static AttributeType[] AllAttribTypes(Revision<TypedEntity>[] mappedCollection)
        {
            return mappedCollection.Select(
                x => x.Item.Attributes.FirstOrDefault(
                    y =>
                    y.AttributeDefinition.AttributeType.Alias == NodeNameAttributeType.AliasValue))
                .WhereNotNull()
                .Select(x => x.AttributeDefinition.AttributeType)
                .ToArray();
        }


        /// <summary>
        /// Because we need to have model properties consistent across postbacks, and model property IDs are Guids which are not stored
        /// in the XML, we need to create a map so they remain consistent.
        /// </summary>
        /// <remarks>
        /// To create the map, we'll map the model ID + the model property name to a GUID
        /// </remarks>
        private static List<KeyValuePair<KeyValuePair<int, string>, Guid>> _nodePropertyIdMap;

        public IPropertyEditorFactory PropertyEditorFactory { get; private set; }

        public XDocument XmlData
        {
            get
            {
                return _nodeData;
            }
        }

        /// <summary>
        /// Returns a test repository for querying/updating
        /// </summary>
        public IEnumerable<ContentEditorModel> ContentData
        {
            get
            {
                if (_testRepository == null)
                {
                    InitRepository();
                }
                return _testRepository;
            }
        }

        /// <summary>
        /// Returns a set of data types to test against
        /// </summary>
        public IEnumerable<DataType> DataTypes
        {
            get
            {
                return _dataTypes;
            }
        }

        public IEnumerable<DocumentTypeEditorModel> DocTypes
        {
            get { return _docTypes; }
        }

        public IEnumerable<TemplateFile> Templates
        {
            get { return _templates; }
        }

        private HashSet<ContentProperty> GetNodeProperties(int id, HiveId selectedTemplateId)
        {

            var customProperties = new List<ContentProperty>();
            var tabIds = _docTypes.SelectMany(tabs => tabs.DefinedTabs).Select(x => x.Id).ToList();
            var currTab = 0;

            var node = XmlData.Root.Descendants()
                .Where(x => (string)x.Attribute("id") == id.ToString())
                .Single();

            var docTypeArray = _docTypes.ToArray();
            //get the corresponding doc type for this node
            var docType = docTypeArray
                .Where(x => x.Id == HiveId.ConvertIntToGuid(int.Parse((string)node.Attribute("nodeType"))))
                .Single();

            //add node name
            var nodeName = new ContentProperty((HiveId)Guid.NewGuid(),
                                               docType.Properties.Where(x => x.Alias == NodeNameAttributeDefinition.AliasValue).Single(),
                                               new Dictionary<string, object> { { "Name", (string)node.Attribute("nodeName") } })
                {
                    Name = NodeNameAttributeDefinition.AliasValue,
                    Alias = NodeNameAttributeDefinition.AliasValue,
                    TabId = docType.DefinedTabs.Where(x => x.Alias == _generalGroup.Alias).Single().Id
                    
                };
            customProperties.Add(nodeName);

            //add selected template (empty)
            var selectedTemplate = new ContentProperty((HiveId)Guid.NewGuid(),
                                                       docType.Properties.Where(x => x.Alias == SelectedTemplateAttributeDefinition.AliasValue).Single(),
                                                       selectedTemplateId.IsNullValueOrEmpty() ? new Dictionary<string, object>() : new Dictionary<string, object> { { "TemplateId", selectedTemplateId.ToString() } })
                {
                    Name = SelectedTemplateAttributeDefinition.AliasValue,
                    Alias = SelectedTemplateAttributeDefinition.AliasValue,
                    TabId = docType.DefinedTabs.Where(x => x.Alias == _generalGroup.Alias).Single().Id
                };
            customProperties.Add(selectedTemplate);

            customProperties.AddRange(
                node.Elements()
                    .Where(e => e.Attribute("isDoc") == null)
                    .Select(e =>
                    {

                        //Assigning the doc type properties is completely arbitrary here, all I'm doing is 
                        //aligning a DocumentTypeProperty that contains the DataType that I want to render

                        ContentProperty np;
                        DocumentTypeProperty dp;
                        switch (e.Name.LocalName)
                        {
                            case "bodyText":
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[0]);
                                dp = docType.Properties.Where(x => x.Alias == "bodyText").Single();
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, e.Value);
                                break;
                            case "colorSwatchPicker":
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[2]);
                                dp = docType.Properties.Where(x => x.Alias == "colorSwatchPicker").Single();
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, e.Value);
                                break;
                            case "tags":
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[3]);
                                dp = docType.Properties.Where(x => x.Alias == "tags").Single();
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, e.Value);
                                break;
                            case "textBox":
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[4]) { OverriddenPreValues = overridenPreVals };
                                dp = docType.Properties.Where(x => x.Alias == "textBox").Single();
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, e.Value);
                                break;
                            case "publisher":
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[4]) { OverriddenPreValues = overridenPreVals };
                                dp = docType.Properties.Where(x => x.Alias == "publisher").Single();
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, e.Value);
                                break;
                            case "numberOfPages":
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[4]) { OverriddenPreValues = overridenPreVals };
                                dp = docType.Properties.Where(x => x.Alias == "numberOfPages").Single();
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, e.Value);
                                break;
                            case "image":
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[4]) { OverriddenPreValues = overridenPreVals };
                                dp = docType.Properties.Where(x => x.Alias == "image").Single();
                                var values = e.Value.Split(',');
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, new Dictionary<string, object> { { "MediaId", values[0] }, { "Value", values[1] } });
                                break;
                            default:
                                //dp = new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]);
                                dp = docType.Properties.Where(x => x.Alias == e.Name.LocalName).Single();
                                np = new ContentProperty((HiveId)GetNodeProperty(e), dp, e.Value);
                                break;
                        }

                        //need to set the data type model for this property

                        np.Alias = e.Name.LocalName;
                        np.Name = e.Name.LocalName;

                        //add to a random tab
                        currTab = 0; // currTab == 2 ? 0 : ++currTab;
                        np.TabId = tabIds[currTab];

                        return np;
                    }).ToList());

            return new HashSet<ContentProperty>(customProperties);
        }

        private void InitRepository()
        {
            _testRepository = new List<ContentEditorModel>();

            var tabIds = _docTypes.SelectMany(tabs => tabs.DefinedTabs).Select(x => x.Id).ToList();

            var currTab = 0;

            XmlData.Root.Descendants()
                .Where(x => x.Attribute("isDoc") != null)
                .ToList()
                .ForEach(x =>
                {
                    var customProperties = new List<ContentProperty>();
                    //create the model
                    var parentId = x.Attribute("parentID");
                    var publishedScheduledDate = x.Attribute("publishedScheduledDate");
                    var unpublishedScheduledDate = x.Attribute("unPublishedScheduledDate");
                    var publishedDate = x.Attribute("publishedDate");

                    //BUG: We need to change this back once the Hive IO provider is fixed and it get its ProviderMetadata.MappingRoot  set
                    // (APN) - I've changed back
                    //var template = ((string)x.Attribute("template")).IsNullOrWhiteSpace() ? HiveId.Empty : new HiveId((Uri)null, "templates", new HiveIdValue((string)x.Attribute("template")));
                    var template = ((string)x.Attribute("template")).IsNullOrWhiteSpace() ? HiveId.Empty : new HiveId(new Uri("storage://templates"), "templates", new HiveIdValue((string)x.Attribute("template")));

                    var docType = string.IsNullOrEmpty((string)x.Attribute("nodeType"))
                                      ? null
                                      : DocTypes.Where(d => d.Id == HiveId.ConvertIntToGuid((int)x.Attribute("nodeType"))).SingleOrDefault();

                    var contentNode = new ContentEditorModel(HiveId.ConvertIntToGuid((int)x.Attribute("id")))
                        {                            
                            Name = (string)x.Attribute("nodeName"),
                            DocumentTypeId = docType == null ? HiveId.Empty : docType.Id,
                            DocumentTypeName = docType == null ? "" : docType.Name,
                            DocumentTypeAlias = docType == null ? "" : docType.Alias,                                                                                 
                            ParentId = parentId != null ? HiveId.ConvertIntToGuid((int)parentId) : HiveId.Empty,
                            //assign the properties
                            Properties = GetNodeProperties((int)x.Attribute("id"), template)
                        };

                    //add the new model
                    _testRepository.Add(contentNode);

                });
        }

        /// <summary>
        /// Creates the test data types
        /// </summary>
        private void InitDataTypes()
        {
            //get the data types from the CoreCmsData
            _dataTypes = new List<DataType>()
                    {
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("rte-pe".EncodeAsGuid())).Single()),
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("sltb-pe".EncodeAsGuid())).Single()),
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("csp-pe".EncodeAsGuid())).Single()),
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("tag-pe".EncodeAsGuid())).Single()),
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("mltb-pe".EncodeAsGuid())).Single()),
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("media-picker-pe".EncodeAsGuid())).Single()),
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("integer-pe".EncodeAsGuid())).Single()),
                        _frameworkContext.TypeMappers.Map<DataType>(CoreCmsData.RequiredCoreUserAttributeTypes().Where(x => x.Id == new HiveId("uploader-pe".EncodeAsGuid())).Single())
                    };

            //create the inbuilt data types

            var selectedTemplateAttributeType = _attributeTypeRegistry.GetAttributeType(SelectedTemplateAttributeType.AliasValue);
            _selectedTemplateDataType = new DataType(
                selectedTemplateAttributeType.Id, 
                selectedTemplateAttributeType.Name,
                selectedTemplateAttributeType.Alias,
                PropertyEditorFactory.GetPropertyEditor(new Guid(CorePluginConstants.SelectedTemplatePropertyEditorId)).Value,
                string.Empty);

            var nodeNameAttributeType = _attributeTypeRegistry.GetAttributeType(NodeNameAttributeType.AliasValue);
            _nodeNameDataType = new DataType(
                nodeNameAttributeType.Id, 
                nodeNameAttributeType.Name,
                nodeNameAttributeType.Alias,
                PropertyEditorFactory.GetPropertyEditor(new Guid(CorePluginConstants.NodeNamePropertyEditorId)).Value,
                string.Empty);

        }

        private void InitDocTypes()
        {
            
            Func<Tab[]> generateTabs = () => new[]
                {
                    new Tab {Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = "tab1", Name = "Some Content", SortOrder = 0},
                    new Tab {Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = "tab2", Name = "A second test tab", SortOrder = 1},
                    new Tab {Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = "tab3", Name = "Other info", SortOrder = 2},
                    //add the general tab
                    new Tab
                        {
                            Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = _generalGroup.Alias, Name = _generalGroup.Name, SortOrder = _generalGroup.Ordinal
                        }
                };

            var providerGroupRoot = new Uri("content://");
            var homePage = new DocumentTypeEditorModel(HiveId.ConvertIntToGuid(1045))
                {
                    Name = "Home Page",
                    Description = "Represents a home page node",
                    Icon = "doc.gif",
                    Thumbnail = "folder.png",
                    Alias = "homePage",
                    AllowedTemplates = new List<SelectListItem>(new[]
                        {
                            _templates.First()
                        }.Select(x => new SelectListItem {Selected = true, Text = x.Name, Value = x.Id.ToString()})),
                    DefaultTemplateId = _templates.First().Id,
                    DefinedTabs = new HashSet<Tab>(generateTabs.Invoke()),
                    InheritFrom = new[]{ new HierarchicalSelectListItem
                                {
                                    Selected = true,
                                    Value = FixedHiveIds.ContentRootSchema.ToString()
                                }
                    },
                    AllowedChildren = new[]
                        {
                            new SelectListItem
                                {
                                    Selected = true,
                                    Value = HiveId.ConvertIntToGuid(providerGroupRoot, "nhibernate", 1046).ToString()
                                },
                                
                            new SelectListItem
                                {
                                    Selected = true,
                                    Value = HiveId.ConvertIntToGuid(providerGroupRoot, "nhibernate", 2000).ToString()
                                }
                        }
                };
            homePage.Properties = new HashSet<DocumentTypeProperty>(new[]
                {
                    CreateNodeName(homePage),
                    CreateSelectedTemplateId(homePage),
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[0]) { Name = "Body Text", Alias = "bodyText", TabId = homePage.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Site Name", Alias = "siteName", TabId = homePage.DefinedTabs.ElementAt(1).Id, SortOrder = 1 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Site Description", Alias = "siteDescription", TabId = homePage.DefinedTabs.ElementAt(1).Id, SortOrder = 2 },
                });         

            var contentPage = new DocumentTypeEditorModel(HiveId.ConvertIntToGuid(1046))
                {
                    Name = "Content Page",
                    Description = "A page for content",
                    Icon = "doc4.gif",
                    Thumbnail = "doc.png",
                    Alias = "contentPage",
                    AllowedTemplates = new List<SelectListItem>(new[] { _templates.ElementAt(1) }.Select(x => new SelectListItem { Selected = true, Text = x.Name, Value = x.Id.ToString() })),
                    DefaultTemplateId = _templates.ElementAt(1).Id,
                    DefinedTabs = new HashSet<Tab>(generateTabs.Invoke()),
                    InheritFrom = new[]{ new HierarchicalSelectListItem
                                {
                                    Selected = true,
                                    Value = FixedHiveIds.ContentRootSchema.ToString()
                                }
                    },
                    AllowedChildren = new[]
                        {
                            new SelectListItem
                                {
                                    Selected = true,
                                    Value = HiveId.ConvertIntToGuid(providerGroupRoot, "nhibernate", 1046).ToString()
                                }
                        }
                };
            contentPage.Properties = new HashSet<DocumentTypeProperty>(new[]
                {
                    CreateNodeName(contentPage),
                    CreateSelectedTemplateId(contentPage),
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[0]) { Name = "Body Text", Alias = "bodyText", TabId = contentPage.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Umbraco Hide in Nav", Alias = "umbracoNaviHide", TabId = contentPage.DefinedTabs.ElementAt(1).Id, SortOrder = 4 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[2]) { Name = "Color Swatch Picker", Alias = "colorSwatchPicker", TabId = contentPage.DefinedTabs.ElementAt(0).Id, SortOrder = 5 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[3]) { Name = "Tags", Alias = "tags", TabId = contentPage.DefinedTabs.ElementAt(0).Id, SortOrder = 5 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[4]) { Name = "Textbox", Alias = "textBox", TabId = contentPage.DefinedTabs.ElementAt(0).Id, SortOrder = 5 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "New tree", Alias = "newTree", TabId = contentPage.DefinedTabs.ElementAt(0).Id, SortOrder = 5 }
                });

            var faq = new DocumentTypeEditorModel(HiveId.ConvertIntToGuid(1055))
                {
                    Name = "Faq",
                    Description = "A Faqs container",
                    Icon = "doc4.gif",
                    Thumbnail = "doc.png",
                    Alias = "Faq",
                    AllowedTemplates = new List<SelectListItem>(new[] {_templates.ElementAt(2)}.Select(x => new SelectListItem {Selected = true, Text = x.Name, Value = x.Id.ToString()})),
                    DefaultTemplateId = _templates.ElementAt(2).Id,
                    DefinedTabs = new HashSet<Tab>(generateTabs.Invoke()),
                    InheritFrom = new[]{ new HierarchicalSelectListItem
                                {
                                    Selected = true,
                                    Value = FixedHiveIds.ContentRootSchema.ToString()
                                }
                    },
                    AllowedChildren = new[]
                        {
                            new SelectListItem
                                {
                                    Selected = true,
                                    Value = HiveId.ConvertIntToGuid(providerGroupRoot, "nhibernate", 1056).ToString()
                                }
                        }
                };
            faq.Properties = new HashSet<DocumentTypeProperty>(new[]
                {
                    CreateNodeName(faq),
                    CreateSelectedTemplateId(faq),
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[0]) { Name = "Body Text", Alias = "bodyText", TabId = faq.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Umbraco Hide in Nav", Alias = "umbracoNaviHide", TabId = faq.DefinedTabs.ElementAt(1).Id, SortOrder = 4 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Show Form", Alias = "ShowForm", TabId = faq.DefinedTabs.ElementAt(0).Id, SortOrder = 5 },
                });

            var faqCategory = new DocumentTypeEditorModel(HiveId.ConvertIntToGuid(1056))
                {
                    Name = "Faq Category",
                    Description = "An Faq category",
                    Icon = "doc4.gif",
                    Thumbnail = "doc.png",
                    Alias = "FaqCategory",
                    //AllowedTemplates = new List<SelectListItem>(new[] { _templates.ElementAt(3) }.Select(x => new SelectListItem { Selected = true, Text = x.Name, Value = x.Id.ToString() })),
                    //DefaultTemplateId = _templates.ElementAt(3).Id,
                    DefinedTabs = new HashSet<Tab>(generateTabs.Invoke()),
                    InheritFrom = new[]{ new HierarchicalSelectListItem
                                {
                                    Selected = true,
                                    Value = FixedHiveIds.ContentRootSchema.ToString()
                                }
                    },
                    AllowedChildren = new[]
                        {
                            new SelectListItem
                                {
                                    Selected = true,
                                    Value = HiveId.ConvertIntToGuid(providerGroupRoot, "nhibernate", 1057).ToString()
                                }
                        }
                };
            faqCategory.Properties = new HashSet<DocumentTypeProperty>(new[]
                {
                    CreateNodeName(faqCategory),
                    CreateSelectedTemplateId(faqCategory),
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[0]) { Name = "Body Text", Alias = "bodyText", TabId = faqCategory.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Umbraco Hide in Nav", Alias = "umbracoNaviHide", TabId = faqCategory.DefinedTabs.ElementAt(1).Id, SortOrder = 4 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Show Form", Alias = "ShowForm", TabId = faqCategory.DefinedTabs.ElementAt(0).Id, SortOrder = 5 },
                });

            var faqQuestion = new DocumentTypeEditorModel(HiveId.ConvertIntToGuid(1057))
                {
                    Name = "Faq Question",
                    Description = "An Faq question",
                    Icon = "doc4.gif",
                    Thumbnail = "doc.png",
                    Alias = "FaqQuestion",
                    //AllowedTemplates = new List<SelectListItem>(new[] { _templates.ElementAt(4) }.Select(x => new SelectListItem { Selected = true, Text = x.Name, Value = x.Id.ToString() })),
                    //DefaultTemplateId = _templates.ElementAt(4).Id,
                    DefinedTabs = new HashSet<Tab>(generateTabs.Invoke()),
                    InheritFrom = new[]{ new HierarchicalSelectListItem
                                {
                                    Selected = true,
                                    Value = FixedHiveIds.ContentRootSchema.ToString()
                                }
                    },
                };
            faqQuestion.Properties = new HashSet<DocumentTypeProperty>(new[]
                {
                    CreateNodeName(faqQuestion),
                    CreateSelectedTemplateId(faqQuestion),
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[0]) { Name = "Body Text", Alias = "bodyText", TabId = faqQuestion.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Question Text", Alias = "questionText", TabId = faqQuestion.DefinedTabs.ElementAt(1).Id, SortOrder = 1 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Umbraco Hide in Nav", Alias = "umbracoNaviHide", TabId = faqQuestion.DefinedTabs.ElementAt(1).Id, SortOrder = 4 },
                });

            var booksPage = new DocumentTypeEditorModel(HiveId.ConvertIntToGuid(2000))
            {
                Name = "Books Page",
                Description = "List of usefull books",
                Icon = "doc4.gif",
                Thumbnail = "doc.png",
                Alias = "BooksPage",
                AllowedTemplates = new List<SelectListItem>(new[] { _templates.ElementAt(5) }.Select(x => new SelectListItem { Selected = true, Text = x.Name, Value = x.Id.ToString() })),
                DefaultTemplateId = _templates.ElementAt(5).Id,
                DefinedTabs = new HashSet<Tab>(new[]
                {
                    new Tab {Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = "content", Name = "Content", SortOrder = 0},
                    new Tab { Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = _generalGroup.Alias, Name = _generalGroup.Name, SortOrder = _generalGroup.Ordinal }
                }),
                InheritFrom = new[]{ new HierarchicalSelectListItem
                                {
                                    Selected = true,
                                    Value = FixedHiveIds.ContentRootSchema.ToString()
                                }
                    },
                AllowedChildren = new[]
                        {
                            new SelectListItem
                                {
                                    Selected = true,
                                    Value = HiveId.ConvertIntToGuid(providerGroupRoot, "nhibernate", 2001).ToString()
                                }
                        }
            };
            booksPage.Properties = new HashSet<DocumentTypeProperty>(new[]
                {
                    CreateNodeName(booksPage),
                    CreateSelectedTemplateId(booksPage)
                 });

            var bookPage = new DocumentTypeEditorModel(HiveId.ConvertIntToGuid(2001))
            {
                Name = "Book Page",
                Description = "An individual book",
                Icon = "doc4.gif",
                Thumbnail = "doc.png",
                Alias = "BookPage",
                AllowedTemplates = new List<SelectListItem>(new[] { _templates.ElementAt(6) }.Select(x => new SelectListItem { Selected = true, Text = x.Name, Value = x.Id.ToString() })),
                DefaultTemplateId = _templates.ElementAt(6).Id,
                DefinedTabs = new HashSet<Tab>(new[]
                {
                    new Tab { Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = "content", Name = "Content", SortOrder = 0},
                    new Tab { Id = HiveId.ConvertIntToGuid(++_tabIdCounter), Alias = _generalGroup.Alias, Name = _generalGroup.Name, SortOrder = _generalGroup.Ordinal }
                }),
                InheritFrom = new[]{ new HierarchicalSelectListItem
                                {
                                    Selected = true,
                                    Value = FixedHiveIds.ContentRootSchema.ToString()
                                }
                    },
            };
            bookPage.Properties = new HashSet<DocumentTypeProperty>(new[]
                {
                    CreateNodeName(bookPage),
                    CreateSelectedTemplateId(bookPage),
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[0]) { Name = "Description", Alias = "bodyText", TabId = bookPage.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[1]) { Name = "Publisher", Alias = "publisher", TabId = bookPage.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[6]) { Name = "Number of Pages", Alias = "numberOfPages", TabId = bookPage.DefinedTabs.ElementAt(0).Id, SortOrder = 0 },
                    new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _dataTypes[7]) { Name = "Image", Alias = "image", TabId = bookPage.DefinedTabs.ElementAt(0).Id, SortOrder = 0 }
                });

            _docTypes = new List<DocumentTypeEditorModel>
                            {
                                homePage,
                                contentPage,
                                faq,
                                faqCategory,
                                faqQuestion,
                                booksPage,
                                bookPage
                            };
           
        }

        private void InitTemplates()
        {
            var providerGroupRoot = new Uri("storage://templates");
            _templates = new List<TemplateFile>
                            {
                                new TemplateFile(new HiveId(providerGroupRoot, "templates", new HiveIdValue("Homepage.cshtml")))
                                    {                                        
                                        Name = "Home page"
                                    },
                                new TemplateFile(new HiveId(providerGroupRoot, "templates", new HiveIdValue("Textpage.cshtml")))
                                    {                                        
                                        Name = "Text page"
                                    },
                                new TemplateFile(new HiveId(providerGroupRoot, "templates", new HiveIdValue("Faq.cshtml")))
                                    {                                        
                                        Name = "Faq page"
                                    },
                                new TemplateFile(new HiveId(providerGroupRoot, "templates", new HiveIdValue("Faq-Category.cshtml")))
                                    {                                        
                                        Name = "Faq category"
                                    },
                                new TemplateFile(new HiveId(providerGroupRoot, "templates", new HiveIdValue("Faq-Question.cshtml")))
                                    {                                        
                                        Name = "Faq question"
                                    },
                                new TemplateFile(new HiveId(providerGroupRoot, "templates", new HiveIdValue("ProductsPage.cshtml")))
                                    {                                        
                                        Name = "Products Page"
                                    },
                                new TemplateFile(new HiveId(providerGroupRoot, "templates", new HiveIdValue("ProductPage.cshtml")))
                                    {                                        
                                        Name = "Product Page"
                                    }
                            };
        }

        private DocumentTypeProperty CreateNodeName(DocumentTypeEditorModel docType)
        {
            return new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _nodeNameDataType)
                       {
                           Name = "Node Name",
                           Alias = NodeNameAttributeDefinition.AliasValue,
                           TabId = docType.DefinedTabs.Where(x => x.Alias == _generalGroup.Alias).Single().Id,
                           SortOrder = 0
                       };
        }

        private DocumentTypeProperty CreateSelectedTemplateId(DocumentTypeEditorModel docType)
        {
            return new DocumentTypeProperty(new HiveId(Guid.NewGuid()), _selectedTemplateDataType)
            {
                Name = "Selected template",
                Alias = SelectedTemplateAttributeDefinition.AliasValue,
                TabId = docType.DefinedTabs.Where(x => x.Alias == _generalGroup.Alias).Single().Id,
                SortOrder = 0
            };
        }

        private Guid GetNodeProperty(XElement node)
        {
            //we'll check if the property id map has been created first
            if (_nodePropertyIdMap == null)
                _nodePropertyIdMap = new List<KeyValuePair<KeyValuePair<int, string>, Guid>>();

            //check if the property map is found for the current model
            var id = (int)node.Parent.Attribute("id");
            var map = _nodePropertyIdMap.Where(x => x.Key.Key == id && x.Key.Value == node.Name.LocalName).SingleOrDefault();
            if (map.Value == default(Guid))
            {
                //if there's no map, we create one, this should occur only once
                map = new KeyValuePair<KeyValuePair<int, string>, Guid>(
                    new KeyValuePair<int, string>(id, node.Name.LocalName),
                    Guid.NewGuid());
                _nodePropertyIdMap.Add(map);
            }
            return map.Value;
        }
    }
}
