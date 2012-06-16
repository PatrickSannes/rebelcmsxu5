namespace Umbraco.Cms.Web.Context
{
    /// <summary>
    /// Interface requiring an UmbracoHelper
    /// </summary>
    public interface IRequiresUmbracoHelper
    {
        UmbracoHelper Umbraco { get; set; }
    }
}
