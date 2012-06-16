using NuGet;

namespace Umbraco.Cms.Web.Packaging
{
    public interface IPackageContext
    {
        IPackageManager LocalPackageManager { get; }
        IPackageManager PublicPackageManager { get; }
        IPackagePathResolver LocalPathResolver { get; }
    }
}