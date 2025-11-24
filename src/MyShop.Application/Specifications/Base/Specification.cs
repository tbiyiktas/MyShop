using System.Linq.Expressions;
using MyShop.Application.Abstractions;

namespace MyShop.Application.Specifications.Base;

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

    // Fluent include with ThenInclude support
    protected void ApplyInclude(Action<IncludeBuilder<TEntity>> configure)
    {
        var builder = new IncludeBuilder<TEntity>();
        configure(builder);
        IncludeExpressions.AddRange(builder.Build());
    }

    protected void ApplyOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression)
        => OrderBy = orderByExpression;

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

    protected void ApplyAsNoTracking()
        => AsNoTracking = true;

    protected void ApplyIgnoreQueryFilters()
        => IgnoreQueryFilters = true;

    // NEW: Split query support
    protected void ApplyAsSplitQuery()
        => AsSplitQuery = true;
}
