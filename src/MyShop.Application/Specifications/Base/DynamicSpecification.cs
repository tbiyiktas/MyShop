using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Internal dynamic specification created by SpecificationBuilder.
/// </summary>
internal class DynamicSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    internal DynamicSpecification(
        List<Expression<Func<TEntity, bool>>> criteriaList,
        List<Expression<Func<TEntity, object>>> includes,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
        bool asNoTracking,
        bool ignoreQueryFilters)
    {
        // Combine all criteria with AND
        if (criteriaList.Count > 0)
        {
            Criteria = criteriaList.Aggregate((left, right) => left.And(right));
        }

        // Add includes using new API
        if (includes.Count > 0)
        {
            ApplyInclude(builder =>
            {
                foreach (var include in includes)
                {
                    builder.Include(include);
                }
            });
        }

        // Order
        OrderBy = orderBy;

        // Flags
        if (asNoTracking)
        {
            ApplyAsNoTracking();
        }

        if (ignoreQueryFilters)
        {
            ApplyIgnoreQueryFilters();
        }
    }
}
