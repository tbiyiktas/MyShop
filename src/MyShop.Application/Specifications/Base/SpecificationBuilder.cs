using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Fluent builder for creating specifications.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public class SpecificationBuilder<TEntity> where TEntity : class
{
    private readonly List<Expression<Func<TEntity, bool>>> _criteriaList = new();
    private readonly List<Expression<Func<TEntity, object>>> _includes = new();
    private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? _orderBy;
    private bool _asNoTracking;
    private bool _ignoreQueryFilters;

    /// <summary>
    /// Creates a new specification builder.
    /// </summary>
    public static SpecificationBuilder<TEntity> Create() => new();

    /// <summary>
    /// Adds a WHERE clause to the specification.
    /// Multiple Where calls are combined with AND logic.
    /// </summary>
    public SpecificationBuilder<TEntity> Where(Expression<Func<TEntity, bool>> criteria)
    {
        _criteriaList.Add(criteria);
        return this;
    }

    /// <summary>
    /// Adds an INCLUDE (eager loading) to the specification.
    /// </summary>
    public SpecificationBuilder<TEntity> Include(Expression<Func<TEntity, object>> include)
    {
        _includes.Add(include);
        return this;
    }

    /// <summary>
    /// Adds an ORDER BY clause to the specification.
    /// </summary>
    public SpecificationBuilder<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy)
    {
        _orderBy = q => q.OrderBy(orderBy);
        return this;
    }

    /// <summary>
    /// Adds an ORDER BY DESC clause to the specification.
    /// </summary>
    public SpecificationBuilder<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderBy)
    {
        _orderBy = q => q.OrderByDescending(orderBy);
        return this;
    }

    /// <summary>
    /// Adds a secondary sort (ThenBy) to an existing OrderBy.
    /// Must be called after OrderBy or OrderByDescending.
    /// </summary>
    public SpecificationBuilder<TEntity> ThenBy(Expression<Func<TEntity, object>> thenBy)
    {
        if (_orderBy is null)
        {
            // If no OrderBy exists, treat as OrderBy
            _orderBy = q => q.OrderBy(thenBy);
        }
        else
        {
            // Chain ThenBy to existing OrderBy
            var previousOrderBy = _orderBy;
            _orderBy = q => ((IOrderedQueryable<TEntity>)previousOrderBy(q)).ThenBy(thenBy);
        }
        return this;
    }

    /// <summary>
    /// Adds a secondary sort (ThenByDescending) to an existing OrderBy.
    /// Must be called after OrderBy or OrderByDescending.
    /// </summary>
    public SpecificationBuilder<TEntity> ThenByDescending(Expression<Func<TEntity, object>> thenBy)
    {
        if (_orderBy is null)
        {
            // If no OrderBy exists, treat as OrderByDescending
            _orderBy = q => q.OrderByDescending(thenBy);
        }
        else
        {
            // Chain ThenByDescending to existing OrderBy
            var previousOrderBy = _orderBy;
            _orderBy = q => ((IOrderedQueryable<TEntity>)previousOrderBy(q)).ThenByDescending(thenBy);
        }
        return this;
    }

    /// <summary>
    /// Enables AsNoTracking for read-only queries (performance optimization).
    /// </summary>
    public SpecificationBuilder<TEntity> AsNoTracking()
    {
        _asNoTracking = true;
        return this;
    }

    /// <summary>
    /// Bypasses global query filters (e.g., soft delete, tenant filters).
    /// </summary>
    public SpecificationBuilder<TEntity> IgnoreQueryFilters()
    {
        _ignoreQueryFilters = true;
        return this;
    }

    /// <summary>
    /// Builds and returns the final specification.
    /// </summary>
    public ISpecification<TEntity> Build()
    {
        return new DynamicSpecification<TEntity>(
            _criteriaList,
            _includes,
            _orderBy,
            _asNoTracking,
            _ignoreQueryFilters);
    }
}
