using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using System;

namespace Umbraco.Cms.Web.Security
{

    /// <summary>
    /// A custom user identity for the Umbraco backoffice
    /// </summary>
    /// <remarks>
    /// All values are lazy loaded for performance reasons as the constructor is called for every single request
    /// </remarks>
    public class UmbracoBackOfficeIdentity : FormsIdentity
    {
        public UmbracoBackOfficeIdentity(FormsAuthenticationTicket ticket)
            : base(ticket)
        {
            _userData = ticket.UserData;
        }

        private HiveId _id;
        private readonly string _userData;
        private dynamic _deserializedValue;
        private string[] _roles;
        private HiveId _startContentNode;
        private HiveId _startMediaNode;
        private string[] _allowedApplications;

        /// <summary>
        /// Called before any property accessor
        /// </summary>
        private void EnsureDeserialized()
        {
            if (_deserializedValue != null)
                return;

            //create a bendey object from the user data
            if (string.IsNullOrEmpty(_userData))
            {
                _deserializedValue = new BendyObject();
                return;
            }
            _deserializedValue = new BendyObject((new JavaScriptSerializer()).Deserialize<IDictionary<string, object>>(_userData)).AsDynamic();
        }

        public HiveId Id
        {
            get
            {
                EnsureDeserialized();
                return _id != HiveId.Empty ? _id : (_id = HiveId.Parse((string)_deserializedValue.Id.ToString()));
            }
        }

        public string RealName
        {
            get
            {
                return _deserializedValue.RealName;
            }
        }

        /// <summary>
        /// Returns the role names that the user is a member of
        /// </summary>
        public string[] Roles
        {
            get
            {
                EnsureDeserialized();
                var roles = _deserializedValue.Roles;
                if (roles == null || roles is BendyObject) return new string[0];
                return _roles ?? (_roles = ((object[])roles).WhereNotNull().Select(x => x.ToString()).ToArray());
            }
            internal set { _roles = value; }
        }

        

        public int SessionTimeout
        {
            get
            {
                EnsureDeserialized();
                return _deserializedValue.SessionTimeout;
            }
        }

        public HiveId StartContentNode
        {
            get
            {
                EnsureDeserialized();
                return _startContentNode != HiveId.Empty ? _startContentNode : (_startContentNode = HiveId.Parse((string)_deserializedValue.StartContentNode));
            }
        }

        

        public HiveId StartMediaNode
        {
            get
            {
                EnsureDeserialized();
                return _startMediaNode != HiveId.Empty ? _startMediaNode : (_startMediaNode = HiveId.Parse((string)_deserializedValue.StartMediaNode));
            }
        }

        

        public string[] AllowedApplications
        {
            get
            {
                EnsureDeserialized();
                return _allowedApplications ?? (_allowedApplications = ((object[])_deserializedValue.AllowedApplications).Select(x => x.ToString()).ToArray());
            }
        }

        
    }
}