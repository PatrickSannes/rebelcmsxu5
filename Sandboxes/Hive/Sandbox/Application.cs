using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using Sandbox.Hive.BootStrappers.AutoFac;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.Hive.Foundation;
using Sandbox.Hive.Foundation.Configuration;

namespace Sandbox
{
  public static class Application
  {
    public static void Start()
    {
      AutoFacResolver.InitialiseFoundation();


    }

    static void Configure()
    {
      //TODO: Implement a helper Context class for easy access to things like the below
      var configurationSection = DependencyResolver.Current.Resolve<IFoundationConfigurationSection>();

      //foreach (PersistenceProviderElement persistenceProviderElement in configurationSection.PersistenceProviders)
      //{
      //  foreach (PersistenceReadWriterElement readWriterElement in persistenceProviderElement.ReadWriters)
      //  {
      //    string[] typeParts = readWriterElement.Type.Split(new[] { ',' });
      //    string typeName = typeParts[0];
      //    Type type = LoadType(typeName);

      //    if (type.IsAssignableFrom(typeof(IPersistenceRepository)))
      //    {
      //      builder.RegisterType(type).Named<IPersistenceRepository>(string.Format("{0}/{1}",
      //                                                                             persistenceProviderElement
      //                                                                               .Key,
      //                                                                             readWriterElement.Key));
      //    }
      //    else
      //    {
      //      //TODO: Implement full exception
      //      throw new Exception(string.Format("Type {0} does not implement {1}", readWriterElement.Type,
      //                                        typeof(IPersistenceRepository).FullName));
      //    }
      //  }
      //}
    }

    /// <summary>
    /// Loads the type.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns></returns>
    static Type LoadType(string typeName)
    {
      Contract.Assert(!string.IsNullOrEmpty(typeName));

      Type type = Type.GetType(typeName);

      if (type == null)
        throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                                                             "'{0}' type could not be loaded", typeName));

      return type;
    }
  }
}
