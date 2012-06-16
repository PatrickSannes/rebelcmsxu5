using System;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Used to set the member rules when creating mappings
    /// </summary>
    internal class MemberMappingExpression<TSource> : IMemberMappingExpression<TSource>
    {
        private readonly PropertyMapDefinition _info;        

        public MemberMappingExpression(PropertyMapDefinition info)
        {
            _info = info;
            ResultOfMapFrom = null;
        }

        internal bool IsIgnored { get; private set; }
        internal object ResultOfMapFrom { get; private set; }

        public void Ignore()
        {
            IsIgnored = true;
        }

        public void MapFrom<TMember>(Func<TSource, TMember> sourceMember)
        {
            Mandate.ParameterNotNull(sourceMember, "sourceMember");
            ResultOfMapFrom = sourceMember((TSource)_info.Source.Value);
        }

        public void MapUsing<TMember>(MemberMapper<TSource, TMember> memberMapper)
        {
            Mandate.ParameterNotNull(memberMapper, "memberMapper");
            ResultOfMapFrom = memberMapper.GetValue((TSource) _info.Source.Value);
        }

        public void MapUsing<TMemberMapper>() where TMemberMapper : IMemberMapper, new()
        {
            var memberMapper = new TMemberMapper();
            ResultOfMapFrom = memberMapper.GetValue((TSource)_info.Source.Value);
        }
    }
}