using System;
using System.Collections.Generic;
using Umbraco.Cms.Web.Dashboards;
using Umbraco.Cms.Web.Dashboards.Filters;
using Umbraco.Cms.Web.Dashboards.Rules;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Surface;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework.Security;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.DependencyManagement
{

    /// <summary>
    /// An object containing all of the meta data for all components/plugins registered
    /// </summary>
    public class ComponentRegistrations
    {
        public ComponentRegistrations(
            IEnumerable<Lazy<MenuItem, MenuItemMetadata>> menuItems,
            IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> editorControllers,
            IEnumerable<Lazy<TreeController, TreeMetadata>> treeControllers,
            IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> surfaceControllers,
            IEnumerable<Lazy<AbstractTask, TaskMetadata>> tasks,
            IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>> propertyEditors,
            IEnumerable<Lazy<AbstractParameterEditor, ParameterEditorMetadata>> parameterEditors,
            IEnumerable<Lazy<DashboardMatchRule, DashboardRuleMetadata>> dashboardMatchRules,
            IEnumerable<Lazy<DashboardFilter, DashboardRuleMetadata>> dashboardFilters,
            IEnumerable<Lazy<Permission, PermissionMetadata>> permissions,
            IEnumerable<Lazy<AbstractMacroEngine, MacroEngineMetadata>> macroEngines)
        {
            TreeControllers = treeControllers;
            MenuItems = menuItems;
            EditorControllers = editorControllers;
            SurfaceControllers = surfaceControllers;
            Tasks = tasks;
            PropertyEditors = propertyEditors;
            ParameterEditors = parameterEditors;
            DashboardMatchRules = dashboardMatchRules;
            DashboardFilters = dashboardFilters;
            Permissions = permissions;
            MacroEngines = macroEngines;
        }

        /// <summary>
        /// Returns all registered property editors
        /// </summary>
        public IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>> PropertyEditors { get; private set; }

        /// <summary>
        /// Returns all registered parameter editors
        /// </summary>
        public IEnumerable<Lazy<AbstractParameterEditor, ParameterEditorMetadata>> ParameterEditors { get; private set; }

        /// <summary>
        /// Gets the dashboard filters.
        /// </summary>
        public IEnumerable<Lazy<DashboardFilter, DashboardRuleMetadata>> DashboardFilters { get; private set; }

        /// <summary>
        /// Returns all registered dashboard match rules
        /// </summary>
        public IEnumerable<Lazy<DashboardMatchRule, DashboardRuleMetadata>> DashboardMatchRules { get; private set; }

        /// <summary>
        /// Returns all the registered Tasks
        /// </summary>
        public IEnumerable<Lazy<AbstractTask, TaskMetadata>> Tasks { get; private set; }

        /// <summary>
        /// Returns all the registered Surface controllers
        /// </summary>
        public IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> SurfaceControllers { get; private set; }

        /// <summary>
        /// Returns all registered menu items
        /// </summary>
        public IEnumerable<Lazy<MenuItem, MenuItemMetadata>> MenuItems { get; private set; }

        /// <summary>
        /// Returns all registered editor controllers
        /// </summary>
        public IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> EditorControllers { get; private set; }

        /// <summary>
        /// Returns all registered tree controllers
        /// </summary>
        public IEnumerable<Lazy<TreeController, TreeMetadata>> TreeControllers { get; private set; }

        /// <summary>
        /// Gets all registered permissions
        /// </summary>
        public IEnumerable<Lazy<Permission, PermissionMetadata>> Permissions { get; private set; }

        /// <summary>
        /// Gets all registered MacroEngines
        /// </summary>
        public IEnumerable<Lazy<AbstractMacroEngine, MacroEngineMetadata>> MacroEngines { get; private set; }
    }
}