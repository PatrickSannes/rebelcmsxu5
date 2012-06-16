namespace Umbraco.Cms.Web.Security
{
    /// <summary>
    /// Data structure used to store information in the authentication cookie
    /// </summary>
    internal class UserData
    {
        public string Id { get; set; }
        public string[] Roles { get; set; }
        public int SessionTimeout { get; set; }
        public string Username { get; set; }
        public string RealName { get; set; }
        public string StartContentNode { get; set; }
        public string StartMediaNode { get; set; }
        public string[] AllowedApplications { get; set; }
    }
}