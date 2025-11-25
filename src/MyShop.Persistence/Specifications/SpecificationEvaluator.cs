using Microsoft.EntityFrameworkCore;
using MyShop.Application.Abstractions;

namespace MyShop.Persistence.Specifications;

/// <summary>
/// Evaluates and applies <see cref="ISpecification{TEntity}"/> to an <see cref="IQueryable{TEntity}"/>.
/// Transforms specification definitions into EF Core query expressions.
/// </summary>
/// <remarks>
/// <para>
/// This evaluator applies specification components in a specific order to build an optimized EF Core query:
/// </para>
/// <list type="number">
/// <item><description>AsNoTracking - Applied first for read-only optimization</description></item>
/// <item><description>IgnoreQueryFilters - Bypasses global filters (e.g., soft delete)</description></item>
/// <item><description>Criteria (Where) - Filters entities based on the specification's criteria</description></item>
/// <item><description>Includes - Eager loads navigation properties using Include/ThenInclude</description></item>
/// <item><description>OrderBy/OrderByDescending - Applies sorting</description></item>
/// <item><description>AsSplitQuery - Prevents cartesian explosion for multiple collection includes</description></item>
/// </list>
/// </remarks>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Applies the given specification to the query, transforming it into an EF Core query.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="query">The base queryable to apply the specification to.</param>
    /// <param name="specification">The specification to apply. If null, returns the original query unchanged.</param>
    /// <returns>A new queryable with all specification components applied.</returns>
    /// <remarks>
    /// This method is typically called by repository implementations to convert specifications into executable queries.
    /// </remarks>
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
