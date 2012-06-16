using System;
using System.Web.Mvc;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

namespace Umbraco.Cms.Web.DependencyManagement
{
    /// <summary>
    /// Uses the DependencyResolver to retrieve the IAttributeTypeRegistry from IoC.
    /// </summary>
    public class DependencyResolverAttributeTypeRegistry : IAttributeTypeRegistry
    {
        private readonly IAttributeTypeRegistry _internalRegistry;

        public DependencyResolverAttributeTypeRegistry()
        {
            var dependencyResolver = DependencyResolver.Current;
            Mandate.That(dependencyResolver != null, x => new InvalidOperationException("DependencyResolver.Current returned null, ensure that IoC is setup"));
            _internalRegistry = dependencyResolver.GetService<IAttributeTypeRegistry>();
            Mandate.That(_internalRegistry != null, x => new InvalidOperationException("Could not resolve an IAttributeTypeRegistry from the DependencyResolver, ensure that one is registered in IoC"));
        }

        /// <summary>
        /// Gets the AttributeType by alias
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public AttributeType GetAttributeType(string alias)
        {
            return _internalRegistry.GetAttributeType(alias);
        }

        public void RegisterAttributeType(Func<AttributeType> attributeType)
        {
            _internalRegistry.RegisterAttributeType(attributeType);
        }

        /// <summary>
        /// Tries to get an <see cref="AttributeType" /> by alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public AttemptTuple<AttributeType> TryGetAttributeType(string alias)
        {
            return _internalRegistry.TryGetAttributeType(alias);
        }
    }
}