using System;
using Examine;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    internal class SearchResultToRevisionData : MemberMapper<SearchResult, RevisionData>
    {
        private readonly ExamineHelper _helper;
        
        public SearchResultToRevisionData(ExamineHelper helper)
        {
            _helper = helper;
        }

        public override RevisionData GetValue(SearchResult source)
        {
            var id = HiveId.Parse(source.Fields[FixedRevisionIndexFields.RevisionId]);
            var revStatusId = Guid.Parse(source.Fields[FixedRevisionIndexFields.RevisionStatusId]);            

            var revStatus = _helper.GetRevisionStatusType(revStatusId);
            if (revStatus == null)
            {
                throw new NotSupportedException("Could not find a revision status with status id: " + revStatusId.ToString("N"));
            }
            
            //NOTE: all dates on a revision will be the same correct? since they only exist one time. SD.
            return new RevisionData(id, revStatus)
                {
                    UtcCreated = ExamineHelper.FromExamineDateTime(source.Fields, FixedIndexedFields.UtcModified).Value,
                    UtcModified = ExamineHelper.FromExamineDateTime(source.Fields, FixedIndexedFields.UtcModified).Value,
                    UtcStatusChanged = ExamineHelper.FromExamineDateTime(source.Fields, FixedIndexedFields.UtcModified).Value
                };
        }
    }
}