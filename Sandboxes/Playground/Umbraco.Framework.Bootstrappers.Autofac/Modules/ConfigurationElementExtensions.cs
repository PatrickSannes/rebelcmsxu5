using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Umbraco.Foundation.Configuration;

namespace Umbraco.Framework.Bootstrappers.Autofac.Modules
{
  public static class ConfigurationElementExtensions
  {
    /// <summary>
    /// Converts the <see cref="PersistenceReadWriterElement"/> in a <see cref="PersistenceReadWriterElementCollection"/>
    /// to a <see cref="ResolvedParameter"/>, meaning persistence repository types referenced in configuration
    /// by name can be injected into the constructor of the parent provider.
    /// </summary>
    /// <remarks><see cref="ResolvedParameter"/> types allow for the deferred resolution of a type by using
    /// an <see cref="IComponentContext"/> to queue calls to resolve the instances, which would otherwise be
    /// tricky to do via chaining modules together</remarks>
    /// <returns>The parameters represented by this collection.</returns>
    public static IEnumerable<Parameter> ToParameters(this PersistenceReadWriterElementCollection collection)
    {
      foreach (PersistenceReadWriterElement parameter in collection)
      {
        PersistenceReadWriterElement localParameter = parameter;

        yield return new ResolvedParameter(
          (p, c) =>
          (p.ParameterType == localParameter.ToType() || p.Name == "readWriter" ||
           localParameter.ToType().IsAssignableFrom(p.ParameterType)),
          (p, c) => ResolveNamedPersistenceRepositoryProvider(c, localParameter));
      }
    }

    /// <summary>
    /// Converts a <see cref="PersistenceReaderElement"/> to a <see cref="ResolvedParameter"/>, meaning persistence repository 
    /// types referenced in configuration by name can be injected into the constructor of the parent provider.
    /// </summary>
    /// <remarks><see cref="ResolvedParameter"/> types allow for the deferred resolution of a type by using
    /// an <see cref="IComponentContext"/> to queue calls to resolve the instances, which would otherwise be
    /// tricky to do via chaining modules together</remarks>
    /// <returns>The parameter.</returns>
    public static ResolvedParameter ToParameter(this PersistenceReaderElement element)
    {
      PersistenceReaderElement localElement = element;

      return new ResolvedParameter(
        (p, c) =>
        (p.ParameterType == localElement.ToType() || p.Name == "reader" ||
         localElement.ToType().IsAssignableFrom(p.ParameterType)),
        (p, c) => ResolveNamedPersistenceRepositoryProvider(c, localElement)
        );
    }


  	/// <summary>
    /// Resolves the named persistence read-writer using an <see cref="IComponentContext"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="providerElement">The provider element.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    private static object ResolveNamedPersistenceRepositoryProvider(IComponentContext context,
                                                                    PersistenceReadWriterElement providerElement)
    {
      return context.ResolveNamed(providerElement.InternalKey, providerElement.ToType());
    }

    /// <summary>
    /// Resolves the named persistence reader using an <see cref="IComponentContext"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="providerElement">The provider element.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    private static object ResolveNamedPersistenceRepositoryProvider(IComponentContext context, PersistenceReaderElement providerElement)
    {
      return context.ResolveNamed(providerElement.InternalKey, providerElement.ToType());
    }
  }
}