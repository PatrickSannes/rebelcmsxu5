using System;
using System.IO;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model
{
    //TODO: Pretty sure this clas is obsolete and not really required. SD.

    /// <summary>
    /// Represents a template object
    /// </summary>
    /// <remarks>
    /// This does NOT represent a Template editor model
    /// </remarks>
    public class TemplateFile : TimestampedModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// The Id is a string serialized HiveId and contains the file name of the template
        /// </remarks>
        public TemplateFile(HiveId id)
        {
            Id = id;
            if (id.Value.Type != HiveIdValueTypes.String)
            {
                throw new NotSupportedException(
                    "The HiveId for a template must be a string with the value of the template file name and extension");
            }
        }

        public string PathWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension((string)Id.Value); }
        }
    }
}
