using Umbraco.Framework.Configuration;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Context
{
    using System;

    /// <summary>
    /// An aggregatation of common Framework services and components.
    /// </summary>
    /// <remarks></remarks>
    public interface IFrameworkContext : IDisposable
    {
        /// <summary>
        /// Gets the task manager
        /// </summary>
        ApplicationTaskManager TaskManager { get;  }

        /// <summary>
        /// Gets or sets the current language used for localization.
        /// </summary>
        /// <value>
        /// The current language.
        /// </value>
        LanguageInfo CurrentLanguage { get; set; }

        /// <summary>
        /// Gets the text manager to be used for localization
        /// </summary>
        TextManager TextManager {get;}

        /// <summary>
        /// Gets a collection of registered type mappers.
        /// </summary>
        /// <remarks></remarks>
        MappingEngineCollection TypeMappers { get; }

        /// <summary>
        /// Gets a scoped finalizer queue.
        /// </summary>
        /// <remarks></remarks>
        AbstractFinalizer ScopedFinalizer { get; }

        /// <summary>
        /// Gets a scoped cache bag
        /// </summary>
        AbstractScopedCache ScopedCache { get;  }

        /// <summary>
        /// Gets the application cache bag
        /// </summary>
        AbstractApplicationCache ApplicationCache { get; }
    }
}