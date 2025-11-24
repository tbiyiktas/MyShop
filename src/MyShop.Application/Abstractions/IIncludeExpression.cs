namespace MyShop.Application.Abstractions;

/// <summary>
/// Represents an include expression that can be applied to a query.
/// Supports both simple Include and ThenInclude operations.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IIncludeExpression<TEntity> where TEntity : class
{
    /// <summary>
    /// Applies the include expression to the query.
    /// </summary>
    IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}
