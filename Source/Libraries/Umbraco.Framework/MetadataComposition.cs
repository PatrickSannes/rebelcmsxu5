using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Framework
{
    /// <summary>
    /// A base class for all meta data classes for use with MEF must derive from.
    /// </summary>
    /// <remarks>
    /// This is required for medium trust support and MEF, medium trust doesn't like when interfaces are used for metadata because
    /// of the dynamic classes it tries to create at runtime.
    /// </remarks>
    public abstract class MetadataComposition : ICloneable
    {
        /// <summary>
        /// constructor, sets all properties of this object based on the dictionary values
        /// </summary>
        /// <param name="obj"></param>
        protected MetadataComposition(IDictionary<string, object> obj)
        {
            if (obj == null) return;
            var t = GetType();
            var props = t.GetProperties();
            foreach (var a in obj)
            {
                var p = props.Where(x => x.Name == a.Key).SingleOrDefault();
                if (p != null)
                {
                    p.SetValue(this, a.Value, null);
                }
            }
        }


        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
