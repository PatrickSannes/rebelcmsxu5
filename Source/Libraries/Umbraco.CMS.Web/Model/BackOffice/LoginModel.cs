using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    public enum LoginDisplayType
    {
        /// <summary>
        /// Displays the standard login page even if in the content frame
        /// </summary>
        StandardPage,

        /// <summary>
        /// Used when showing the login overlay to ensure the successful result page clears the overlay
        /// </summary>
        DisplayingOverlay,

        /// <summary>
        /// Used when an auth timeout occurs when selecting a tree node and the login overlay should be displayed, the 
        /// content frame will remain empty.
        /// </summary>
        ShowOverlay
    }

    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [HiddenInput(DisplayValue = false)]
        public LoginDisplayType DisplayType { get; set; }
    }
}