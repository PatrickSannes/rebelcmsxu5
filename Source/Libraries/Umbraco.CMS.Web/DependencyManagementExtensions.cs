using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Umbraco.Cms.Web.DependencyManagement;

using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// Extension methods for IContainerBuilder
    /// </summary>
    public static class DependencyManagementExtensions
    {
        /// <summary>
        /// Register all model binders found in the specified Assemblies which are registered to types based
        /// on the ModelBinderForAttribute.
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="assemblies"></param>
        /// <param name="typeFinder"></param>
        /// <returns></returns>
        public static IContainerBuilder RegisterModelBinders(this IContainerBuilder containerBuilder,
            IEnumerable<Assembly> assemblies,
            TypeFinder typeFinder)
        {
            foreach (var type in typeFinder.FindClassesOfType<IModelBinder>(assemblies))
            {
                var register = containerBuilder.For(type)
                    .KnownAs<IModelBinder>();

                foreach (ModelBinderForAttribute item in type.GetCustomAttributes(typeof(ModelBinderForAttribute), true))
                {
                    register.WithMetadata<ModelBinderMetadata, Type>(prop => prop.BinderType, item.TargetType);
                    ModelBinders.Binders.Add(item.TargetType, new ModelBinderAdapter(item.TargetType));
                }
            }
            return containerBuilder;
        }

        /// <summary>
        /// Register all model binders found in the specified Assembly which are registered to types based
        /// on the ModelBinderForAttribute.
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="assembly"></param>
        /// <param name="typeFinder"></param>
        /// <returns></returns>
         public static IContainerBuilder RegisterModelBinders(this IContainerBuilder containerBuilder,
            Assembly assembly,
            TypeFinder typeFinder)
         {
             return containerBuilder.RegisterModelBinders(new[] { assembly }, typeFinder);
         }

        /// <summary>
        /// Register all controllers found in the specified assemblies
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="assemblies"></param>
        /// <param name="typeFinder"></param>
        /// <returns></returns>
        public static IContainerBuilder RegisterControllers(this IContainerBuilder containerBuilder,
            IEnumerable<Assembly> assemblies,
            TypeFinder typeFinder)
        {
            //TODO: Include extenders!
            foreach (var type in typeFinder.FindClassesOfType<IController>(assemblies)
                .Where(t => t.Name.EndsWith("Controller")))
            {
                containerBuilder.For(type).KnownAsSelf();
            }
            return containerBuilder;
        }

        /// <summary>
        /// Register all controllers found in the specified Assembly
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="assembly"></param>
        /// <param name="typeFinder"></param>
        /// <returns></returns>
        public static IContainerBuilder RegisterControllers(this IContainerBuilder containerBuilder,
            Assembly assembly,
            TypeFinder typeFinder)
        {
            return containerBuilder.RegisterControllers(new[] { assembly }, typeFinder);
        }

        public static IContainerBuilder RegisterModelBinderProvider(this IContainerBuilder containerBuilder)
        {
            containerBuilder.For<ModelBinderProvider>()
                .KnownAs<IModelBinderProvider>()
                .ScopedAs.Singleton();
            return containerBuilder;
        }
    }
}
