using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Framework.Configuration;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Context
{
    /// <summary>
    /// The default implementation of <see cref="IFrameworkContext"/>
    /// </summary>
    /// <remarks></remarks>
    public class DefaultFrameworkContext : DisposableObject, IFrameworkContext
    {
        protected DefaultFrameworkContext(AbstractScopedCache scopedCache,
            AbstractApplicationCache applicationCache,
            AbstractFinalizer finalizer)
        {
            ScopedCache = scopedCache;
            ApplicationCache = applicationCache;
            ScopedFinalizer = finalizer;            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFrameworkContext"/> class.
        /// </summary>
        /// <param name="textManager">The text manager.</param>
        /// <param name="typeMappers">The type mappers.</param>
        /// <param name="scopedCache">The scoped cache.</param>
        /// <param name="applicationCache">The application cache</param>
        /// <param name="finalizer"></param>
        /// <param name="taskMgr"></param>
        /// <remarks></remarks>
        public DefaultFrameworkContext(
            TextManager textManager, 
            MappingEngineCollection typeMappers, 
            AbstractScopedCache scopedCache,
            AbstractApplicationCache applicationCache,
            AbstractFinalizer finalizer,
            ApplicationTaskManager taskMgr)
            : this(scopedCache, applicationCache, finalizer)
        {
            TextManager = textManager;
            TypeMappers = typeMappers;
            TaskManager = taskMgr;
        }

        private LanguageInfo _currentLanguage;

        /// <summary>
        /// Gets the task manager
        /// </summary>
        public ApplicationTaskManager TaskManager { get; private set; }

        /// <summary>
        /// Gets or sets the current language used for localization.
        /// </summary>
        /// <value>The current language.</value>
        /// <remarks></remarks>
        public LanguageInfo CurrentLanguage
        {
            get { return _currentLanguage; }
            set
            {                
                if (value == null)
                {
                    throw new ArgumentNullException(
                        TextManager.Get<DefaultFrameworkContext>("FrameworkContext.CurrentLanguage.NullException"));
                }
                _currentLanguage = value;
            }
        }

        /// <summary>
        /// Gets the text manager to be used for localization
        /// </summary>
        /// <value>The text manager.</value>
        /// <remarks></remarks>
        public TextManager TextManager { get; protected set; }

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

        /// <summary>
        /// Gets the scoped cache bag
        /// </summary>
        public AbstractScopedCache ScopedCache { get; protected set; }

        /// <summary>
        /// Gets the application cache bag
        /// </summary>
        public AbstractApplicationCache ApplicationCache { get; protected set; }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            ScopedFinalizer.IfNotNull(x =>
                {
                    x.FinalizeScope();
                    x.Dispose();
                });
            ScopedCache.IfNotNull(x =>
                {
                    x.ScopeComplete();
                    x.Dispose();
                });
        }

        #endregion
    }
}
