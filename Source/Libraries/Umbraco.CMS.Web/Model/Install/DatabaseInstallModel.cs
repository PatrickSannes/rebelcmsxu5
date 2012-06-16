using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Model.Install
{
    /// <summary>
    /// Represents the model to install the database during installation
    /// </summary>
    public class DatabaseInstallModel : IValidatableObject
    {
        
        public DatabaseServerType DatabaseType { get; set; }

        /// <summary>
        /// Used when specifying a custom connection string
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Used when specifying a custom connection string (my sql, ms sql, etc...)
        /// </summary>
        public string ProviderName { get; set; }

        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }



        /// <summary>
        /// Performs the validation based on options selected
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {            
            switch (DatabaseType)
            {
                case DatabaseServerType.MSSQL:
                case DatabaseServerType.MySQL:
                    if (string.IsNullOrEmpty(Server)) yield return new ValidationResult("Server is required", new [] { "Server" });
                    if (string.IsNullOrEmpty(DatabaseName)) yield return new ValidationResult("DatabaseName is required", new[] { "DatabaseName" });
                    if (string.IsNullOrEmpty(Username)) yield return new ValidationResult("Username is required", new[] { "Username" });
                    if (string.IsNullOrEmpty(Password)) yield return new ValidationResult("Password is required", new[] { "Password" });
                    break;
                
                    break;
                case DatabaseServerType.Custom:
                    if (string.IsNullOrEmpty(ConnectionString)) yield return new ValidationResult("Connection string is required", new[] { "ConnectionString" });
                    //if (string.IsNullOrEmpty(ProviderName)) yield return new ValidationResult("Provider name is required", new[] { "ProviderName" });
                    break;
                case DatabaseServerType.SQLCE:
                default:
                    break;
            }
        }
    }
}
