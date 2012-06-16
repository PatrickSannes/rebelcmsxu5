using Umbraco.Cms.Web.System;

using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;

namespace Umbraco.Cms.Web.DependencyManagement
{
    /// <summary>
    /// Used to register all Umbraco system components which can then be queried through this class
    /// </summary>
    public interface IComponentRegistrar
    {

        /// <summary>
        /// Registers the macro engines.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        void RegisterMacroEngines(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers the dashboard filters.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        void RegisterDashboardFilters(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers the dashboard match rules.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        void RegisterDashboardMatchRules(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Register the tree controllers
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        /// <remarks>
        /// This must register all tree controllers as their native types and also register them all as the type TreeController
        /// </remarks>
        void RegisterTreeControllers(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers the Surface controllers
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        void RegisterSurfaceControllers(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers all menu items
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        void RegisterMenuItems(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Register the editor controllers
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        void RegisterEditorControllers(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers all UmbracoPropertyEditors
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        /// <remarks>
        /// All Property Editors should be registered as the type PropertyEditor
        /// </remarks>
        void RegisterPropertyEditors(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers all UmbracoParameterEditors
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="typeFinder"></param>
        /// <remarks>
        /// All Parameter Editors should be registered as the type ParameterEditor
        /// </remarks>
        void RegisterParameterEditors(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers all tasks.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        /// <remarks></remarks>
        void RegisterTasks(IContainerBuilder builder, TypeFinder typeFinder);

        /// <summary>
        /// Registers the permissions.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        void RegisterPermissions(IContainerBuilder builder, TypeFinder typeFinder);
    }
}