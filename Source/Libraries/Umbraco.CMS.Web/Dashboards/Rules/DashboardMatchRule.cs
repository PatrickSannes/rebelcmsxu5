namespace Umbraco.Cms.Web.Dashboards.Rules
{
    /// <summary>
    /// Abstract class defining configuration match rules for dashboard views/widgets
    /// </summary>
    public abstract class DashboardMatchRule
    {

        public abstract bool IsMatch(string condition);

    }
}
 