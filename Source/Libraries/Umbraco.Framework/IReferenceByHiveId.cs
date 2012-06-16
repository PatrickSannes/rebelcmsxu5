using System.ComponentModel;

namespace Umbraco.Framework
{
    /// <summary>
    /// Stipulates that implementing types must have an <see cref="Id"/> property of type <see cref="HiveId"/>.
    /// </summary>
    public interface IReferenceByHiveId
    {
        /// <summary>
        ///   Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [TypeConverter(typeof(HiveIdTypeConverter))]
        HiveId Id { get; set; }
    }
}