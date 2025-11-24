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
        Includes.AddRange(
            left.Includes
                .Concat(right.Includes)
                .Distinct());

        // Order: left > right
        OrderBy = left.OrderBy ?? right.OrderBy;
        OrderByDescending = left.OrderByDescending ?? right.OrderByDescending;
    }
}
