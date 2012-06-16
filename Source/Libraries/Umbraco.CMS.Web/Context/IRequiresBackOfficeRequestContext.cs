namespace Umbraco.Cms.Web.Context
{
    /// <summary>
    /// Interface requiring a BackOfficeRequestContext
    /// </summary>
    public interface IRequiresBackOfficeRequestContext
    {
        IBackOfficeRequestContext BackOfficeRequestContext { get; set; }
    }
}