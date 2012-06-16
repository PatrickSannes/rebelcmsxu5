namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// Defines a package found in the plugins packages folder
    /// </summary>
    public class PackageFolder
    {
        /// <summary>
        /// The name of the package
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Returns true if the package has been installed by Nuget, this is determined if there is a nupkg file in existence there
        /// </summary>
        public bool IsNugetInstalled { get; set; }

        ///// <summary>
        ///// Returns true if the package contains assemblies in the 'lib' folder
        ///// </summary>
        //public bool HasAssemblies { get; set; }
    }
}