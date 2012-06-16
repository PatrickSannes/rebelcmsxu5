using System;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.DataManagement;

namespace Umbraco.Framework
{
	[Obsolete]
    public interface IMappedIdentifier : IEquatable<IMappedIdentifier>, IEquatable<string>
    {
        /// <summary>
        ///   Gets or sets the value as string.
        /// </summary>
        /// <value>The value as string.</value>
        string ValueAsString { get; }

        /// <summary>
        ///   Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        dynamic Value { get; set; }

        /// <summary>
        ///   Gets the entity provider key.
        /// </summary>
        /// <value>The entity provider key.</value>
        string MappingKey { get; }

        /// <summary>
        ///   Gets the provider responsible for the object with this Id.
        /// </summary>
        /// <value>The mapped provider.</value>
        IProviderManifest MappedProvider { get; set; }

        /// <summary>
        ///   Gets the type of data serialization.
        /// </summary>
        /// <value>The type of the serialization.</value>
        DataSerializationTypes DataSerializationType { get; }

        //T NewValue<T>() where T : IMappedIdentifier, new();
    }
}