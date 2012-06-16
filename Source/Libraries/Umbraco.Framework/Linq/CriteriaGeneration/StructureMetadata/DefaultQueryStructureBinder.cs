namespace Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata
{
    using System.Linq.Expressions;

    using Umbraco.Framework.Dynamics.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    /// <summary>
    /// Provides a mechanism to assess certain types of expression and determine whether they are supported by the expression binding provider. 
    /// </summary>
    /// <remarks></remarks>
    public class DefaultQueryStructureBinder : AbstractQueryStructureBinder
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
                case "Field":
                    // This is an extension method for the RenderViewModel so the first argument is the extended object
                    return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[1]).Value.ToString());
                case "get_Item":
                    // This is the default accessor of a Dictionary, so check if the parent object is supported too
                    if (ExpressionHelper.IsMember(expression.Object) && IsSupportedMember(expression.Object as MemberExpression).SignatureSupportType != SignatureSupportType.NotSupported)
                        return new FieldSelectorExpression(((ConstantExpression) expression.Arguments[0]).Value.ToString());
                    break;
            }

            // Add support for dynamic expressions using an intermediary set of "fake" methods on DynamicMemberMetadata
            if (expression.Method == DynamicMemberMetadata.GetMemberMethod)
            {
                // The first argument is the field name
                return new FieldSelectorExpression(((ConstantExpression)expression.Arguments[0]).Value.ToString());
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
            switch (expression.Method.Name)
            {
                case "Equals":
                    return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldValue, ValuePredicateType.Equal);
                case "StartsWith":
                    return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldValue, ValuePredicateType.StartsWith);
                case "EndsWith":
                    return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldValue, ValuePredicateType.EndsWith);
                case "get_Item":
                    // This is the default accessor of a Dictionary, so check if the parent object is supported too
                    if (ExpressionHelper.IsMember(expression.Object) && IsSupportedMember(expression.Object as MemberExpression).SignatureSupportType != SignatureSupportType.NotSupported)
                        return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.Equal);
                    break;
            }

            // Add support for dynamic expressions using an intermediary set of "fake" methods on DynamicMemberMetadata
            if (expression.Method == DynamicMemberMetadata.GetMemberMethod)
            {
                var firstArg = expression.Arguments[0] as ConstantExpression;
                if (firstArg != null)
                switch (firstArg.Value as string)
                {
                    case "NodeTypeAlias":
                    case "ContentTypeAlias":
                    case "MediaTypeAlias":
                        return new BindingSignatureSupport(SignatureSupportType.SupportedAsSchemaAlias, ValuePredicateType.Empty);
                }

                return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.Empty);
            }

            return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.Equal);
        }

        /// <summary>
        /// Determines whether the expression represents a supported member access call.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <remarks></remarks>
        public override BindingSignatureSupport IsSupportedMember(MemberExpression expression)
        {
            // Account for if this is a schema access. TODO: Move this into a separate QueryStructureBinder
            var memberParent = expression.Expression as MemberExpression;
            if (memberParent != null)
            {
                if (memberParent.Member.Name == "ContentType")
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
                case "Fields":
                    return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.Equal);
            }
            return new BindingSignatureSupport(SignatureSupportType.SupportedAsFieldName, ValuePredicateType.Equal);
        }
    }
}