using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.Providers.Membership.Config;
using Umbraco.Hive.Providers.Membership.Linq;

namespace Umbraco.Hive.Providers.Membership.Hive
{
    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public class EntityRepository : AbstractEntityRepository
    {
        protected IEnumerable<MembershipProvider> MembershipProviders { get; private set; }
        protected MembershipWrapperHelper Helper { get; private set; }

        public EntityRepository(
            ProviderMetadata providerMetadata,
            IFrameworkContext frameworkContext,
            IEnumerable<MembershipProvider> membershipProviders,
            IEnumerable<ProviderElement> configuredProviders)
            : base(providerMetadata, frameworkContext)
        {
            var providers = membershipProviders.ToArray();
            MembershipProviders = providers;
            Helper = new MembershipWrapperHelper(configuredProviders, providers, frameworkContext);
        }

        protected override void DisposeResources()
        {
            Schemas.Dispose();
            Revisions.Dispose();
            Transaction.Dispose();
        }

        public override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            return Helper.PerformGet<T>(allOrNothing, ids);
        }

        public override IEnumerable<T> PerformExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            //var direction = query.From.HierarchyScope;

            var results = Helper.PerformGetByQuery<T>(query, objectBinder);

            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Count:
                    //this is weird but returns an integer
                    return new[] { results.Count() }.Cast<T>();
                case ResultFilterType.Take:
                    results = results.Take(query.ResultFilter.SelectorArgument);
                    break;
            }

            if (typeof(T).IsAssignableFrom(query.ResultFilter.ResultType))
            {
                return results.Distinct().Select(node => FrameworkContext.TypeMappers.Map<T>(node));
            }

            return Enumerable.Empty<T>();

        }

        public override T PerformExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Count:
                    //this is weird but returns an integer
                    return ExecuteMany<T>(query, objectBinder).Single();
            }

            var many = ExecuteMany<T>(query, objectBinder);

            return many.Single();
        }

        public override T PerformExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Single:
                case ResultFilterType.SingleOrDefault:
                    var result = Helper.PerformGetByQuery<T>(query, objectBinder);
                    if (!result.Any())
                    {
                        if (query.ResultFilter.ResultFilterType == ResultFilterType.SingleOrDefault)
                        {
                            return default(T);
                        }
                        throw new InvalidOperationException("Sequence contains 0 elements but query specified exactly 1 must be present");
                    }
                    if (result.Count() > 1)
                    {
                        throw new InvalidOperationException("Sequence contains {0} elements but query specified exactly 1 must be present.".InvariantFormat(result.Count()));
                    }
                    return FrameworkContext.TypeMappers.Map<T>(result.Single());
            }

            return ExecuteMany<T>(query, objectBinder).FirstOrDefault();
        }

        /// <summary>
        /// Returns all entities for the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// This is going to be REALLY expensive with multiple providers.
        /// </remarks>
        public override IEnumerable<T> PerformGetAll<T>()
        {
            return Helper.PerformGetAll<T>();
        }

        public override bool CanReadRelations
        {
            get { return false; }
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            throw new NotImplementedException();
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            throw new NotImplementedException();
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            //loop through the providers 
            return MembershipProviders
                .Select(m => m.GetUser(id.Value.Value, false))
                .Any(found => found != null);
        }

        protected override void PerformAddOrUpdate(TypedEntity entity)
        {
            ValidateEntityForMembership(entity);

            //loop through the providers 
            foreach (var m in MembershipProviders)
            {
                if (!entity.Id.IsNullValueOrEmpty())
                {
                    var found = m.GetUser(entity.Id.Value.Value, false);
                    if (found != null)
                    {
                        //its an existing uer
                        FrameworkContext.TypeMappers.Map(entity, found);
                        m.UpdateUser(found);
                        continue;
                    }   
                }

                //new user
                MembershipCreateStatus status;
                var member = m.CreateUser(
                    entity.Attribute<string>("username".ToUmbracoAlias()),
                    entity.Attribute<string>("password".ToUmbracoAlias()),
                    entity.Attribute<string>("email".ToUmbracoAlias()),
                    entity.Attribute<string>("passwordQuestion".ToUmbracoAlias()),
                    entity.Attribute<string>("passwordAnswer".ToUmbracoAlias()),
                    entity.Attribute<bool>("isApproved".ToUmbracoAlias()),
                    entity.Id.Value.Value,
                    out status);
                if (status != MembershipCreateStatus.Success)
                {
                    throw new InvalidOperationException("Could not create a new membership user. Failed with status: " + status);
                }
                //re-map it
                FrameworkContext.TypeMappers.Map(member, (TypedEntity)entity);
            }
        }

        protected override void PerformDelete<T>(HiveId id)
        {
            //loop through the providers 
            foreach (var m in MembershipProviders)
            {
                var found = m.GetUser(id.Value.Value, false);
                if (found != null)
                {
                    //todo: should we make the delete all related data a config option ?
                    m.DeleteUser(found.UserName, true);
                }
            }
        }

        public override bool CanWriteRelations
        {
            get { return false; }
        }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            throw new NotImplementedException();
        }

        protected override void PerformRemoveRelation(IRelationById item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// We must ensure this entity has all required attributes
        /// </summary>
        /// <param name="entity"></param>
        private void ValidateEntityForMembership(TypedEntity entity)
        {
            var required = new[]
                {
                    "userName", 
                    "password",
                    "email",
                    "passwordQuestion",
                    "passwordAnswer",
                    "isApproved"
                };
            if (required.Any(r => !entity.Attributes.ContainsKey(r)))
            {
                throw new InvalidOperationException("The TypedEntity object does not contain the required attributes to be used to add or update a member");
            }
        }
    }
}