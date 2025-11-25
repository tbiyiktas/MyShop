using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Internal dynamic specification created by <see cref="SpecificationBuilder{TEntity}"/>.
/// This specification combines multiple criteria, includes, and query options into a single specification.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// This class is not intended for direct use. Use <see cref="SpecificationBuilder{TEntity}"/> to create instances.
/// </remarks>
internal class DynamicSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicSpecification{TEntity}"/> class.
    /// </summary>
    /// <param name="criteriaList">List of criteria expressions to combine using AND logic.</param>
    /// <param name="includes">List of include expressions for eager loading.</param>
    /// <param name="orderBy">Optional ordering function.</param>
    /// <param name="asNoTracking">Whether to apply AsNoTracking for read-only queries.</param>
    /// <param name="ignoreQueryFilters">Whether to ignore global query filters.</param>
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
