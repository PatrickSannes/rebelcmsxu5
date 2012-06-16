using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Localization.Configuration;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Tests.Extensions
{
    /// <summary>
    /// An implementation of <see cref="IFrameworkContext"/> used for unit testing
    /// </summary>
    /// <remarks></remarks>
    public class FakeFrameworkContext : DisposableObject, IFrameworkContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public FakeFrameworkContext()
        {
            var fakeMapperList = new List<Lazy<AbstractMappingEngine, TypeMapperMetadata>>();
            TypeMappers = new MappingEngineCollection(fakeMapperList);
            CurrentLanguage = Thread.CurrentThread.CurrentCulture;
            TextManager = LocalizationConfig.SetupDefault();
            ScopedFinalizer = new NestedLifetimeFinalizer();
            TaskManager = new ApplicationTaskManager(Enumerable.Empty<Lazy<AbstractTask, TaskMetadata>>());
            ApplicationCache = new HttpRuntimeApplicationCache();
            ScopedCache = new ThreadStaticScopedCache();
        }

        public FakeFrameworkContext(MappingEngineCollection typeMappers) : this()
        {
            TypeMappers = typeMappers;
        }

        public void SetTypeMappers(MappingEngineCollection coll)
        {
            foreach (var binder in coll.Binders)
            {
                TypeMappers.Add(binder);
            }
        }

        #region Implementation of IFrameworkContext

        public ApplicationTaskManager TaskManager { get; private set; }

        /// <summary>
        /// Gets or sets the current language used for localization.
        /// </summary>
        /// <value>
        /// The current language.
        /// </value>
        public LanguageInfo CurrentLanguage { get; set; }

        /// <summary>
        /// Gets the text manager to be used for localization
        /// </summary>
        public TextManager TextManager { get; private set; }

        /// <summary>
        /// Gets a collection of registered type mappers.
        /// </summary>
        /// <remarks></remarks>
        public MappingEngineCollection TypeMappers { get; protected set; }

        /// <summary>
        /// Gets a scoped finalizer queue.
        /// </summary>
        /// <remarks></remarks>
        public AbstractFinalizer ScopedFinalizer { get; protected set; }

        public AbstractScopedCache ScopedCache { get; private set; }

        public AbstractApplicationCache ApplicationCache { get; private set; }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            return;
        }

        #endregion
    }
}