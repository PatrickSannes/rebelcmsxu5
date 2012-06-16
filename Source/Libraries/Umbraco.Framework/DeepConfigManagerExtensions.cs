using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;

using Umbraco.Framework.Configuration;

namespace Umbraco.Framework
{
    /// <summary>
    /// Extensions for <see cref="DeepConfigManager"/>.
    /// </summary>
    public static class DeepConfigManagerExtensions
    {
        /// <summary>
        /// Gets the first configuration setting matching <paramref name="sectionName"/> by calling <see cref="DeepConfigManager.GetWebSettings{TSection,TOut}(string,Func{TSection,TOut},string,string,string)"/>.
        /// See that method for an example. The relative plugin base path in <see cref="PluginManagerConfiguration.PluginsPath"/> is mapped to a physical path using the <see cref="System.Web.HttpContextWrapper"/> supplied to this instance
        /// <paramref name="manager"/> in its constructor.
        /// </summary>
        /// <typeparam name="TSection">The type of the section.</typeparam>
        /// <typeparam name="TOut">The type outputted by the expression in <paramref name="deferred"/>.</typeparam>
        /// <param name="manager">The manager.</param>
        /// <param name="sectionName">Name of the section in the configuration files.</param>
        /// <param name="deferred">The deffered expression.</param>
        /// <returns></returns>
        public static TOut GetFirstPluginSetting<TSection, TOut>(this DeepConfigManager manager, string sectionName, Expression<Func<TSection, TOut>> deferred)
            where TSection : ConfigurationSection
            where TOut : class
        {
            var settings = PluginManagerConfiguration.GetSettings();
            return manager.GetFirstWebSetting(sectionName, deferred, settings.PluginsPath);
        }

        /// <summary>
        /// Gets the first plugin section.
        /// </summary>
        /// <typeparam name="TSection">The type of the section.</typeparam>
        /// <param name="manager">The manager.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <returns></returns>
        public static TSection GetFirstPluginSection<TSection>(this DeepConfigManager manager, string sectionName)
            where TSection : ConfigurationSection
        {
            var settings = PluginManagerConfiguration.GetSettings();
            return manager.GetFirstWebSetting<TSection, TSection>(sectionName, x => x, settings.PluginsPath);
        }

        /// <summary>
        /// Gets the plugin settings.
        /// </summary>
        /// <typeparam name="TSection">The type of the section.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="manager">The manager.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="deferred">The deferred.</param>
        /// <returns></returns>
        public static IEnumerable<TOut> GetPluginSettings<TSection, TOut>(this DeepConfigManager manager, string sectionName, Expression<Func<TSection, TOut>> deferred)
            where TSection : ConfigurationSection
            where TOut : class
        {
            var settings = PluginManagerConfiguration.GetSettings();
            var pluginBasePath = settings != null ? settings.PluginsPath : "~/Config/";
            return manager.GetWebSettings(sectionName, deferred, pluginBasePath);
        }

        /// <summary>
        /// Returns a collection of all connection string registered via deep config
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static IEnumerable<ConnectionStringSettings> GetConnectionStrings(this DeepConfigManager manager)
        {
            var settings = PluginManagerConfiguration.GetSettings();
            return manager.GetConnectionStrings(settings.PluginsPath);
        }
    }
}