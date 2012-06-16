namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    using System.Linq.Expressions;

    /// <summary>
    /// Represents an abstract, custom boolean expression. In .NET 4 for creating custom expressions it is recommended to override
    /// the <see cref="NodeType"/> and <see cref="Type"/> properties rather than using the obsolete constructor on <see cref="Expression"/>
    /// </summary>
    /// <remarks></remarks>
    public class AbstractExpressionExtension<T> : Expression
    {
        /// <summary>
        /// Gets the node type of this <see cref="T:System.Linq.Expressions.Expression"/>.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Linq.Expressions.ExpressionType"/> values.</returns>
        /// <remarks></remarks>
        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="T:System.Linq.Expressions.Expression"/> represents.
        /// </summary>
        /// <returns>The <see cref="T:System.Type"/> that represents the static type of the expression.</returns>
        /// <remarks></remarks>
        public override System.Type Type
        {
            get
            {
                return typeof(T);
            }
        }
    }
}