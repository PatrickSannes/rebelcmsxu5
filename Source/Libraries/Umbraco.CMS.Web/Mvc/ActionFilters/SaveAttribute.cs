namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// Specific action to support the Save operation
    /// </summary>
    public sealed class SaveAttribute : RequiredFieldAttribute
    {

        public const string DefaultSaveButtonId = "submit.Save";

        /// <summary>
        /// Default constructor, always expect submit.Save value in the Request
        /// </summary>
        public SaveAttribute()
            : base(DefaultSaveButtonId)
        {
        }

        /// <summary>
        /// Alternate constructor allowing you to specify the id/name of the different save button 
        /// ids that could exist in the request, at least one must match.
        /// </summary>
        /// <param name="saveButtons"></param>
        public SaveAttribute(params string[] saveButtons)
            : base(FieldMatchCount.MatchAtLeastOne, saveButtons)
        {
        }

        /// <summary>
        /// Alternate constructor allowing you to specify the id/name of the different save button 
        /// ids that could exist in the request, at least one must match and they are matched using the FieldMatchType specified
        /// </summary>
        /// <param name="saveButtons"></param>
        /// <param name="matchType"></param>
        public SaveAttribute(FieldMatchType matchType, params string[] saveButtons)
            : base(FieldMatchCount.MatchAtLeastOne, matchType, saveButtons)
        {
        }


    }
}