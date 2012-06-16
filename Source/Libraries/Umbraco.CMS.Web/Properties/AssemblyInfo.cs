using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.UI;
using Umbraco.Cms.Web;
using Umbraco.Framework.Localization.Configuration;

[assembly: AssemblyTitle("Umbraco.Cms.Web")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyContainsPlugins]

[assembly: InternalsVisibleTo("Umbraco.Tests.DomainDesign")]
[assembly: InternalsVisibleTo("Umbraco.Cms.Web")]
[assembly: InternalsVisibleTo("Umbraco.Cms.Web.UI")]
[assembly: InternalsVisibleTo("Umbraco.Tests.Cms")]
[assembly: InternalsVisibleTo("Umbraco.Cms.Web.Editors")]
[assembly: InternalsVisibleTo("Umbraco.Cms.Web.Trees")]
[assembly: InternalsVisibleTo("Umbraco.Tests.Extensions")]
[assembly: InternalsVisibleTo("Umbraco.Cms.Web.PropertyEditors")]
[assembly: InternalsVisibleTo("Umbraco.Cms.Packages.DevDataset")]
[assembly: InternalsVisibleTo("Umbraco.Cms.Web.Tasks")]
[assembly: InternalsVisibleTo("Umbraco.Tests.Cms.DomainIntegration")]

[assembly: LocalizationXmlSource("Localization.Default.xml")]

[assembly: WebResource("Umbraco.Cms.Web.EmbeddedViews.Views.Resources.Site.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Umbraco.Cms.Web.EmbeddedViews.Views.Resources.umbraco-logo.png", "image/png")]

[assembly: InternalsVisibleTo("Umbraco.Tests.CoreAndFramework")]