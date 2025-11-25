using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

/// <summary>
/// Base implementation of <see cref="ISpecification{TEntity}"/> providing default behavior for criteria,
/// includes, ordering, and query execution flags.
/// </summary>
/// <typeparam name="TEntity">The entity type the specification targets.</typeparam>
public abstract class Specification<TEntity> : ISpecification<TEntity>
    where TEntity : class
{
    public virtual Expression<Func<TEntity, bool>>? Criteria { get; protected set; }

    // Advanced includes with ThenInclude support
    public List<IIncludeExpression<TEntity>> IncludeExpressions { get; } = new();

    public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; protected set; }
    public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderByDescending { get; protected set; }

    public bool AsNoTracking { get; private set; }
    public bool IgnoreQueryFilters { get; private set; }
    public bool AsSplitQuery { get; private set; }

    /// <summary>
    /// Configures eager‑loading includes using the fluent <see cref="IncludeBuilder{TEntity}"/>.
    /// </summary>
    /// <param name="configure">Action that receives a builder to define include chains.</param>
    protected void ApplyInclude(Action<IncludeBuilder<TEntity>> configure)
    {
        var builder = new IncludeBuilder<TEntity>();
        configure(builder);
        IncludeExpressions.AddRange(builder.Build());
    }

    /// <summary>
    /// Sets the ordering function for ascending order.
    /// </summary>
    /// <param name="orderByExpression">Expression defining the ordering.</param>
    protected void ApplyOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression)
        => OrderBy = orderByExpression;

    /// <summary>
    /// Sets the ordering function for descending order.
    /// </summary>
    /// <param name="orderByDescendingExpression">Expression defining the descending ordering.</param>
    protected void ApplyOrderByDescending(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByDescendingExpression)
        => OrderByDescending = orderByDescendingExpression;

    /// <summary>
    /// Adds a secondary sort (ThenBy) to an existing OrderBy.
    /// Must be called after ApplyOrderBy.
    /// </summary>
    protected void ApplyThenBy(Expression<Func<TEntity, object>> thenByExpression)
    {
        if (OrderBy is null)
        {
            // If no OrderBy exists, treat as OrderBy
            ApplyOrderBy(q => q.OrderBy(thenByExpression));
        }
        else
        {
            // Chain ThenBy to existing OrderBy
            var previousOrderBy = OrderBy;
            OrderBy = q => ((IOrderedQueryable<TEntity>)previousOrderBy(q)).ThenBy(thenByExpression);
        }
    }

    /// <summary>
    /// Adds a secondary sort (ThenByDescending) to an existing OrderBy.
    /// Must be called after ApplyOrderBy.
    /// </summary>
    protected void ApplyThenByDescending(Expression<Func<TEntity, object>> thenByExpression)
    {
        if (OrderBy is null)
        {
            // If no OrderBy exists, treat as OrderByDescending
            ApplyOrderByDescending(q => q.OrderByDescending(thenByExpression));
        }
        else
        {
            // Chain ThenByDescending to existing OrderBy
            var previousOrderBy = OrderBy;
            OrderBy = q => ((IOrderedQueryable<TEntity>)previousOrderBy(q)).ThenByDescending(thenByExpression);
        }
    }

    /// <summary>
    /// Enables <c>AsNoTracking</c> for read‑only queries.
    /// </summary>
    protected void ApplyAsNoTracking()
        => AsNoTracking = true;

    /// <summary>
    /// Enables ignoring of global query filters.
    /// </summary>
    protected void ApplyIgnoreQueryFilters()
        => IgnoreQueryFilters = true;

    /// <summary>
    /// Enables EF Core split‑query execution.
    /// </summary>
    protected void ApplyAsSplitQuery()
        => AsSplitQuery = true;
}
