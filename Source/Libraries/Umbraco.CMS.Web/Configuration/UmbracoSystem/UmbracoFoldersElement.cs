using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.UmbracoSystem
{
    public class UmbracoFoldersElement : ConfigurationElement
    {

        /// <summary>
        /// Gets or sets the local package repository folder
        /// </summary>
        /// <value>
        /// The local package repository folder.
        /// </value>
        [ConfigurationProperty("localPackageRepositoryFolder", IsRequired = true)]
        public string LocalPackageRepositoryFolder
        {
            get { return this["localPackageRepositoryFolder"].ToString(); }
            set
            {
                this["localPackageRepositoryFolder"] = value;
            }
        }

        /// <summary>
        /// The folder that stores the template files
        /// </summary>
        [ConfigurationProperty("templateFolder", IsRequired = true)]
        public string TemplateFolder
        {
            get { return this["templateFolder"].ToString(); }
            set
            {
                this["templateFolder"] = value;
            }
        }

        [ConfigurationProperty("backOfficeFolder", IsRequired = true)]
        public string BackOfficeFolder
        {
            get { return this["backOfficeFolder"].ToString(); }
            set
            {
                this["backOfficeFolder"] = value;
            }
        }

        [ConfigurationProperty("docTypeIconFolder", IsRequired = true)]
        public string DocTypeIconFolder
        {
            get { return this["docTypeIconFolder"].ToString(); }
            set
            {
                this["docTypeIconFolder"] = value;
            }
        }

        [ConfigurationProperty("appIconFolder", IsRequired = true)]
        public string ApplicationIconFolder
        {
            get { return this["appIconFolder"].ToString(); }
            set
            {
                this["appIconFolder"] = value;
            }
        }

        [ConfigurationProperty("docTypeThumbnailFolder", IsRequired = true)]
        public string DocTypeThumbnailFolder
        {
            get { return this["docTypeThumbnailFolder"].ToString(); }
            set
            {
                this["docTypeThumbnailFolder"] = value;
            }
        }

        [ConfigurationProperty("scriptsFolder", IsRequired = true)]
        public string ScriptsFolder
        {
            get { return this["scriptsFolder"].ToString(); }
            set
            {
                this["scriptsFolder"] = value;
            }
        }

        [ConfigurationProperty("stylesheetsFolder", IsRequired = true)]
        public string StylesheetsFolder
        {
            get { return this["stylesheetsFolder"].ToString(); }
            set
            {
                this["stylesheetsFolder"] = value;
            }
        }

    }
}