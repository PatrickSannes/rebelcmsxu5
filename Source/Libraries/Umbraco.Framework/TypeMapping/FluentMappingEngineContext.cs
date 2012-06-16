using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Framework.Context;

namespace Umbraco.Framework.TypeMapping
{
    

    /// <summary>
    /// Each injector mapping operations contains a context which represents the Mapping engine being used to 
    /// do all of the mappings and exposes internally the member rules for mapping operations.
    /// 
    /// One mapping context exists for each map created.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    public class FluentMappingEngineContext<TSource, TTarget> : IMappingExpression<TSource, TTarget>
    {
        public FluentMappingEngineContext(AbstractFluentMappingEngine engine)
        {
            Engine = engine;
            FrameworkContext = engine.FrameworkContext;
        }

        private readonly HashSet<MemberExpressionSignature<TSource>> _memberExpressionSignatures = new HashSet<MemberExpressionSignature<TSource>>();

        //private readonly HashSet<int> _memberMapped = new HashSet<int>();

        /// <summary>
        /// Gets the current MappingEngine
        /// </summary>
        public AbstractFluentMappingEngine Engine { get; private set; }

        /// <summary>
        /// The current IFrameworkContext
        /// </summary>
        public IFrameworkContext FrameworkContext { get; private set; }

        /// <summary>
        /// Gets or sets the create using delegate for the Target type
        /// </summary>
        /// <value>
        /// The create using.
        /// </value>
        internal Func<TSource, TTarget> CreateUsingAction { get; set; }

        /// <summary>
        /// Gets or sets the after map delegate for the mapping operation
        /// </summary>
        /// <value>
        /// The after map.
        /// </value>
        internal Action<TSource, TTarget> AfterMapAction { get; set; }

        /// <summary>
        /// Returns all member expressions registered
        /// </summary>
        internal IEnumerable<MemberExpressionSignature<TSource>> MemberExpressions
        {
            get { return _memberExpressionSignatures; }
        }

        /// <summary>
        /// Adds a member expression to the internal list
        /// </summary>
        /// <param name="member"></param>
        /// <param name="actionToExecute">The expression.</param>
        public void AddMemberExpression<TProperty>(Expression<Func<TTarget, TProperty>> member, Action<IMemberMappingExpression<TSource>> actionToExecute)
        {
            Mandate.That(member.Body is MemberExpression,
                         x => new NotSupportedException("AddMemberExpression can only be called on a Member/Field of an object"));

            //_memberExpressions.Add(member, expression);
            //create a signature from the expression
            var memberExpressionCasted = (MemberExpression)member.Body;
            _memberExpressionSignatures.Add(new MemberExpressionSignature<TSource>(
                                                memberExpressionCasted.Type,
                                                typeof(TTarget),
                                                memberExpressionCasted.Member.Name,
                                                actionToExecute));
        }

        /// <summary>
        /// Adds a member expression to the internal list
        /// </summary>
        /// <param name="targetMemberType"></param>
        /// <param name="targetMemberContainerType"></param>
        /// <param name="targetMemberName"></param>
        /// <param name="actionToExecute"></param>
        public void AddMemberExpression(Type targetMemberType, Type targetMemberContainerType, string targetMemberName, Action<IMemberMappingExpression<TSource>> actionToExecute)
        {
            Mandate.ParameterNotNull(targetMemberType, "targetMemberType");
            Mandate.ParameterNotNull(targetMemberContainerType, "targetMemberContainerType");
            Mandate.ParameterNotNullOrEmpty(targetMemberName, "targetMemberName");

            _memberExpressionSignatures.Add(new MemberExpressionSignature<TSource>(
                                                targetMemberType,
                                                targetMemberContainerType,
                                                targetMemberName,
                                                actionToExecute));
        }

        /// <summary>
        /// Determine if the member value should be mapped from a rule, if true, then the value of 'value' will be set to the custom mapping
        /// </summary>
        /// <param name="info"></param>
        /// <param name="expression"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool MapFromRule(PropertyMapDefinition info, MemberExpressionSignature expression, out object value)
        {
            //find the expression in our list based on the target object type, the target property type and the target property name
            var action = (from m in _memberExpressionSignatures
                          where m.Equals(expression)
                          select m.ActionToExecute).SingleOrDefault();


            if (action != null)
            {
                //invoke the action with the specified parameter
                var memberExpression = new MemberMappingExpression<TSource>(info);
                action.Invoke(memberExpression);

                if (!memberExpression.IsIgnored)
                {
                    //set the value to the output
                    value = memberExpression.ResultOfMapFrom;
                    return true;
                }
            }

            //no 'MapFrom' found
            value = null;
            return false;
        }


        public FluentMappingEngineContext<TSource, TTarget> MappingContext
        {
            get { return this; }
        }

        public IMappingExpression<TSource, TTarget> AfterMap(Action<TSource, TTarget> afterMap)
        {
            var expression = new MappingExpression<TSource, TTarget>(this);
            return expression.AfterMap(afterMap);
        }

        public IMappingExpression<TSource, TTarget> CreateUsing(Func<TSource, TTarget> createUsing)
        {
            var expression = new MappingExpression<TSource, TTarget>(this);
            return expression.CreateUsing(createUsing);
        }

        public IMappingExpression<TSource, TTarget> ForMember<TProperty>(Expression<Func<TTarget, TProperty>> destinationMember, Action<IMemberMappingExpression<TSource>> memberOptions)
        {
            var expression = new MappingExpression<TSource, TTarget>(this);
            return expression.ForMember(destinationMember, memberOptions);
        }
    }
}