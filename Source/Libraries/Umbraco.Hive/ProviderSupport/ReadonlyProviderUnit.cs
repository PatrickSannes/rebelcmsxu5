using System;
using Umbraco.Framework;
using Umbraco.Framework.Context;

namespace Umbraco.Hive.ProviderSupport
{
    public class ReadonlyProviderUnit : DisposableObject, IReadonlyProviderUnit
    {
        public ReadonlyProviderUnit(AbstractReadonlyEntityRepository entityRepository, AbstractScopedCache unitScopedCache)
        {
            EntityRepository = entityRepository;
            FrameworkContext = entityRepository.FrameworkContext;
            UnitScopedCache = unitScopedCache;
        }

        /// <summary>
        /// Gets or sets the entity repository.
        /// </summary>
        /// <value>The entity repository.</value>
        public AbstractReadonlyEntityRepository EntityRepository { get; protected set; }

        protected override void DisposeResources()
        {
            EntityRepository.Dispose();
        }

        /// <summary>
        /// Completes this unit of work.
        /// </summary>
        public void Complete()
        {
            return;
        }

        /// <summary>
        /// Abandons this unit of work and its changes.
        /// </summary>
        public void Abandon()
        {
            return;
        }

        /// <summary>
        /// Gets the unit-scoped cache.
        /// </summary>
        /// <value>The unit scoped cache.</value>
        public AbstractScopedCache UnitScopedCache { get; private set; }

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; private set; }

        #endregion
    }
}