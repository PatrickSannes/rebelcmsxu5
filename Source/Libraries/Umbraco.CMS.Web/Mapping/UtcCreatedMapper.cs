using System;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    /// <summary>
    /// Returns the UtcCreated of the Timestamped model if it is not default(DateTimeOffset), otherwise returns UtcNow
    /// </summary>
    internal class UtcCreatedMapper : MemberMapper<TimestampedModel, DateTimeOffset>
    {
        public override DateTimeOffset GetValue(TimestampedModel source)
        {
            return source.UtcCreated == default(DateTimeOffset) ? DateTimeOffset.UtcNow : source.UtcCreated;
        }
    }
}