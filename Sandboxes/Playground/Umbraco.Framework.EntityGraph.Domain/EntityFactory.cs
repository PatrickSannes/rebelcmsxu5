using System;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;

namespace Umbraco.Framework.EntityGraph.Domain.Brokers
{
    /// <summary>
    /// A generic service broker for emitting classes based upon <see cref="IEntity"/> and <see cref="IEntityVertex"/> interfaces.
    /// </summary>
    public static class EntityFactory
    {
        /// <summary>
        /// Setups the specified entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static T Setup<T>(this T entity, IEntityTypeDefinition entityType) where T : class, ITypedEntity, new()
        {
            entity.Setup();
            entity.EntityType = entityType;
            //TODO: How to make entity type attributes to entity attributes
            return entity;
        }

        /// <summary>
        /// Sets up the specified entity with default values.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static T Setup<T>(this T entity) where T : class, IEntity, new()
        {
            entity.Id = RepositoryGeneratedIdentifier.Create();
            entity.ConcurrencyToken = new RepositoryGeneratedToken();
            entity.Status = null; //TODO
            entity.UtcCreated = entity.UtcModified = entity.UtcStatusChanged = DateTime.UtcNow;
            return entity;
        }

        /// <summary>
        /// Sets up the specified entity with default values.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="alias">An alias.</param>
        /// <param name="name">A name.</param>
        /// <returns></returns>
        public static T Setup<T>(this T entity, string alias, string name)
            where T : class, IEntity, IReferenceByAlias, new()
        {
            entity.Setup();
            entity.Alias = alias;
            entity.Name = name;
            return entity;
        }

        /// <summary>
        /// Sets up the specified entity with default values.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="alias">An alias.</param>
        /// <param name="name">A name.</param>
        /// <returns></returns>
        public static T SetupRoot<T>(this T entity)
            where T : class, IEntityVertex, new()
        {
            entity.Setup();
            entity.IsRoot = true;
            entity.RootEntity = entity;
            return entity;
        }

        /// <summary>
        /// Sets up the specified entity with default values.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="alias">An alias.</param>
        /// <param name="name">A name.</param>
        /// <returns></returns>
        public static T Setup<T>(this T entity, string alias, string name, IEntityVertex parent)
            where T : class, IEntityVertex, IReferenceByAlias, new()
        {
            entity.Setup(alias, name);
            if (parent != null)
            {
                entity.ParentDynamic = parent;
                entity.ParentEntity = parent;
                entity.ParentId = parent.Id;
                entity.Depth = parent.Depth + 1;
                entity.RootEntity = parent.RootEntity;
            }
            entity.HasDescendents = false;
            entity.DescendentsDepth = 0;
            entity.AssociatedEntities = new EntityAssociationCollection();
            return entity;
        }

        /// <summary>
        /// Sets up the specified entity with default values.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="alias">An alias.</param>
        /// <param name="name">A name.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public static T SetupTypeDefinition<T>(this T entity, string alias, string name, IEntityVertex parent)
            where T : class, IEntityTypeDefinition, IReferenceByAlias, new()
        {
            entity.Setup(alias, name, parent);
            entity.GraphSchema = new EntityGraphSchema();
            entity.AttributeSchema = new AttributionSchemaDefinition();
            return entity;
        }




        /// <summary>
        /// Creates an instance of <typeparamref name="TAllowedType"/>.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <returns></returns>
        public static T Create<T>() where T : class, IEntity, new()
        {
            var entity = new T() as IEntity;
            return Setup((T)entity);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TAllowedType"/> having a reference by string alias.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static T Create<T>(string alias, string name) where T : class, IEntity, IReferenceByAlias, new()
        {
            var entity = Create<T>();
            return Setup(entity, alias, name);
        }

        /// <summary>
        /// Creates an <see cref="IAttributeDefinition"/> with a given <see cref="IAttributeTypeDefinition"/> and references it to a given <see cref="IAttributeGroupDefinition"/>.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <param name="attributeTypeDefinition">The attribute type definition.</param>
        /// <param name="serializationDefinition">The serialization definition.</param>
        /// <param name="attributeGroupDefinition">The attribute group definition.</param>
        /// <returns></returns>
        public static T CreateAttributeIn<T>(string alias, string name, IAttributeTypeDefinition attributeTypeDefinition, IAttributeSerializationDefinition serializationDefinition,
                                             IAttributeGroupDefinition attributeGroupDefinition)
            where T : class, IAttributeDefinition, new()
        {
            var entity = Create<T>(alias, name);
            entity.AttributeType = attributeTypeDefinition;
            entity.AttributeType.SerializationType = serializationDefinition;
            attributeGroupDefinition.AttributeDefinitions.Add(entity);
            return entity;
        }

        /// <summary>
        /// Creates an <see cref="IEntity"/> and places it in the given <paramref name="entityCollection"/>.
        /// </summary>
        /// <typeparam name="TAllowedType"></typeparam>
        /// <param name="alias">The alias.</param>
        /// <param name="name">The name.</param>
        /// <param name="entityCollection">The entity collection.</param>
        /// <returns></returns>
        public static T CreateIn<T>(string alias, string name, IEntityCollection<T> entityCollection)
            where T : class, IEntity, IReferenceByAlias, new()
        {
            var entity = Create<T>(alias, name);
            entityCollection.Add(entity);
            return entity;
        }
    }
}