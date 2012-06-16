using System;
using System.Reflection;

namespace Umbraco.Framework
{
    ///<summary>
    ///</summary>
    public abstract class MemberInfoAttribute : Attribute
    {
        #region Properties

        public MemberInfo Target { get; set; }

        #endregion
    }
}