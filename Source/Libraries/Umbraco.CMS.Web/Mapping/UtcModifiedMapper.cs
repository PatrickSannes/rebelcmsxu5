using System;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    internal class UtcModifiedMapper : MemberMapper<TimestampedModel, DateTimeOffset>
    {
        public override DateTimeOffset GetValue(TimestampedModel source)
        {
            return source.UtcModified == default(DateTimeOffset) ? DateTimeOffset.UtcNow : source.UtcModified;
        }
    }
}