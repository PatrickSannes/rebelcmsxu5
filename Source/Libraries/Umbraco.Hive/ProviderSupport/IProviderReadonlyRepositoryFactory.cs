using System;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public interface IProviderReadonlyRepositoryFactory<out T> 
        : IDisposable
        where T : AbstractProviderRepository
    {
        /// <summary>
        /// Gets the provider metadata.
        /// </summary>
        /// <value>The provider metadata.</value>
        ProviderMetadata ProviderMetadata { get; }

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        IFrameworkContext FrameworkContext { get; }

        /// <summary>
        /// Gets an <see cref="AbstractProviderRepository"/> of type <see cref="T"/>. It will have only read operations.
        /// </summary>
        /// <returns></returns>
        T GetReadonlyRepository();
    }
}