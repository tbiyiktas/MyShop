using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

// AND
public sealed class AndSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    public AndSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
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
            Criteria = ExpressionCombiner.And(left.Criteria, right.Criteria);
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
    }
}
