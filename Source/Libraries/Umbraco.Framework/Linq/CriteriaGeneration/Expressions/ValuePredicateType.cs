namespace Umbraco.Framework.Linq.CriteriaGeneration.Expressions
{
    /// <summary>
    /// An enumeration of the possible predicates for a value.
    /// </summary>
    /// <remarks></remarks>
    public enum ValuePredicateType
    {
        // Value-comparison types
        Equal,
        NotEqual,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        StartsWith,
        EndsWith,
        Contains,
        MatchesWildcard,
        Empty
    }
}