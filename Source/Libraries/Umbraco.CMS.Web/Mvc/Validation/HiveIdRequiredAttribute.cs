using System;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class HiveIdRequiredAttribute : RequiredAttribute
    {

        public HiveIdRequiredAttribute()
        {
            ErrorMessage = "Field {0} is required";
        }

        public override bool IsValid(object value)
        {
            if (value is HiveId)
            {
                var forChecking = (HiveId)value;
                return !forChecking.IsNullValueOrEmpty();
            }
            return false;
        }
    }
}
