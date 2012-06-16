using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;

namespace Umbraco.Framework.Configuration
{
    public static class ConfigurationHelper
	{
		/// <summary>
		/// Gets the type from the type string.
		/// </summary>
		/// <param name="typeConfigName">Name of the type.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static Type GetTypeFromTypeConfigName(string typeConfigName)
		{
			string[] typeParts = typeConfigName.Split(new[] {','});
			string typeName = typeParts[0];
			string assemblyname = typeParts[1];
			return LoadType(typeName, assemblyname);
		}

		/// <summary>
		/// Loads a type either from the existing AppDomain or from the supplied assembly name.
		/// </summary>
		/// <param name="typeName">Name of the type.</param>
		/// <returns></returns>
		public static Type LoadType(string typeName, string assemblyName)
		{
            Mandate.ParameterNotNullOrEmpty(typeName, "typeName");

			Type type = Type.GetType(typeName);

			if (type == null)
			{
				Assembly assembly = Assembly.Load(assemblyName);
				type = assembly.GetType(typeName, false);
			}

			if (type == null)
				throw new ConfigurationErrorsException(String.Format(CultureInfo.CurrentCulture,
				                                                     "'{0}' type could not be loaded", typeName));

			return type;
		}
	}
}
