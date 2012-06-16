using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Dynamics.Expressions;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Hive;
namespace Umbraco.Cms.Web
{
    using Umbraco.Cms.Web.Security.Permissions;
    using Umbraco.Framework.Persistence.Model.Associations;
    using Umbraco.Framework.Security;

    public static class RenderViewModelExtensions
    {

        /// <summary>
        /// Gets the path of the content 
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static EntityPath GetPath(this Content content)
        {
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return GetPath(content, appContext.Hive);
            }
            return new EntityPath(content.Id.AsEnumerableOfOne());
        }

        public static EntityPath GetPath(this Content content, IHiveManager hiveManager)
        {
            // Open a reader and pass in the scoped cache which should be per http-request
            using (var uow = hiveManager.OpenReader<IContentStore>(hiveManager.FrameworkContext.ScopedCache))
            {
                return uow.Repositories.GetEntityPath(content.Id, FixedRelationTypes.DefaultRelationType);
            }
        }

        /// <summary>
        /// Returns the url 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string NiceUrl(this Content content)
        {
            var urlUtility = DependencyResolver.Current.GetService<IRoutingEngine>();
            return urlUtility.GetUrl(content.Id);
        }

        public static IEnumerable<T> GetAncestors<T>(this IHiveManager hiveManager, HiveId descendentId, RelationType relationType = null, string relationAlias = null)
            where T : class, IRelatableEntity
        {
            // Open a reader and pass in the scoped cache which should be per http-request
            using (var uow = hiveManager.OpenReader<IContentStore>(hiveManager.FrameworkContext.ScopedCache))
            {
                if (relationType != null && relationAlias.IsNullOrWhiteSpace())
                    relationType = new RelationType(relationAlias);

                return uow.Repositories.GetAncestors(descendentId, relationType)
                    .ForEach(x => hiveManager.FrameworkContext.TypeMappers.Map<T>(x));
            }
        }

        public static T Parent<T>(IHiveManager hiveManager, ISecurityService security, HiveId childId)
            where T : TypedEntity
        {
            // Open a reader and pass in the scoped cache which should be per http-request
            using (var uow = hiveManager.OpenReader<IContentStore>(hiveManager.FrameworkContext.ScopedCache))
            {
                // TODO: Add path of current to this

                // We get the relations by id here because we only want to load the parent entity, not both halves of the relation
                // (as of 15h Oct 2011 but this should change once UoW gets its own ScopedCache added to cater for this - APN)
                var firstParentFound = uow.Repositories.GetParentRelations(childId, FixedRelationTypes.DefaultRelationType)
                        .FirstOrDefault();

                if (firstParentFound != null)
                {
                    var parentId = firstParentFound.SourceId.AsEnumerableOfOne();

                    // Filter the ancestor ids based on anonymous view permissions
                    using (var secUow = hiveManager.OpenReader<ISecurityStore>(hiveManager.FrameworkContext.ScopedCache))
                        parentId = parentId.FilterAnonymousWithPermissions(security, uow, secUow, new ViewPermission().Id).ToArray();

                    if (!parentId.Any()) return null;

                    //var parent = uow.Repositories.Get<T>(parentId);
                    //if (parent != null)
                    //    return hiveManager.FrameworkContext.TypeMappers.Map<T>(parent);
                    var revision = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(parentId.FirstOrDefault(), FixedStatusTypes.Published);
                    return revision != null ? hiveManager.FrameworkContext.TypeMappers.Map<T>(revision.Item) : null;
                }
            }
            return null;
        }

        public static IEnumerable<Content> AncestorContent(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<Content>>(
                    "rvme_AncestorContent_" + content.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetAncestorIds(content.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                                hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            var ancestors = uow.Repositories.Revisions.GetLatestRevisions<TypedEntity>(false, FixedStatusTypes.Published, hiveIds);
                            return ancestors
                                .WhereNotNull()
                                .Where(x => x.Item.EntitySchema != null && !x.Item.EntitySchema.Id.IsSystem())
                                .Select(x => appContext.FrameworkContext.TypeMappers.Map<Content>(x.Item))
                                .ToArray();
                        }
                    });
            }
            return Enumerable.Empty<Content>();
        }

        /// <summary>
        /// Gets all ancestor ids of the <paramref name="entity"/> regardless of publish status.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> AllAncestorIds(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<HiveId>>(
                    "rvme_AllAncestorIds_" + entity.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetAncestorIds(entity.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                                hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            return hiveIds;
                        }
                    });
            }
            return Enumerable.Empty<HiveId>();
        }

        public static IEnumerable<HiveId> AllAncestorIdsOrSelf(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            return entity.Id.AsEnumerableOfOne().Union(entity.AllAncestorIds());
        }


        /// <summary>
        /// Gets all descendant ids of the <paramref name="entity"/> regardless of publish status.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> AllDescendantIds(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<HiveId>>(
                    "rvme_AllDescendantIds_" + entity.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetDescendantIds(entity.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                                hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            return hiveIds;
                        }
                    });
            }
            return Enumerable.Empty<HiveId>();
        }

        /// <summary>
        /// Gets all descendant ids of the <paramref name="entity"/> regardless of publish status.
        /// </summary>
        /// <param name="entity">The content.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> AllChildIds(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<HiveId>>(
                    "rvme_AllChildIds_" + entity.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetChildRelations(entity.Id, FixedRelationTypes.DefaultRelationType).Select(x => x.DestinationId).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                                hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            return hiveIds;
                        }
                    });
            }
            return Enumerable.Empty<HiveId>();
        }

        public static IEnumerable<HiveId> AllDescendantIdsOrSelf(this TypedEntity entity)
        {
            if (entity == null) return Enumerable.Empty<HiveId>();
            return entity.Id.AsEnumerableOfOne().Union(entity.AllDescendantIds());
        }

        public static IEnumerable<Content> DescendantContent(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<Content>>(
                    "rvme_DescendantContent_" + content.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            var hiveIds = uow.Repositories.GetDescendantIds(content.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            // Filter the ancestor ids based on anonymous view permissions
                            using (var secUow = appContext.Hive.OpenReader<ISecurityStore>(appContext.FrameworkContext.ScopedCache))
                                hiveIds = hiveIds.FilterAnonymousWithPermissions(appContext.Security, uow, secUow, new ViewPermission().Id).ToArray();

                            var descendants = uow.Repositories.Revisions.GetLatestRevisions<TypedEntity>(false, FixedStatusTypes.Published, hiveIds);
                            return descendants
                                .WhereNotNull()
                                .Where(x => x.Item.EntitySchema != null && !x.Item.EntitySchema.Id.IsSystem())
                                .Select(x => appContext.FrameworkContext.TypeMappers.Map<Content>(x.Item))
                                .ToArray();
                        }
                    });
            }
            return Enumerable.Empty<Content>();
        }

        public static IEnumerable<Content> AncestorContentOrSelf(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            return content.AsEnumerableOfOne().Union(content.AncestorContent());
        }

        public static IEnumerable<Content> DescendantContentOrSelf(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();
            return content.AsEnumerableOfOne().Union(content.DescendantContent());
        }

        public static Content ParentContent(this Content content)
        {
            if (content == null) return null;

            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext
                    .ScopedCache
                    .GetOrCreateTyped<Content>("rvme_ParentContent_" + content.Id,
                        () =>
                        {
                            var parent = Parent<Content>(appContext.Hive, appContext.Security, content.Id);
                            return parent;
                        });
            }
            return null;
        }

        public static IQueryable<Content> Children(this Content content)
        {
            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return content.Children(appContext.Hive);
            }
            return Enumerable.Empty<Content>().AsQueryable();
        }

        public static IQueryable<Content> Children(this Content content, IHiveManager hiveManager)
        {
            var childIds = content.AllChildIds();
            return hiveManager.QueryContent().OfRevisionType(FixedStatusTypes.Published).InIds(childIds);
        }

        public static IEnumerable<Content> ChildContent(this Content content)
        {
            if (content == null) return Enumerable.Empty<Content>();

            return content.Children();

            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return appContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IEnumerable<Content>>(
                    "rvme_ChildContent_" + content.Id,
                    () =>
                    {
                        // Open a reader and pass in the scoped cache which should be per http-request
                        using (var uow = appContext.Hive.OpenReader<IContentStore>(appContext.FrameworkContext.ScopedCache))
                        {
                            // TODO: Add path of current to this

                            // Here we just get the child relations by id, and then bulk ask the repo for items with those ids,
                            // because we're caching the resultset anyway which means we can't yield the result

                            // Using this method rather than GetLazyChildRelations avoids the Source (our parent) being loaded too
                            // (as of 15h Oct 2011 but this should change once UoW gets its own ScopedCache added to cater for this - APN)
                            var childIds = uow.Repositories.GetChildRelations(
                                content.Id, FixedRelationTypes.DefaultRelationType)
                                .Select(x => x.DestinationId)
                                .ToArray();

                            var children = uow.Repositories.Revisions.GetLatestRevisions<TypedEntity>(false, FixedStatusTypes.Published, childIds);
                            return children
                                .WhereNotNull()
                                .Where(x => x.Item.EntitySchema != null && !x.Item.EntitySchema.Id.IsSystem())
                                .Select(x => appContext.FrameworkContext.TypeMappers.Map<Content>(x.Item))
                                .ToArray();
                        }
                    });
            }
            return Enumerable.Empty<Content>();
        }

        /// <summary>
        /// Returns a dynamic representation of the provided content object.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static dynamic AsDynamic(this Content content)
        {
            Mandate.ParameterNotNull(content, "content");

            return content.Bend();
        }

        /// <summary>
        /// Turns a list of content items into a list of dynamic content items
        /// </summary>
        /// <param name="contentItems"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> AsDynamic(this IEnumerable<Content> contentItems)
        {
            return contentItems.Select(item => item.AsDynamic());
        }

        #region Dynamic

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey)
        {
            return coll.Field(fieldKey, false);
        }

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey, bool recursive)
        {
            return coll.Field(fieldKey, recursive);
        }

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey, string valueField)
        {
            return coll.Field(fieldKey, valueField, false);
        }

        public static dynamic DynamicField(this TypedEntity coll, string fieldKey, string valueField, bool recursive)
        {
            return coll.Field(fieldKey, valueField, recursive);
        }

        #endregion

        #region Generic

        public static T Field<T>(this TypedEntity coll, string fieldKey)
        {
            return (T)coll.Field(fieldKey, false);
        }

        public static T Field<T>(this TypedEntity coll, string fieldKey, bool recursive)
        {
            return (T)coll.Field(fieldKey, recursive);
        }

        public static T Field<T>(this TypedEntity coll, string fieldKey, string valueField)
        {
            return (T)coll.Field(fieldKey, valueField, false);
        }

        public static T Field<T>(this TypedEntity coll, string fieldKey, string valueField, bool recursive)
        {
            return (T)coll.Field(fieldKey, valueField, recursive);
        }

        #endregion

        #region String

        public static string StringField(this TypedEntity coll, string fieldKey)
        {
            return coll.Field<string>(fieldKey, false);
        }

        public static string StringField(this TypedEntity coll, string fieldKey, bool recursive)
        {
            return coll.Field<string>(fieldKey, recursive);
        }

        public static string StringField(this TypedEntity coll, string fieldKey, string valueField)
        {
            return coll.Field<string>(fieldKey, valueField, false);
        }

        public static string StringField(this TypedEntity coll, string fieldKey, string valueField, bool recursive)
        {
            return coll.Field<string>(fieldKey, valueField, recursive);
        }

        #endregion

        #region Number

        public static Int32 NumberField(this TypedEntity coll, string fieldKey)
        {
            return coll.Field<Int32>(fieldKey, false);
        }

        public static Int32 NumberField(this TypedEntity coll, string fieldKey, bool recursive)
        {
            return coll.Field<Int32>(fieldKey, recursive);
        }

        public static Int32 NumberField(this TypedEntity coll, string fieldKey, string valueField)
        {
            return coll.Field<Int32>(fieldKey, valueField, false);
        }

        public static Int32 NumberField(this TypedEntity coll, string fieldKey, string valueField, bool recursive)
        {
            return coll.Field<Int32>(fieldKey, valueField, recursive);
        }

        #endregion

        #region Boolean

        public static bool BooleanField(this TypedEntity coll, string fieldKey)
        {
            return coll.Field<bool>(fieldKey, false);
        }

        public static bool BooleanField(this TypedEntity coll, string fieldKey, bool recursive)
        {
            return coll.Field<bool>(fieldKey, recursive);
        }

        public static bool BooleanField(this TypedEntity coll, string fieldKey, string valueField)
        {
            return coll.Field<bool>(fieldKey, valueField, false);
        }

        public static bool BooleanField(this TypedEntity coll, string fieldKey, string valueField, bool recursive)
        {
            return coll.Field<bool>(fieldKey, valueField, recursive);
        }

        #endregion

        #region Object

        public static object Field(this TypedEntity coll, string fieldKey)
        {
            return coll.Field(fieldKey, false);
        }

        public static object Field(this TypedEntity coll, string fieldKey, bool recursive)
        {
            return coll.Field(fieldKey, "", recursive);
        }

        public static object Field(this TypedEntity coll, string fieldKey, string valueKey)
        {
            return coll.Field(fieldKey, valueKey, false);
        }

        public static object Field(this TypedEntity coll, string fieldKey, string valueKey, bool recursive)
        {
            var contentToCheck = new List<TypedEntity>(new[] { coll });

            if (recursive)
            {
                var newContent = new Content(coll.Id, coll.Attributes);
                contentToCheck.AddRange(newContent.AncestorContent());
            }

            foreach (var content in contentToCheck)
            {
                var field = content.Attributes.Where(x => x.AttributeDefinition.Alias == fieldKey).FirstOrDefault();
                if (field != null)
                {
                    var defaultValue = (!valueKey.IsNullOrWhiteSpace()) ? field.Values[valueKey] : field.Values.GetDefaultValue();
                    if (defaultValue != null && !defaultValue.ToString().IsNullOrWhiteSpace()) return defaultValue;
                    //TODO: Update to be able to pass in Action<bool> to perform a null check based on specific types rather than just ToString
                }
            }

            return null;
        }

        #endregion

        public static KeyedFieldValue DefaultValue(this IEnumerable<KeyedFieldValue> coll)
        {
            Mandate.ParameterNotNull(coll, "coll");

            KeyedFieldValue tryDefault =
                coll.Where(x => x.Key == TypedAttributeValueCollection.DefaultAttributeValueKey).FirstOrDefault();
            return tryDefault ?? coll.FirstOrDefault();
        }

        public static dynamic Bend(this Content content)
        {
            if (content == null) return null;

            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            if (appContext != null && appContext.Hive != null)
            {
                return content.Bend(appContext.Hive);
            }

            return null;
        }

        public static dynamic Bend(this Content content, IHiveManager hiveManager)
        {
            if (content == null) return null;

            var bendy = new BendyObject(content);
            dynamic dynamicBendy = bendy;

            //get all fields that have a value 
            foreach (var typedAttribute in
                content.Attributes.Where(typedAttribute => typedAttribute.Values.Any()))
            {

                //if there are more than 1 value, put them into a sub bendy
                if (typedAttribute.Values.Count() > 1)
                {
                    var subBendy1 = new BendyObject();
                    var subBendy2 = new BendyObject();

                    foreach (var v in typedAttribute.Values)
                    {
                        subBendy1[v.Key.ToUmbracoAlias(StringAliasCaseType.PascalCase)] = v.Value;
                        subBendy2[v.Key.ToUmbracoAlias(StringAliasCaseType.Unchanged)] = v.Value;
                    }

                    bendy[typedAttribute.AttributeDefinition.Alias.ToUmbracoAlias(StringAliasCaseType.PascalCase)] = subBendy1;
                    bendy[typedAttribute.AttributeDefinition.Alias.ToUmbracoAlias(StringAliasCaseType.Unchanged)] = subBendy2;
                }
                else
                {
                    bendy[typedAttribute.AttributeDefinition.Alias.ToUmbracoAlias(StringAliasCaseType.PascalCase)] = typedAttribute.Values.First().Value;
                    bendy[typedAttribute.AttributeDefinition.Alias.ToUmbracoAlias(StringAliasCaseType.Unchanged)] = typedAttribute.Values.First().Value;
                }
            }

            bendy["__OriginalItem"] = content;

            bendy.WhenItemNotFound = (bent, membername) =>
                {
                    if (membername.IsNullOrWhiteSpace()) return new BendyObject();
                    // Support recursively looking up a property similarly to v4
                    if (membername.StartsWith("_") && membername.ElementAt(1).IsLowerCase())
                    {
                        return content.Field(membername.TrimStart('_'), true);
                    }
                    // Support pluralised document-type aliases inferring a query
                    if (membername.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var alias = membername.TrimEnd("s");
                        var up = alias.StartsWith("Ancestor", StringComparison.InvariantCultureIgnoreCase);
                        if (up) alias = alias.TrimStart("Ancestor");
                        dynamic theBendy = bent;

                        return up ? theBendy.AncestorsOrSelf.Where("NodeTypeAlias == @0", alias) : theBendy.DescendantsOrSelf.Where("NodeTypeAlias == @0", alias);
                    }
                    return new BendyObject();
                };

            bendy.AddLazy("AllAncestorIds", () => content.AllAncestorIds());
            bendy.AddLazy("Ancestors", () =>
                {
                    var ancestorIds = content.AllAncestorIds();
                    return new DynamicContentList(content.Id, hiveManager, new BendyObject(), ancestorIds);
                });
            bendy.AddLazy("AllAncestorIdsOrSelf", () => content.AllAncestorIdsOrSelf());
            bendy.AddLazy("AncestorsOrSelf", () =>
                {
                    var ancestorIds = content.AllAncestorIdsOrSelf();
                    return new DynamicContentList(content.Id, hiveManager, new BendyObject(), ancestorIds);
                });
            bendy.AddLazy("AllDescendantIds", () => content.AllDescendantIds());
            bendy.AddLazy("Descendants", () =>
                {
                    var descendantContentIds = content.AllDescendantIds();
                    return new DynamicContentList(content.Id, hiveManager, new BendyObject(), descendantContentIds);
                });
            bendy.AddLazy("AllDescendantIdsOrSelf", () => content.AllDescendantIdsOrSelf());
            bendy.AddLazy("DescendantsOrSelf", () =>
                {
                    var descendantContentIds = content.AllDescendantIdsOrSelf();
                    return new DynamicContentList(content.Id, hiveManager, new BendyObject(), descendantContentIds);
                });
            bendy.AddLazy("AllChildIds", () => content.AllChildIds());
            bendy.AddLazy("Children", () =>
                {
                    var childIds = content.AllChildIds();
                    return new DynamicContentList(content.Id, hiveManager, new BendyObject(), childIds);
                });
            bendy.AddLazy("Parent", () => content.ParentContent().Bend());
            bendy.AddLazy("Path", () => content.GetPath(hiveManager));
            bendy.AddLazy("PathById", () => content.GetPath(hiveManager));
            bendy.AddLazy("Level", () => content.GetPath(hiveManager).Level);

            //Add lazy url property
            bendy.AddLazy("Url", content.NiceUrl);
            bendy.AddLazy("NiceUrl", content.NiceUrl);
            bendy.AddLazy("NodeTypeAlias", () => content.ContentType.IfNotNull(x => x.Alias, string.Empty));
            bendy.AddLazy("Template", () => content.CurrentTemplate.IfNotNull(x => x.Alias, string.Empty));
            bendy.AddLazy("TemplateId", () => content.CurrentTemplate.IfNotNull(x => x.Id, HiveId.Empty));
            bendy.AddLazy("TemplateFileName", () => content.CurrentTemplate.IfNotNull(x => (string)x.Id.Value, string.Empty));
            bendy.AddLazy("CreateDate", () => content.UtcCreated);
            bendy.AddLazy("UpdateDate", () => content.UtcModified);
            bendy.AddLazy("StatusChangedDate", () => content.UtcStatusChanged);

            dynamicBendy.ContentType = content.ContentType;
            var nodeNameAlias = NodeNameAttributeDefinition.AliasValue.ToUmbracoAlias(StringAliasCaseType.PascalCase);
            try
            {
                dynamicBendy.Name = bendy[nodeNameAlias].Name;
                dynamicBendy.UrlName = bendy[nodeNameAlias].UrlName;
            }
            catch
            {
                /* Nothing */
            }

            dynamicBendy.Id = content.Id;

            return dynamicBendy;
        }
    }
}