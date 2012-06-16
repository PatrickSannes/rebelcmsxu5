using System;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Used to create the mapping rules for members when creating maps
    /// </summary>
    public interface IMemberMappingExpression<TSource>
    {
        void Ignore();

        void MapFrom<TMember>(Func<TSource, TMember> sourceMember);

        void MapUsing<TMember>(MemberMapper<TSource, TMember> memberMapper);

        void MapUsing<TMemberMapper>()
            where TMemberMapper : IMemberMapper, new();
    }
}