using System;
using System.Linq;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution
{
    public class TypedAttributeCollection : EntityGraph<ITypedAttribute>, ITypedAttributeCollection
    {
        #region Implementation of ITypedAttributeCollection

        public ITypedAttribute this[string attributeName]
        {
            get { return Get(attributeName); }
        }

        public ITypedAttribute Get(ITypedAttributeName attributeName)
        {
            return
                this.SingleOrDefault(
                    a =>
                    string.Compare(a.AttributeType.Alias, attributeName.Alias,
                                   StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public ITypedAttribute Get(string attributeName)
        {
            return 
                this.SingleOrDefault(
                    a =>
                    string.Compare(a.AttributeType.Alias, attributeName,
                                    StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public bool Contains(string attributeName)
        {
            return this.Any<ITypedAttribute>(x => x.AttributeType.Alias == attributeName);
        }

        #endregion
    }
}