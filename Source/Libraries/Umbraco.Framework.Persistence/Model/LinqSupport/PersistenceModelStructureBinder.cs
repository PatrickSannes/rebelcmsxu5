using System.Linq.Expressions;

using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Umbraco.Framework.Persistence.Model.LinqSupport
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;

    /// <summary>
    /// Provides a mechanism to assess certain types of expression and determine whether they are supported by the expression binding provider. 
    /// </summary>
    /// <remarks></remarks>
    public class PersistenceModelStructureBinder : DefaultQueryStructureBinder
    {
        /// <summary>
        /// Creates a <see cref="FieldSelectorExpression"/> from a <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="reportedSignatureSupport">A component outlining the supported expression structure of this provider.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public override FieldSelectorExpression CreateFieldSelector(MethodCallExpression expression, BindingSignatureSupport reportedSignatureSupport)
        {
            switch (expression.Method.Name)
            {
                case "Attribute":
                    // This is an extension method for the Persistence model so the first argument is the extended object
                    
                    if (expression.Arguments.Count == 3)
                    {
                        //if there are 3 arguments, then we are selecting a field by name with a value key
                        return new FieldSelectorExpression(
                            ((ConstantExpression) expression.Arguments[1]).Value.ToString(),
                            ((ConstantExpression) expression.Arguments[2]).Value.ToString());
                    }
                    if (expression.Arguments.Count == 2)
                    {                        
                        //if there are 2 arguments, then we are selecting a field only by name regardless of the value key
                        return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[1]).Value.ToString());    
                    }
                    break;
                case "get_Item":
                    // This is the default accessor of a Dictionary, so check if the parent object is supported too
                    if (ExpressionHelper.IsMember(expression.Object) && IsSupportedMember(expression.Object as MemberExpression).SignatureSupportType != SignatureSupportType.NotSupported)
                        return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[0]).Value.ToString());

                    // There are two dictionaries on this model, the TypedAttributeCollection and its child TypedAttributeValueCollection.
                    // Make sure we're getting the field name from the TypedAttributeCollection indexer. To do this, access the MemberExpression
                    // "one up the chain" and recurse back into this method.
                    if (ExpressionHelper.IsMember(expression.Object) && ExpressionHelper.IsMethod(((MemberExpression)expression.Object).Expression))
                        return CreateFieldSelector(((MemberExpression)expression.Object).Expression as MethodCallExpression, reportedSignatureSupport);

                    break;
            }

            return base.CreateFieldSelector(expression, reportedSignatureSupport);
        }

        /// <summary>
        /// Determines whether the expression represents a supported method call.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <remarks></remarks>
        public override BindingSignatureSupport IsSupportedMethod(MethodCallExpression expression)
        {
            return base.IsSupportedMethod(expression);
        }

        /// <summary>
        /// Determines whether the expression represents a supported member access call.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <remarks></remarks>
        public override BindingSignatureSupport IsSupportedMember(MemberExpression expression)
        {
            if (typeof(TypedAttributeValueCollection).IsAssignableFrom(expression.Type))
            {
                return new BindingSignatureSupport(SignatureSupportType.NotSupported, ValuePredicateType.Empty);
            }

            // Account for if this is a schema access
            var memberParent = expression.Expression as MemberExpression;
            if (memberParent != null)
            {
                if (memberParent.Member.Name == "EntitySchema")
                {
                    switch (expression.Member.Name)
                    {
                        case "Alias":
                            return new BindingSignatureSupport(SignatureSupportType.SupportedAsSchemaAlias, ValuePredicateType.Equal);
                    }
                    return new BindingSignatureSupport(SignatureSupportType.SupportedAsSchemaMetaDataValue, ValuePredicateType.Equal);
                }
            }

            // If it's not schema, treat it like it's a field expression
            switch (expression.Member.Name)
            {
                case "Attributes":
                    return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.Equal);
            }

            return base.IsSupportedMember(expression);
        }
    }
}