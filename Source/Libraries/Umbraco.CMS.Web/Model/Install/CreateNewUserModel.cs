using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace Umbraco.Cms.Web.Model.Install
{
    public class CreateNewUserModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [EqualTo("Password")]
        public string ConfirmPassword { get; set; }

        public bool SignUpForNewsletter { get; set; }
    }
}