using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Represents the logical OR composition of two specifications.
/// Combines criteria using OR logic and merges include expressions from both specifications.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// <para>
/// This specification is created by the <see cref="SpecificationExtensions.Or{TEntity}"/> extension method.
/// </para>
/// <para>
/// <strong>Criteria Combination:</strong> If both specifications have criteria, they are combined using OR logic.
/// If only one has criteria, that criteria is used. If neither has criteria, the result has no criteria.
/// </para>
/// <para>
/// <strong>Include Merging:</strong> Include expressions from both specifications are merged and deduplicated.
/// </para>
/// <para>
/// <strong>Flag Propagation:</strong> Query execution flags (AsNoTracking, IgnoreQueryFilters) are set to true
/// if either the left or right specification has them enabled.
/// </para>
/// </remarks>
public sealed class OrSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrSpecification{TEntity}"/> class.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    public OrSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
    {
        if (left.Criteria is null && right.Criteria is null)
        {
            Criteria = null;
        }
        else if (left.Criteria is null)
        {
            Criteria = right.Criteria;
        }
        else if (right.Criteria is null)
        {
            Criteria = left.Criteria;
        }
        else
        {
            Criteria = ExpressionCombiner.Or(left.Criteria, right.Criteria);
        }

        // Includes: left + right (distinct)
        IncludeExpressions.AddRange(
            left.IncludeExpressions
                .Concat(right.IncludeExpressions)
                .Distinct());

        // Order: left > right
        OrderBy = left.OrderBy ?? right.OrderBy;
        OrderByDescending = left.OrderByDescending ?? right.OrderByDescending;

        // AsNoTracking: true if either left or right is true
        if (left.AsNoTracking || right.AsNoTracking)
        {
            ApplyAsNoTracking();
        }

        // IgnoreQueryFilters: true if either left or right is true
        if (left.IgnoreQueryFilters || right.IgnoreQueryFilters)
        {
            ApplyIgnoreQueryFilters();
        }

        // AsSplitQuery: true if either left or right is true
        if (left.AsSplitQuery || right.AsSplitQuery)
        {
            ApplyAsSplitQuery();
        }
    }
}
