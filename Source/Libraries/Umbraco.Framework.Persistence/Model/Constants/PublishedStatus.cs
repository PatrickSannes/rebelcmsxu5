using Umbraco.Foundation;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    public class PublishedStatus : RevisionStatusType
    {
        public const string AliasValue = "published";

        public PublishedStatus() : base(AliasValue)
        {
            Id = 1;
            Alias = AliasValue;
            Name = "Published";
            IsSystem = true;
        }

        public static PublishedStatus Default { get { return new PublishedStatus(); } }
    }
}