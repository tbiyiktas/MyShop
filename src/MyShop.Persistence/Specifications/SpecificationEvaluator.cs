using Microsoft.EntityFrameworkCore;
using MyShop.Application.Abstractions;

namespace MyShop.Persistence.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this IQueryable<TEntity> query,
        ISpecification<TEntity> specification)
        where TEntity : class
    {
        if (specification is null)
            return query;

        // Apply AsNoTracking for read-only queries (performance optimization)
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply IgnoreQueryFilters to bypass global filters (e.g., soft delete)
        if (specification.IgnoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Advanced includes with ThenInclude support
        foreach (var includeExpression in specification.IncludeExpressions)
        {
            query = includeExpression.Apply(query);
        }

        if (specification.OrderBy is not null)
        {
            query = specification.OrderBy(query);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = specification.OrderByDescending(query);
        }

        // Apply split query to prevent cartesian explosion
        if (specification.AsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        return query;
    }
}
