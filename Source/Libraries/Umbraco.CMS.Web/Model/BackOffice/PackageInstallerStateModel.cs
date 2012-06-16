namespace Umbraco.Cms.Web.Model.BackOffice
{
    public class PackageInstallerStateModel
    {
        public string PackageId { get; set; }        
        public PackageInstallationState State { get; set; }
    }
}