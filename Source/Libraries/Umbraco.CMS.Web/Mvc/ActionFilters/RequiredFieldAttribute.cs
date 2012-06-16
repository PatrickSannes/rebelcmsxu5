using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    public enum FieldMatchCount { MatchAll, MatchAtLeastOne }
    public enum FieldMatchType { Normal, Regex }

    /// <summary>
    /// Makes an action method only respond if particular form field(s) is included in the request
    /// </summary>
    public class RequiredFieldAttribute : ActionMethodSelectorAttribute
    {
        
        private readonly string[] _fieldNames;
        private readonly FieldMatchCount _matchCount = FieldMatchCount.MatchAll;
        private readonly FieldMatchType _matchType = FieldMatchType.Normal;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldNames"></param>
        public RequiredFieldAttribute(params string[] fieldNames)
        {
            _fieldNames = fieldNames;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <param name="matchCount"></param>
        public RequiredFieldAttribute(FieldMatchCount matchCount, params string[] fieldNames)
        {
            _fieldNames = fieldNames;
            _matchCount = matchCount;
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <param name="matchCount"></param>
        /// <param name="matchType"></param>
        public RequiredFieldAttribute(FieldMatchCount matchCount, FieldMatchType matchType, params string[] fieldNames)
        {
            _fieldNames = fieldNames;
            _matchCount = matchCount;
            _matchType = matchType;
        }

        /// <summary>
        /// Ensure that field names specified are found in the request depending on the field match type.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            if (_matchType == FieldMatchType.Regex)
            {
                var matchCount = (from s in _fieldNames
                                  from f in controllerContext.HttpContext.Request.Form.AllKeys
                                  where Regex.IsMatch(f, s)
                                  select s).Count();

                return _matchCount == FieldMatchCount.MatchAll ? _fieldNames.Count() == matchCount : matchCount > 0;
            }
            else
            {
                return _matchCount == FieldMatchCount.MatchAll
                    ? _fieldNames.Select(s => controllerContext.HttpContext.Request.Form[s]).All(value => !string.IsNullOrEmpty(value))
                    : _fieldNames.Select(s => controllerContext.HttpContext.Request.Form[s]).Any(value => !string.IsNullOrEmpty(value));    
            }

        }
    }
}