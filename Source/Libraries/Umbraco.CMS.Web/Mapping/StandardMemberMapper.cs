using Umbraco.Cms.Web.Context;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    internal abstract class StandardMemberMapper<TFrom, TTo> : MemberMapper<TFrom, TTo>
    {
        public AbstractFluentMappingEngine CurrentEngine { get; private set; }
        protected MapResolverContext ResolverContext { get; private set; }

        protected StandardMemberMapper(AbstractFluentMappingEngine currentEngine, MapResolverContext context)
        {
            CurrentEngine = currentEngine;
            ResolverContext = context;
        }
    }
}