using System;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using System.Web;

namespace Umbraco.Cms.Web.Dashboards.Rules
{
    /// <summary>
    /// Dashboard configuration match rule for matching a user's Role
    /// </summary>
    public class UserRoleMatchRule : DashboardMatchRule
    {
        private readonly HttpContextBase _http;
        private readonly IRoutableRequestContext _routableRequestContext;

        public UserRoleMatchRule(HttpContextBase http, IRoutableRequestContext routableRequestContext)
        {
            _http = http;
            _routableRequestContext = routableRequestContext;
        }

        public override bool IsMatch(string condition)
        {
            return _http.User.IsInRole(condition);
        }
    }
}