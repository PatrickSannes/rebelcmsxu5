using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.RdbmsModel.Mapping
{

    //public class ManualMapper : AbstractTypeMapper
    //{
    //    private readonly DelegatedMapHandlerList _delegatedMapHandlers = new DelegatedMapHandlerList();
    //    private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
    //    private readonly AbstractLookupHelper _lookupHelper;
    //    private readonly IHiveProvider _hiveProvider;
    //    private readonly RdbmsToModelMapper _rdbmsToModelMapper;

    //    public ManualMapper(AbstractLookupHelper lookupHelper, IHiveProvider hiveProvider)
    //    {
    //        _lookupHelper = lookupHelper;
    //        _hiveProvider = hiveProvider;
    //        _rdbmsToModelMapper = new RdbmsToModelMapper(_hiveProvider);
    //    }

    //    #region Overrides of AbstractTypeMapper

    //    public override object Map(object source, Type sourceType, Type destinationType)
    //    {
    //        Mandate.ParameterNotNull(source, "source");
    //        Mandate.ParameterNotNull(sourceType, "sourceType");
    //        Mandate.ParameterNotNull(destinationType, "destinationType");

    //        var mapHandler =
    //            GetMapHandlerEqualPredicate(sourceType, destinationType)
    //                .Select(x => x.Value.ByReturn).FirstOrDefault() ??
    //            GetMapHandlerInheritedPredicate(sourceType, destinationType).
    //                Select(x => x.Value.ByReturn).FirstOrDefault();


    //        if (mapHandler == null)
    //            throw new InvalidOperationException(String.Format("Cannot map from {0} to {1} - no handler registered",
    //                                                sourceType.FullName, destinationType.FullName));

    //        LogHelper.TraceIfEnabled<ManualMapper>("Mapping {0} to {1} by outupt", () => sourceType.Name, () => destinationType.Name);
    //        return mapHandler.Invoke(source, _lookupHelper, this);
    //    }

    //    private IEnumerable<KeyValuePair<TypeMapperMetadata, TypeMapDelegatePair>> GetMapHandlerInheritedPredicate(Type sourceType, Type destinationType)
    //    {
    //        return _delegatedMapHandlers.Where(x => x.Key.PermitTypeInheritance && x.Key.SourceType.IsAssignableFrom(sourceType) && x.Key.DestinationType.IsAssignableFrom(destinationType));
    //    }

    //    private IEnumerable<KeyValuePair<TypeMapperMetadata, TypeMapDelegatePair>> GetMapHandlerEqualPredicate(Type sourceType, Type destinationType)
    //    {
    //        return _delegatedMapHandlers.Where(x => x.Key.SourceType.Equals(sourceType) && x.Key.DestinationType.Equals(destinationType));
    //    }

    //    public override void Map(object source, object destination, Type sourceType, Type destinationType)
    //    {
    //        Mandate.ParameterNotNull(source, "source");
    //        Mandate.ParameterNotNull(sourceType, "sourceType");
    //        Mandate.ParameterNotNull(destinationType, "destinationType");

    //        var mapHandler =
    //            GetMapHandlerEqualPredicate(sourceType, destinationType)
    //                .Select(x => x.Value.ByReference).FirstOrDefault() ??
    //            GetMapHandlerInheritedPredicate(sourceType, destinationType).
    //                Select(x => x.Value.ByReference).FirstOrDefault();

    //        if (mapHandler == null)
    //            throw new InvalidOperationException(String.Format("Cannot map from {0} to {1} - no handler registered",
    //                                                sourceType.FullName, destinationType.FullName));

    //        LogHelper.TraceIfEnabled<ManualMapper>("Mapping {0} to {1} by reference", () => sourceType.Name, () => destinationType.Name);
    //        mapHandler.Invoke(source, destination, _lookupHelper, this);
    //    }

    //    public override bool TryGetDestinationType(Type sourceType, out Type destinationType)
    //    {
    //        Mandate.ParameterNotNull(sourceType, "sourceType");

    //        destinationType =
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.SourceType.Equals(sourceType))
    //                .Select(x => x.Key.DestinationType).FirstOrDefault() ??
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.PermitTypeInheritance && x.Key.SourceType.IsAssignableFrom(sourceType))
    //                .Select(x => x.Key.DestinationType).FirstOrDefault() ??
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.DestinationType.Equals(sourceType))
    //                .Select(x => x.Key.SourceType).FirstOrDefault() ??
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.PermitTypeInheritance && x.Key.DestinationType.IsAssignableFrom(sourceType))
    //                .Select(x => x.Key.SourceType).FirstOrDefault();

    //        return destinationType != null;                
    //    }

    //    /// <summary>
    //    /// Tries to find a destination type given the <paramref name="sourceType"/> and a target type that is equal to or inherits from <paramref name="baseDestinationType"/>.
    //    /// </summary>
    //    /// <param name="sourceType">The source.</param>
    //    /// <param name="baseDestinationType">Base type of the destination.</param>
    //    /// <param name="destinationType">Type of the destination.</param>
    //    /// <returns></returns>
    //    /// <remarks></remarks>
    //    public override bool TryGetDestinationType(Type sourceType, Type baseDestinationType, out Type destinationType)
    //    {
    //        Mandate.ParameterNotNull(sourceType, "sourceType");
    //        Mandate.ParameterNotNull(baseDestinationType, "baseDestinationType");

    //        destinationType =
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.SourceType.Equals(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.DestinationType))
    //                .Select(x => x.Key.DestinationType).FirstOrDefault() ??
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.PermitTypeInheritance && x.Key.SourceType.IsAssignableFrom(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.DestinationType))
    //                .Select(x => x.Key.DestinationType).FirstOrDefault() ??
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.DestinationType.Equals(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.SourceType))
    //                .Select(x => x.Key.SourceType).FirstOrDefault() ??
    //            _delegatedMapHandlers
    //                .Where(x => x.Key.PermitTypeInheritance && x.Key.DestinationType.IsAssignableFrom(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.SourceType))
    //                .Select(x => x.Key.SourceType).FirstOrDefault();

    //        return destinationType != null;        
    //    }

    //    public override void Configure()
    //    {
    //        using (new WriteLockDisposable(_locker))
    //        {
    //            if (IsConfigured) return;

    //            _delegatedMapHandlers.Add<AttributeType, Model.Attribution.MetaData.AttributeType>(_rdbmsToModelMapper.MapAttributeTypeDefinition, _rdbmsToModelMapper.MapAttributeTypeDefinition, true);
    //            _delegatedMapHandlers.Add<Model.Attribution.MetaData.AttributeType, AttributeType>(ModelToRdbmsMapper.MapAttributeTypeDefinition, ModelToRdbmsMapper.MapAttributeTypeDefinition, true);

    //            _delegatedMapHandlers.Add<AttributeDefinition, Model.Attribution.MetaData.AttributeDefinition>(_rdbmsToModelMapper.MapAttributeDefinition, _rdbmsToModelMapper.MapAttributeDefinition, true);
    //            _delegatedMapHandlers.Add<Model.Attribution.MetaData.AttributeDefinition, AttributeDefinition>(ModelToRdbmsMapper.MapAttributeDefinition, ModelToRdbmsMapper.MapAttributeDefinition, true);

    //            _delegatedMapHandlers.Add<Attribute, TypedAttribute>(_rdbmsToModelMapper.MapAttribute, _rdbmsToModelMapper.MapAttribute, true);
    //            _delegatedMapHandlers.Add<TypedAttribute, Attribute>(ModelToRdbmsMapper.MapAttribute, ModelToRdbmsMapper.MapAttribute, true);

    //            _delegatedMapHandlers.Add<AttributeDefinitionGroup, AttributeGroup>(_rdbmsToModelMapper.MapAttributeGroupDefinition, _rdbmsToModelMapper.MapAttributeGroupDefinition, true);
    //            _delegatedMapHandlers.Add<AttributeGroup, AttributeDefinitionGroup>(ModelToRdbmsMapper.MapAttributeDefinitionGroup, ModelToRdbmsMapper.MapAttributeDefinitionGroup, true);

    //            _delegatedMapHandlers.Add<AttributeSchemaDefinition, EntitySchema>(_rdbmsToModelMapper.MapAttributeSchemaDefinition, _rdbmsToModelMapper.MapAttributeSchemaDefinition, true);
    //            _delegatedMapHandlers.Add<EntitySchema, AttributeSchemaDefinition>(ModelToRdbmsMapper.MapAttributeSchemaDefinition, ModelToRdbmsMapper.MapAttributeSchemaDefinition, true);

    //            _delegatedMapHandlers.Add<NodeVersionStatusType, RevisionStatusType>(_rdbmsToModelMapper.MapEntityStatusType, _rdbmsToModelMapper.MapEntityStatusType, true);
    //            _delegatedMapHandlers.Add<RevisionStatusType, NodeVersionStatusType>(ModelToRdbmsMapper.MapStatusType, ModelToRdbmsMapper.MapStatusType, true);


    //            _delegatedMapHandlers.Add<NodeVersion, TypedEntity>(_rdbmsToModelMapper.MapTypedEntityForRevision, _rdbmsToModelMapper.MapTypedEntityForRevision, true);
    //            _delegatedMapHandlers.Add<TypedEntity, NodeVersion>(ModelToRdbmsMapper.MapTypedEntityForRevision, ModelToRdbmsMapper.MapTypedEntityForRevision, true);

    //            //_delegatedMapHandlers.Add<Node, TypedEntity>(_rdbmsToModelMapper.MapMostRecentTypedEntity, _rdbmsToModelMapper.MapMostRecentTypedEntity, true);
    //            //_delegatedMapHandlers.Add<TypedEntity, Node>(ModelToRdbmsMapper.MapMostRecentTypedEntity, ModelToRdbmsMapper.MapMostRecentTypedEntity, true);

    //            _delegatedMapHandlers.Add<NodeVersion, Revision<TypedEntity>>(_rdbmsToModelMapper.MapRevision, _rdbmsToModelMapper.MapRevision, true);
    //            _delegatedMapHandlers.Add<Revision<TypedEntity>, NodeVersion>(ModelToRdbmsMapper.MapFromRevision, ModelToRdbmsMapper.MapFromRevision, true);

    //            IsConfigured = true;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets supported mappings which can only be declared dynamically at runtime, as opposed to through attribution at compile time
    //    /// by decoration with <see cref="TypeMapperAttribute"/>.
    //    /// </summary>
    //    /// <returns></returns>
    //    /// <remarks></remarks>
    //    public override IEnumerable<TypeMapperMetadata> GetDynamicSupportedMappings()
    //    {
    //        return _delegatedMapHandlers.Select(x => x.Key);
    //    }

    //    #endregion
    //}

    public class ManualMapperv2 : AbstractMappingEngine
    {
        private readonly DelegatedMapHandlerList _delegatedMapHandlers = new DelegatedMapHandlerList();
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private readonly AbstractLookupHelper _lookupHelper;
        private readonly ProviderMetadata _hiveProvider;
        private readonly RdbmsToModelMapper _rdbmsToModelMapper;

        public ManualMapperv2(AbstractLookupHelper lookupHelper, ProviderMetadata hiveProvider)
        {
            _lookupHelper = lookupHelper;
            _hiveProvider = hiveProvider;
            _rdbmsToModelMapper = new RdbmsToModelMapper(_hiveProvider);
        }

        #region Overrides of AbstractTypeMapper

        public override object Map(object source, Type sourceType, Type destinationType)
        {
            Mandate.ParameterNotNull(source, "source");
            Mandate.ParameterNotNull(sourceType, "sourceType");
            Mandate.ParameterNotNull(destinationType, "destinationType");

            var equalPredicate = GetMapHandlerEqualPredicate(sourceType, destinationType)
                .Select(x => x.Value.ByReturn).FirstOrDefault();
            var inheritedPredicate = GetMapHandlerInheritedPredicate(sourceType, destinationType).
                Select(x => x.Value.ByReturn).FirstOrDefault();

            var mapHandler = equalPredicate ?? inheritedPredicate;

            if (mapHandler == null)
                throw new InvalidOperationException(String.Format("Cannot map from {0} to {1} - no handler registered",
                                                    sourceType.FullName, destinationType.FullName));

            // New Sep 2011: If we're using the delegate based on inheritance, since we're
            // outputting a new object in this Map method, we must create a new instance of the destination type
            // and use the inheritance-based map delegate to map onto it so that we return the correct
            // type and not the base one found in the mappings collection
            if (mapHandler == inheritedPredicate)
            {
                try
                {
                    var mapTo = Activator.CreateInstance(destinationType);
                    Map(source, mapTo, sourceType, destinationType);
                    return mapTo;
                }
                catch (MissingMethodException)
                {
                    LogHelper.TraceIfEnabled<ManualMapperv2>("Could not map from {0} to {1} because {1} does not have a parameterless constructor", () => sourceType.Name, () => destinationType.Name);
                }
            }

            return mapHandler.Invoke(source, _lookupHelper, this);
        }

        private IEnumerable<KeyValuePair<TypeMapperMetadata, TypeMapDelegatePair>> GetMapHandlerInheritedPredicate(Type sourceType, Type destinationType)
        {
            return _delegatedMapHandlers.Where(x => x.Key.PermitTypeInheritance && x.Key.SourceType.IsAssignableFrom(sourceType) && x.Key.DestinationType.IsAssignableFrom(destinationType));
        }

        private IEnumerable<KeyValuePair<TypeMapperMetadata, TypeMapDelegatePair>> GetMapHandlerEqualPredicate(Type sourceType, Type destinationType)
        {
            return _delegatedMapHandlers.Where(x => x.Key.SourceType.Equals(sourceType) && x.Key.DestinationType.Equals(destinationType));
        }

        public override void Map(object source, object destination, Type sourceType, Type destinationType)
        {
            Mandate.ParameterNotNull(source, "source");
            Mandate.ParameterNotNull(sourceType, "sourceType");
            Mandate.ParameterNotNull(destinationType, "destinationType");

            var mapHandler =
                GetMapHandlerEqualPredicate(sourceType, destinationType)
                    .Select(x => x.Value.ByReference).FirstOrDefault() ??
                GetMapHandlerInheritedPredicate(sourceType, destinationType).
                    Select(x => x.Value.ByReference).FirstOrDefault();

            if (mapHandler == null)
                throw new InvalidOperationException(String.Format("Cannot map from {0} to {1} - no handler registered",
                                                    sourceType.FullName, destinationType.FullName));

            mapHandler.Invoke(source, destination, _lookupHelper, this);
        }

        public override bool TryGetDestinationType(Type sourceType, out Type destinationType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");

            destinationType =
                _delegatedMapHandlers
                    .Where(x => x.Key.SourceType.Equals(sourceType))
                    .Select(x => x.Key.DestinationType).FirstOrDefault() ??
                _delegatedMapHandlers
                    .Where(x => x.Key.PermitTypeInheritance && x.Key.SourceType.IsAssignableFrom(sourceType))
                    .Select(x => x.Key.DestinationType).FirstOrDefault() ??
                _delegatedMapHandlers
                    .Where(x => x.Key.DestinationType.Equals(sourceType))
                    .Select(x => x.Key.SourceType).FirstOrDefault() ??
                _delegatedMapHandlers
                    .Where(x => x.Key.PermitTypeInheritance && x.Key.DestinationType.IsAssignableFrom(sourceType))
                    .Select(x => x.Key.SourceType).FirstOrDefault();

            return destinationType != null;
        }

        /// <summary>
        /// Tries to find a destination type given the <paramref name="sourceType"/> and a target type that is equal to or inherits from <paramref name="baseDestinationType"/>.
        /// </summary>
        /// <param name="sourceType">The source.</param>
        /// <param name="baseDestinationType">Base type of the destination.</param>
        /// <param name="destinationType">Type of the destination.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override bool TryGetDestinationType(Type sourceType, Type baseDestinationType, out Type destinationType)
        {
            Mandate.ParameterNotNull(sourceType, "sourceType");
            Mandate.ParameterNotNull(baseDestinationType, "baseDestinationType");

            destinationType =
                _delegatedMapHandlers
                    .Where(x => x.Key.SourceType.Equals(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.DestinationType))
                    .Select(x => x.Key.DestinationType).FirstOrDefault() ??
                _delegatedMapHandlers
                    .Where(x => x.Key.PermitTypeInheritance && x.Key.SourceType.IsAssignableFrom(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.DestinationType))
                    .Select(x => x.Key.DestinationType).FirstOrDefault() ??
                _delegatedMapHandlers
                    .Where(x => x.Key.DestinationType.Equals(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.SourceType))
                    .Select(x => x.Key.SourceType).FirstOrDefault() ??
                _delegatedMapHandlers
                    .Where(x => x.Key.PermitTypeInheritance && x.Key.DestinationType.IsAssignableFrom(sourceType) && baseDestinationType.IsAssignableFrom(x.Key.SourceType))
                    .Select(x => x.Key.SourceType).FirstOrDefault();

            return destinationType != null;
        }

        public override void Configure()
        {
            using (new WriteLockDisposable(_locker))
            {
                if (IsConfigured) return;

                _delegatedMapHandlers.Add<AttributeType, Model.Attribution.MetaData.AttributeType>(_rdbmsToModelMapper.MapAttributeTypeDefinition, _rdbmsToModelMapper.MapAttributeTypeDefinition, true);
                _delegatedMapHandlers.Add<Model.Attribution.MetaData.AttributeType, AttributeType>(ModelToRdbmsMapper.MapAttributeTypeDefinition, ModelToRdbmsMapper.MapAttributeTypeDefinition, true);

                _delegatedMapHandlers.Add<AttributeDefinition, Model.Attribution.MetaData.AttributeDefinition>(_rdbmsToModelMapper.MapAttributeDefinition, _rdbmsToModelMapper.MapAttributeDefinition, true);
                _delegatedMapHandlers.Add<Model.Attribution.MetaData.AttributeDefinition, AttributeDefinition>(ModelToRdbmsMapper.MapAttributeDefinition, ModelToRdbmsMapper.MapAttributeDefinition, true);

                _delegatedMapHandlers.Add<Attribute, TypedAttribute>(_rdbmsToModelMapper.MapAttribute, _rdbmsToModelMapper.MapAttribute, true);
                _delegatedMapHandlers.Add<TypedAttribute, Attribute>(ModelToRdbmsMapper.MapAttribute, ModelToRdbmsMapper.MapAttribute, true);

                _delegatedMapHandlers.Add<AttributeDefinitionGroup, AttributeGroup>(_rdbmsToModelMapper.MapAttributeGroupDefinition, _rdbmsToModelMapper.MapAttributeGroupDefinition, true);
                _delegatedMapHandlers.Add<AttributeGroup, AttributeDefinitionGroup>(ModelToRdbmsMapper.MapAttributeDefinitionGroup, ModelToRdbmsMapper.MapAttributeDefinitionGroup, true);

                _delegatedMapHandlers.Add<AttributeSchemaDefinition, EntitySchema>(_rdbmsToModelMapper.MapAttributeSchemaDefinition, _rdbmsToModelMapper.MapAttributeSchemaDefinition, true);
                _delegatedMapHandlers.Add<EntitySchema, AttributeSchemaDefinition>(ModelToRdbmsMapper.MapAttributeSchemaDefinition, ModelToRdbmsMapper.MapAttributeSchemaDefinition, true);

                _delegatedMapHandlers.Add<NodeVersionStatusType, RevisionStatusType>(_rdbmsToModelMapper.MapEntityStatusType, _rdbmsToModelMapper.MapEntityStatusType, true);
                _delegatedMapHandlers.Add<RevisionStatusType, NodeVersionStatusType>(ModelToRdbmsMapper.MapStatusType, ModelToRdbmsMapper.MapStatusType, true);


                _delegatedMapHandlers.Add<NodeVersion, TypedEntity>(_rdbmsToModelMapper.MapTypedEntityForRevision, _rdbmsToModelMapper.MapTypedEntityForRevision, true);
                _delegatedMapHandlers.Add<TypedEntity, NodeVersion>(ModelToRdbmsMapper.MapTypedEntityForRevision, ModelToRdbmsMapper.MapTypedEntityForRevision, true);

                //_delegatedMapHandlers.Add<Node, TypedEntity>(_rdbmsToModelMapper.MapMostRecentTypedEntity, _rdbmsToModelMapper.MapMostRecentTypedEntity, true);
                //_delegatedMapHandlers.Add<TypedEntity, Node>(ModelToRdbmsMapper.MapMostRecentTypedEntity, ModelToRdbmsMapper.MapMostRecentTypedEntity, true);

                _delegatedMapHandlers.Add<NodeVersion, Revision<TypedEntity>>(_rdbmsToModelMapper.MapRevision, _rdbmsToModelMapper.MapRevision, true);
                _delegatedMapHandlers.Add<Revision<TypedEntity>, NodeVersion>(ModelToRdbmsMapper.MapFromRevision, ModelToRdbmsMapper.MapFromRevision, true);

                IsConfigured = true;
            }
        }

        /// <summary>
        /// Gets supported mappings which can only be declared dynamically at runtime, as opposed to through attribution at compile time
        /// by decoration with <see cref="TypeMapperAttribute"/>.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override IEnumerable<TypeMapperMetadata> GetDynamicSupportedMappings()
        {
            return _delegatedMapHandlers.Select(x => x.Key);
        }

        #endregion
    }
}
