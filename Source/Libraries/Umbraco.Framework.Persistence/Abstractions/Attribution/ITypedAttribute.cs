using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution
{
		/// <summary>
		/// Represents a strongly-typed attribute value
		/// </summary>
		public interface ITypedAttribute : IPersistenceEntity
		{
				// TODO: 2011 01 08: APN - ITypedAttribute:
				// Originally ITypedAttribute was an IEntityVertex but this has been removed pending removal of
				// circular reference between flat persistence and graphable persistence types

				/// <summary>
				/// Gets or sets the type of the attribute.
				/// </summary>
				/// <value>The type of the attribute.</value>
				IAttributeTypeDefinition AttributeType { get; set; }

				/// <summary>
				/// Gets or sets the value of the attribute.
				/// </summary>
				/// <value>The value.</value>
				dynamic Value { get; set; }
		}
}