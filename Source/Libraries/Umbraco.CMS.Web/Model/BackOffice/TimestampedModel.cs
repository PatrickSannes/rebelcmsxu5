using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// A basic class representing a model with create/updated dates
    /// </summary>
    [Bind(Exclude = "UtcCreated,UtcModified")]
    public abstract class TimestampedModel : IdentityModel
    {
        [ReadOnly(true)]
        [Display(Name = "CoreEntityModel.UtcCreated")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTimeOffset UtcCreated { get; set; }

        [Display(Name = "Date Updated")] //This will be the default if the text is not localized (it is)
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTimeOffset UtcModified { get; set; }
    }
}