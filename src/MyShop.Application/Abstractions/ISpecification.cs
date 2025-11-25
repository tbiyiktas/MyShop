using System.Linq.Expressions;

namespace MyShop.Application.Abstractions;

/// <summary>
/// Represents a specification for querying entities of type <typeparamref name="TEntity"/>.
/// It encapsulates filtering criteria, eager‑loading includes, ordering, and query‑execution flags.
/// </summary>
/// <typeparam name="TEntity">The entity type the specification targets.</typeparam>
public interface ISpecification<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the predicate used to filter the query.
    /// </summary>
    /// <summary>
    /// Gets the predicate used to filter the query.
    /// Returns <c>null</c> when no filtering is applied.
    /// </summary>
    Expression<Func<TEntity, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the collection of include expressions for eager loading navigation properties.
    /// </summary>
    /// <summary>
    /// Gets the collection of include expressions for eager loading navigation properties.
    /// Each expression is applied to the <c>IQueryable</c> during evaluation.
    /// </summary>
    List<IIncludeExpression<TEntity>> IncludeExpressions { get; }

    /// <summary>
    /// Gets the function that applies an ascending ordering to the query.
    /// </summary>
    /// <summary>
    /// Gets the function that applies an ascending ordering to the query.
    /// Returns <c>null</c> when no ordering is specified.
    /// </summary>
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; }

    /// <summary>
    /// Gets the function that applies a descending ordering to the query.
    /// </summary>
    /// <summary>
    /// Gets the function that applies a descending ordering to the query.
    /// Returns <c>null</c> when no descending ordering is specified.
    /// </summary>
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderByDescending { get; }

    /// <summary>
    /// Indicates whether the query should be executed with <c>AsNoTracking</c> for read‑only scenarios.
    /// </summary>
    /// <summary>
    /// Indicates whether the query should be executed with <c>AsNoTracking</c> for read‑only scenarios.
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// Indicates whether global query filters (e.g., soft‑delete, multi‑tenant) should be ignored.
    /// </summary>
    /// <summary>
    /// Indicates whether global query filters (e.g., soft‑delete, multi‑tenant) should be ignored.
    /// </summary>
    bool IgnoreQueryFilters { get; }

    /// <summary>
    /// Enables EF Core split‑query execution to avoid cartesian explosion when including multiple collections.
    /// </summary>
    /// <summary>
    /// Enables EF Core split‑query execution to avoid cartesian explosion when including multiple collections.
    /// </summary>
    bool AsSplitQuery { get; }
}
