namespace Umbraco.Framework.Linq.CriteriaGeneration.ExpressionVisitors
{
    using System;
    using System.Linq.Expressions;

    using Umbraco.Framework.Dynamics.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;

    /// <summary>
    /// A static class containing helper methods to rewrite expressions into <see cref="FieldPredicateExpression"/> trees where appropriate.
    /// </summary>
    /// <remarks></remarks>
    public static class FieldPredicateExpressionRewriter
    {
        /// <summary>
        /// Determines whether the supplied <see cref="BinaryExpression"/> is convertible to a binary in the context of the featureset of
        /// the Umbraco Framework-supported expression tree.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="structureBinder">The structure binder.</param>
        /// <returns><c>true</c> if [is convertible binary] [the specified expression]; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public static bool IsConvertibleBinary(BinaryExpression expression, AbstractQueryStructureBinder structureBinder)
        {
            var left = expression.Left;
            var right = expression.Right;

            BindingSignatureSupport bindingSignatureSupport = GetBindingSupport(left, structureBinder);

            return bindingSignatureSupport != null && (ExpressionHelper.IsConstant(right) || right is UnaryExpression);
        }

        /// <summary>
        /// Gets a <see cref="BindingSignatureSupport"/> from an expression.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="structureBinder">The structure binder.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static BindingSignatureSupport GetBindingSupport(Expression left, AbstractQueryStructureBinder structureBinder)
        {
            Func<MethodCallExpression, BindingSignatureSupport>
                getMethodSupport = x => ExpressionHelper.IsMethod(x) ? structureBinder.IsSupportedMethod(x) : null;

            Func<MemberExpression, BindingSignatureSupport>
                getMemberSupport = x => ExpressionHelper.IsMember(x) ? structureBinder.IsSupportedMember(x) : null;

            var supportedNonDynamic = getMemberSupport.Invoke(left as MemberExpression) ?? getMethodSupport.Invoke(left as MethodCallExpression);

            if (supportedNonDynamic == null)
            {
                var unary = left as UnaryExpression;
                if (unary != null)
                {
                    var innerMethod = unary.Operand as MethodCallExpression;
                    if (innerMethod != null)
                        return getMethodSupport.Invoke(innerMethod);
                }
            }

            return supportedNonDynamic;
        }

        /// <summary>
        /// Negates a field predicate, i.e. reverses its <see cref="ValuePredicateType"/> for example Equals will be switched to NotEquals.
        /// </summary>
        /// <param name="fieldPredicateExpression">The field predicate expression.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static FieldPredicateExpression NegateFieldPredicate(FieldPredicateExpression fieldPredicateExpression)
        {
            Func<ValuePredicateType> reversed = () =>
                {
                    switch (fieldPredicateExpression.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            return ValuePredicateType.NotEqual;
                        case ValuePredicateType.GreaterThan:
                            return ValuePredicateType.LessThan;
                        case ValuePredicateType.GreaterThanOrEqual:
                            return ValuePredicateType.LessThanOrEqual;
                        case ValuePredicateType.NotEqual:
                            return ValuePredicateType.Equal;
                    }
                    return fieldPredicateExpression.ValueExpression.ClauseType;
                };

            return new FieldPredicateExpression(fieldPredicateExpression.SelectorExpression, new FieldValueExpression(reversed.Invoke(), fieldPredicateExpression.ValueExpression.Value));
        }

        /// <summary>
        /// Converts a <see cref="BinaryExpression"/> to a field predicate expression, if supported and convertible.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="structureBinder">The structure binder.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Expression ConvertToFieldPredicate(BinaryExpression expression, AbstractQueryStructureBinder structureBinder)
        {
            if (IsConvertibleBinary(expression, structureBinder))
            {
                var left = expression.Left;

                // Check if the left had side is a Unary wrapping a DynamicMemberMetadata call just to get the type right
                var unaryLeft = left as UnaryExpression;
                if (unaryLeft != null)
                {
                    var methodLeft = unaryLeft.Operand as MethodCallExpression;
                    if (methodLeft != null && methodLeft.Method == DynamicMemberMetadata.GetMemberMethod)
                    {
                        left = methodLeft;
                    }
                }

                // First assume that the right hand side of the binary is a constant
                ConstantExpression right = expression.Right as ConstantExpression;

                // If it's not, it might be a unary (e.g. "convert value to object" if it's from DynamicMemberMetadata)
                if (right == null)
                {
                    var unary = expression.Right as UnaryExpression;
                    if (unary != null)
                    {
                        // If it was a unary, ignore the operator and just go for the operand (the value)
                        right = unary.Operand as ConstantExpression;
                    }
                }

                BindingSignatureSupport leftBindingSignatureSupport = GetBindingSupport(left, structureBinder);

                // Update the ValuePredicateType based on the expression which might reference an operator or a NodeType of NotEqual etc.
                UpdateValuePredicateType(leftBindingSignatureSupport, expression);

                switch (leftBindingSignatureSupport.SignatureSupportType)
                {
                    case SignatureSupportType.SupportedAsFieldName:
                        var selectorExpression = GetFieldSelector(left, structureBinder, leftBindingSignatureSupport);
                        return new FieldPredicateExpression(selectorExpression, new FieldValueExpression(leftBindingSignatureSupport.NodeType, right.Value));
                    case SignatureSupportType.SupportedAsFieldValue:
                        var methodCallExpression = ((MethodCallExpression)left);
                        if (ExpressionHelper.IsMethod(left))
                        {
                            var fieldValueExpression = structureBinder
                                .CreateFieldValueExpression(methodCallExpression, leftBindingSignatureSupport);

                            var objectOfMethod = methodCallExpression.Object;
                            var bindingSupportForMethodObject = GetBindingSupport(objectOfMethod, structureBinder);
                            var fieldSelector = GetFieldSelector(objectOfMethod, structureBinder, bindingSupportForMethodObject);

                            return new FieldPredicateExpression(fieldSelector, fieldValueExpression);
                        }
                        break;
                    case SignatureSupportType.SupportedAsSchemaAlias:
                        var schemaSelectorExpression = GetSchemaSelector(left, structureBinder, leftBindingSignatureSupport);
                        return
                            new SchemaPredicateExpression(
                                new SchemaSelectorExpression(schemaSelectorExpression.Name),
                                new SchemaValueExpression(leftBindingSignatureSupport.NodeType, right.Value));
                }
            }

            return expression;
        }

        private static void UpdateValuePredicateType(BindingSignatureSupport signatureSupport, BinaryExpression binaryExpression)
        {
            // Only modify the ValuePredicateType if it already is "Equals" or "Empty". This is pending a refactor
            // of BindingSignatureSupport to include a negation property and the removal of NotEquals

            if (signatureSupport.NodeType != ValuePredicateType.Equal && signatureSupport.NodeType != ValuePredicateType.Empty) return;

            // First check the NodeType. Binaries joined with "!= true" will have a NodeType of NotEqual
            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Equal:
                    signatureSupport.NodeType = ValuePredicateType.Equal;
                    break;
                case ExpressionType.NotEqual:
                    signatureSupport.NodeType = ValuePredicateType.NotEqual;
                    break;
                case ExpressionType.GreaterThan:
                    signatureSupport.NodeType = ValuePredicateType.GreaterThan;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    signatureSupport.NodeType = ValuePredicateType.GreaterThanOrEqual;
                    break;
                case ExpressionType.LessThan:
                    signatureSupport.NodeType = ValuePredicateType.LessThan;
                    break;
                case ExpressionType.LessThanOrEqual:
                    signatureSupport.NodeType = ValuePredicateType.LessThanOrEqual;
                    break;
            }

            // Then check the Method, which might be an operator
            if (binaryExpression.Method != null)
                switch (binaryExpression.Method.Name)
                {
                    case "op_Inequality":
                        signatureSupport.NodeType = ValuePredicateType.NotEqual;
                        break;
                }
        }

        private static SchemaSelectorExpression GetSchemaSelector(Expression left, AbstractQueryStructureBinder structureBinder, BindingSignatureSupport bindingSignatureSupport)
        {
            switch (bindingSignatureSupport.SignatureSupportType)
            {
                case SignatureSupportType.SupportedAsSchemaAlias:
                    if (ExpressionHelper.IsMember(left))
                        return structureBinder.CreateSchemaSelector(left as MemberExpression, bindingSignatureSupport);
                    else if (ExpressionHelper.IsMethod(left))
                        return structureBinder.CreateSchemaSelector(left as MethodCallExpression, bindingSignatureSupport);
                    break;
            }
            return null;
        }

        public static FieldSelectorExpression GetFieldSelector(Expression left, AbstractQueryStructureBinder structureBinder)
        {
            var signatureSupport = GetBindingSupport(left, structureBinder);
            return GetFieldSelector(left, structureBinder, signatureSupport);
        }

        /// <summary>
        /// Gets a <see cref="FieldSelectorExpression"/> from an expression it the <paramref name="bindingSignatureSupport"/> identifies it as supported.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="structureBinder">The structure binder.</param>
        /// <param name="bindingSignatureSupport">The binding signature support.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static FieldSelectorExpression GetFieldSelector(Expression left, AbstractQueryStructureBinder structureBinder, BindingSignatureSupport bindingSignatureSupport)
        {
            switch (bindingSignatureSupport.SignatureSupportType)
            {
                case SignatureSupportType.SupportedAsFieldName:
                    if (ExpressionHelper.IsMember(left))
                        return structureBinder.CreateFieldSelector(left as MemberExpression, bindingSignatureSupport);
                    else if (ExpressionHelper.IsMethod(left))
                        return structureBinder.CreateFieldSelector(left as MethodCallExpression, bindingSignatureSupport);
                    break;
            }
            return null;
        }
    }
}