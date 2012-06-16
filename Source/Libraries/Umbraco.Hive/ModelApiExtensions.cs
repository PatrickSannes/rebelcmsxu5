using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Hive
{
    using Umbraco.Framework;
    using Umbraco.Framework.Persistence;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
    using Umbraco.Hive.RepositoryTypes;

    public static class ModelApiExtensions
    {
        public static T CreateSchema<T, TProviderFilter>(
            this IHiveManager hiveManager,
            string alias)
            where T : EntitySchema, new()
            where TProviderFilter : class, IProviderTypeFilter
        {
            var schema = new T {Alias = alias};
            return schema;
        }

        public static ISchemaBuilderStep<T, TProviderFilter> NewSchema<T, TProviderFilter>(
            this IHiveManager hiveManager,
            string alias)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            return new SchemaBuilderStep<T, TProviderFilter>(hiveManager, hiveManager.CreateSchema<T, TProviderFilter>(alias));
        }

        public static ISchemaBuilderStep<T, TProviderFilter> Define<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            string propertyAlias,
            AttributeType type,
            AttributeGroup group)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var def = new AttributeDefinition(propertyAlias, propertyAlias) { AttributeType = type, AttributeGroup = @group };
            builder.Item.AttributeDefinitions.Add(def);
            return builder;
        }

        public static ISchemaBuilderStep<T, TProviderFilter> Define<T, TProviderFilter>(
            this ISchemaBuilderStep<T, TProviderFilter> builder,
            string propertyAlias,
            Func<IBuilderStep<AttributeType, TProviderFilter>, ISchemaBuilderStep<AttributeType, TProviderFilter>> typeBuilder,
            AttributeGroup group)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var type = typeBuilder.Invoke(new BuilderStarter<AttributeType, TProviderFilter>(builder.HiveManager));
            return Define(builder, propertyAlias, type.Item, group);
        }

        public static ISchemaSaveResult<T> Commit<T, TProviderFilter>(this ISchemaBuilderStep<T, TProviderFilter> modelBuilder)
            where TProviderFilter : class, IProviderTypeFilter
            where T : AbstractSchemaPart, new()
        {
            using (var unit = modelBuilder.HiveManager.OpenWriter<TProviderFilter>())
            {
                var item = modelBuilder.Item;
                try
                {
                    unit.Repositories.Schemas.AddOrUpdate(item);
                    unit.Complete();
                    return new SaveResult<T>(true, item);
                }
                catch (Exception ex)
                {
                    unit.Abandon();
                    return new SaveResult<T>(false, item, ex);
                }
            }
        }
    }

    public static class ModelTypeApiExtensions
    {
        public static ISchemaBuilderStep<AttributeType, TProviderFilter> UseExistingType<TProviderFilter>(this IBuilderStep<AttributeType, TProviderFilter> builder, string alias)
            where TProviderFilter : class, IProviderTypeFilter
        {
            var registry = AttributeTypeRegistry.Current;
            return UseExistingType(builder, registry, alias);
        }

        public static ISchemaBuilderStep<AttributeType, TProviderFilter> UseExistingType<TProviderFilter>(this IBuilderStep<AttributeType, TProviderFilter> builder, IAttributeTypeRegistry registry, string alias)
            where TProviderFilter : class, IProviderTypeFilter
        {
            var check = registry.TryGetAttributeType(alias);
            if (!check.Success) throw new InvalidOperationException("AttributeType '{0}' is not registered with the supplied IAttributeTypeRegistry".InvariantFormat(alias));
            var existing = check.Result;
            return new SchemaBuilderStep<AttributeType, TProviderFilter>(builder.HiveManager, existing);
        }
    }

    public interface IBuilderStep<out T, out TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractEntity
    {
        IHiveManager HiveManager { get; }
    }

    public interface ISchemaBuilderStep<out T, out TProviderFilter> : IModelBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractSchemaPart
    {
    }

    public interface IModelBuilderStep<out T, out TProviderFilter> : IBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractEntity
    {
        T Item { get; }
    }

    public class BuilderStarter<T, TProviderFilter> : IBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractEntity
    {
        public BuilderStarter(IHiveManager hiveManager)
        {
            HiveManager = hiveManager;
        }

        public IHiveManager HiveManager { get; protected set; }
    }

    public class SchemaBuilderStep<T, TProviderFilter> : ISchemaBuilderStep<T, TProviderFilter>
        where TProviderFilter : class, IProviderTypeFilter
        where T : AbstractSchemaPart
    {
        public IHiveManager HiveManager { get; protected set; }

        public SchemaBuilderStep(IHiveManager hiveManager, T item)
        {
            HiveManager = hiveManager;
            Item = item;
        }

        public T Item { get; protected set; }
    }

    public interface ISaveResult
    {
        bool Success { get; }
        IEnumerable<Exception> Errors { get; }
    }

    public interface ISchemaSaveResult<T> : ISaveResult
        where T : AbstractSchemaPart
    {
        T Item { get; }
    }

    public class SaveResult<T> : ISchemaSaveResult<T>
        where T : AbstractSchemaPart
    {
        public SaveResult(bool success, T item, params Exception[] errors)
        {
            Success = success;
            Item = item;
            Errors = errors;
        }

        public bool Success { get; protected set; }

        public T Item { get; protected set; }

        public IEnumerable<Exception> Errors { get; protected set; }
    }
}
