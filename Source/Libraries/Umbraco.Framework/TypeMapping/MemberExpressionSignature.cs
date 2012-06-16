using System;

namespace Umbraco.Framework.TypeMapping
{

    internal class MemberExpressionSignature<TSource> : MemberExpressionSignature
    {
        public MemberExpressionSignature(Type targetMemberType, Type targetMemberContainerType, string targetMemberName, Action<IMemberMappingExpression<TSource>> expression)
            : base(targetMemberType, targetMemberContainerType, targetMemberName)
        {
            ActionToExecute = expression;
        }

        public Action<IMemberMappingExpression<TSource>> ActionToExecute { get; private set; }
    }

    internal class MemberExpressionSignature
    {
        internal MemberExpressionSignature(Type targetMemberType, Type targetMemberContainerType, string targetMemberName)
        {
            Mandate.ParameterNotNull(targetMemberType, "targetMemberType");
            Mandate.ParameterNotNull(targetMemberContainerType, "targetMemberContainerType");
            Mandate.ParameterNotNull(targetMemberName, "targetMemberName");

            TargetMemberType = targetMemberType;
            TargetMemberContainerType = targetMemberContainerType;
            TargetMemberName = targetMemberName;
        }

        internal Type TargetMemberType { get; private set; }
        internal Type TargetMemberContainerType { get; private set; }
        internal string TargetMemberName { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            //base equality off of our hash code.
            return obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            var hash = 31;
            //the hash code should be unique to the parent value, the parent type, the property name and the property type
            hash ^= TargetMemberType.GetHashCode();
            hash ^= TargetMemberContainerType.GetHashCode();
            hash ^= TargetMemberName.GetHashCode();
            return hash;
        }
    }
}