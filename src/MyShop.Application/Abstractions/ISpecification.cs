using System.Linq.Expressions;

namespace MyShop.Application.Abstractions;

public interface ISpecification<TEntity> where TEntity : class
{
    Expression<Func<TEntity, bool>>? Criteria { get; }

    // Advanced includes with ThenInclude support
    List<IIncludeExpression<TEntity>> IncludeExpressions { get; }

    // Sıralama için; IQueryable'ı sızdırmadan içerde kullanacağız.
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; }
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderByDescending { get; }

    // Performance optimization: disable change tracking for read-only queries
    bool AsNoTracking { get; }

    // Bypass global query filters (e.g., soft delete, tenant filters)
    bool IgnoreQueryFilters { get; }

    // Query splitting to prevent cartesian explosion
    bool AsSplitQuery { get; }
}
