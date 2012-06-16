using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.PropertyEditors.UrlPicker
{
    /// <summary>
    /// Determines in which serialized format the the data is saved to the database
    /// </summary>
    public enum UrlPickerDataFormat
    {
        /// <summary>
        /// Store as XML
        /// </summary>
        XML,
        /// <summary>
        /// Store as comma delimited (CSV, single line)
        /// </summary>
        CSV,
        /// <summary>
        /// Store as a JSON object, which can be deserialized by .NET or JavaScript
        /// </summary>
        JSON
    }
}
