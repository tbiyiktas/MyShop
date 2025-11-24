using MyShop.Application.Abstractions;
using System.Linq.Expressions;

namespace MyShop.Application.Specifications.Base;

// NOT
public sealed class NotSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    public NotSpecification(ISpecification<TEntity> inner)
    {
        if (inner.Criteria is null)
        {
            Criteria = null;
        }
        else
        {
            var param = inner.Criteria.Parameters.Single();
            var body = Expression.Not(inner.Criteria.Body);
            Criteria = Expression.Lambda<Func<TEntity, bool>>(body, param);
        }

        // Includes & Order'ı mirror et
        IncludeExpressions.AddRange(inner.IncludeExpressions);
        OrderBy = inner.OrderBy;
        OrderByDescending = inner.OrderByDescending;

        // AsNoTracking & IgnoreQueryFilters'ı mirror et
        if (inner.AsNoTracking)
        {
            ApplyAsNoTracking();
        }

        if (inner.IgnoreQueryFilters)
        {
            ApplyIgnoreQueryFilters();
        }
    }
}
